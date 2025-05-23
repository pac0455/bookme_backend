using System.Security.Claims;
using System.Text;
using bookme_backend;
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
using bookme_backend.UI;
using bookme_backend.Services;
using Microsoft.Extensions.FileProviders;

namespace bookme_backend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

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
            builder.Services.AddControllers(options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes=true);
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
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


            // Registrar servicios
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
           

            builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            builder.Services.AddScoped<IUsuarioService, UsuarioService>();
            builder.Services.AddScoped<IHorarioService, HorarioService>();
            builder.Services.AddScoped<IServicioService, ServicioService>();


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

            app.Run();


        }
    }
}
