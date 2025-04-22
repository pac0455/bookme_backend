using bookme_backend;
using bookme_backend.BLL.Interfaces;
using bookme_backend.BLL.Services;
using bookme_backend.DataAcces.Repositories.Implementation;
using bookme_backend.DataAcces.Repositories.Interfaces;

namespace bookme_backend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Registrar servicios
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            //Usuario
            builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            builder.Services.AddScoped<IUsuarioService, UsuarioService>();


            var app = builder.Build();
            
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();


            app.Run();
        }
    }
}
