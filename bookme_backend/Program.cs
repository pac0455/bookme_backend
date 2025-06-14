using System.Security.Claims;
using System.Text;
using bookme_backend.BLL.Interfaces;
using bookme_backend.BLL.Services;
using bookme_backend.DataAcces.Models;
using bookme_backend.DataAcces.Repositories.Implementation;
using bookme_backend.DataAcces.Repositories.Interfaces;
using FirebaseAdmin;
using Google;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using bookme_backend.Services;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.DependencyInjection;

using bookme_backend.DataAcces.DTO.Google;
using System.Text.Json.Serialization;
using bookme_backend.DataAcces.Seeds;

namespace bookme_backend
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            //Crear roles


            // JWT Authentication
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),

                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = ClaimTypes.NameIdentifier
                };
            });

            //Configurar firebase auth
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile("bookme-firebaseKeys.json"),
            });
            //Configurar conexion
            builder.Services.AddDbContext<BookmeContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Add services to the container.
            builder.Services
                .AddControllers(options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true)
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.OperationFilter<FileUploadOperationFilter>();
                c.SwaggerDoc("v1", new() { Title = "Bookme API", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new()
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Escriba su token JWT aquí. Ejemplo: Bearer {token}"
                });

                c.AddSecurityRequirement(new()
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });

            });

            //APi key
            builder.Services.Configure<GoogleMapsSettings>(builder.Configuration.GetSection("GoogleMaps"));
            builder.Services.AddHttpClient<NegocioService>();



            // Registrar servicios
            builder.Services.AddSingleton<AuthCodeStore>();
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));


            builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            builder.Services.AddScoped<IUsuarioService, UsuarioService>();
            builder.Services.AddScoped<IHorarioService, HorarioService>();
            builder.Services.AddScoped<IServicioService, ServicioService>();
            builder.Services.AddScoped<ICategoriaService, CategoriaService>();
            builder.Services.AddScoped<IReservaService, ReservaService>();
            builder.Services.AddScoped<IValoracionesService, ValoracionesService>();
            builder.Services.AddScoped<IPasarelaSimulada, PasarelaService>();
            builder.Services.AddScoped<INegocioService, NegocioService>();
            builder.Services.AddScoped<ICustomEmailSender, EmailSender>();


            builder.Logging.AddDebug();
            builder.Logging.AddConsole();
            // Registra primero la implementación concreta como Singleton
            //Hasher
            builder.Services.AddScoped<IPasswordHelper, PasswordHelper>();
            //IDENTITY USER
            builder.Services.AddIdentityCore<Usuario>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole>()
            .AddErrorDescriber<IdentityErrorDescriberEs>()
            .AddEntityFrameworkStores<BookmeContext>()
            .AddDefaultTokenProviders();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseStaticFiles(); // Esto habilita wwwroot (por defecto)

            // Sirve archivos de .well-known con tipo JSON (App Links)
            var fileRoute = Path.Combine(Directory.GetCurrentDirectory(), "UI", "wwwroot", ".well-known");
            Console.Write(fileRoute);
            Console.WriteLine($"[INIT] .well-known path: {fileRoute}");
            app.UseStaticFiles(new StaticFileOptions
            {
               
                FileProvider = new PhysicalFileProvider(fileRoute),
                RequestPath = "/.well-known",
                ServeUnknownFileTypes = true,
                DefaultContentType = "application/json"
            });

            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }


            // Habilita servir archivos desde la carpeta 'uploads'
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "uploads")),
                RequestPath = "/uploads"
            });
            app.UseHttpsRedirection();
            app.UseRouting();  // Esto es absolutamente crítico

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();


            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                try
                {
                    // Aplica migraciones pendientes en la base de datos
                    var dbContext = services.GetRequiredService<BookmeContext>();
                    dbContext.Database.Migrate();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error aplicando migraciones: {ex.Message}");
                }

                // Sembrar roles al iniciar la aplicación
                try
                {
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                    await IdentitySeeder.SeedRolesAsync(roleManager);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al sembrar roles: {ex.Message}");
                }
                try
                {
                    var userManager = services.GetRequiredService<UserManager<Usuario>>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                    var usuarioService = services.GetRequiredService<IUsuarioService>();
                    await usuarioService.CreateAdminUserAsync(userManager, roleManager);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al sembrar datos de identidad: {ex.Message}");
                }

                app.Run();
            }
        }
    }
}