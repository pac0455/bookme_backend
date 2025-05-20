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
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.General;

namespace bookme_backend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // JWT Authentication
            // JWT Authentication
            _ = builder.Services.AddAuthentication(options =>
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
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
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
            builder.Services.AddControllers();
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
            //Usuario
            builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            builder.Services.AddScoped<IUsuarioService, UsuarioService>();
            // Horarios
            builder.Services.AddScoped<IHorarioRepository, HorarioRepository>();


            // Registra primero la implementación concreta como Singleton
            builder.Services.AddScoped<ICustomEmailSender, EmailSender>();



            //Hasher
            builder.Services.AddScoped<IPasswordHelper, PasswordHelper>();
            //IDENTITY USER
            builder.Services.AddIdentity<Usuario, IdentityRole>(options =>
            {
                // Configura las políticas de contraseñas
                options.Password.RequireDigit = true; // Requiere al menos un dígito
                options.Password.RequireLowercase = true; // Requiere al menos una minúscula
                options.Password.RequireUppercase = true; // Requiere al menos una mayúscula
                options.Password.RequireNonAlphanumeric = false; // Requiere un carácter no alfanumérico
                options.Password.RequiredLength = 6; // Longitud mínima de la contraseña
                options.Password.RequiredUniqueChars = 0; // Número de caracteres únicos requeridos

                // Política de bloqueo de cuenta
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5; // Máximo de intentos fallidos
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15); // Tiempo de bloqueo
                options.Lockout.MaxFailedAccessAttempts = 5; // Número de intentos fallidos antes de bloquear la cuenta

                // Política de confirmación de correo electrónico
                options.User.RequireUniqueEmail = true; // Requiere que el correo electrónico sea único
                options.SignIn.RequireConfirmedAccount = false; // Importante para recuperación

            })
            .AddErrorDescriber<IdentityErrorDescriberEs>() // 👈 Aquí cambiamos el idioma
            .AddEntityFrameworkStores<BookmeContext>()
            .AddDefaultTokenProviders();





            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseRouting();  // Esto es absolutamente crítico

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            //app.MapIdentityApi<Usuario>();  // Debe ir después de UseRouting

            app.Run();


        }
    }
}
