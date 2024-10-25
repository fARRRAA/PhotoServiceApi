

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PhotoServiceApi.dbContext;
using PhotoServiceApi.Interfaces;
using PhotoServiceApi.Models;
using PhotoServiceApi.Services;

namespace PhotoServiceApi
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

            builder.Services.AddScoped<IPhotoService>(provider =>
            {
                var context = provider.GetRequiredService<PhotoServiceDb>();

                var storagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "photos");

                return new PhotoService(storagePath, context);
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                    });
            });

            builder.Services.AddDbContext<PhotoServiceDb>(options =>
     options.UseSqlServer(builder.Configuration.GetConnectionString("TestDbString")), ServiceLifetime.Scoped);
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
