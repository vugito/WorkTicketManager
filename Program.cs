
using Microsoft.EntityFrameworkCore;
using WorkTicketManager.Data;
using WorkTicketManager.Models;

namespace HelpdeskWM
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // --- Add services ---
            builder.Services.AddDbContext<WMDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
            );

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    // В разработке можно AllowAnyOrigin, на продакшене лучше ограничить домен фронтенда
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            var app = builder.Build();

            // --- Apply migrations & seed database ---
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WMDbContext>();
                try
                {
                    db.Database.Migrate();
                    SeedDatabase(db);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка при миграции или seed данных: " + ex.Message);
                    // На продакшене возможно нужно логировать и продолжить работу
                }
            }

            // --- Middleware ---
            app.UseHttpsRedirection();
            app.UseCors("AllowFrontend");

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();

            app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));
            app.MapControllers();

            app.Run();
        }

        // --- Метод для заполнения начальных данных ---
        private static void SeedDatabase(WMDbContext db)
        {
            // Departments
            if (!db.Departments.Any())
            {
                db.Departments.AddRange(
                    new Department { Name = "Терапия" },
                    new Department { Name = "Хирургия" },
                    new Department { Name = "Регистратура" },
                    new Department { Name = "Кардиология" },
                    new Department { Name = "Администрация" }
                );
                db.SaveChanges();
            }

            // Users
            if (!db.Users.Any())
            {
                db.Users.AddRange(
                    new User { FullName = "Иванов Иван", Phone = "123456789", DepartmentId = 1, IsActive = true },
                    new User { FullName = "Петров Пётр", Phone = "987654321", DepartmentId = 2, IsActive = true },
                    new User { FullName = "Сидоров Сидор", Phone = "555666777", DepartmentId = 3, IsActive = true }
                );
                db.SaveChanges();
            }

            // Priorities
            if (!db.Priorities.Any())
            {
                db.Priorities.AddRange(
                    new Priority { Name = "Низкий", IsActive = true },
                    new Priority { Name = "Средний", IsActive = true },
                    new Priority { Name = "Высокий", IsActive = true }
                );
                db.SaveChanges();
            }

            // Statuses
            if (!db.Statuses.Any())
            {
                db.Statuses.AddRange(
                    new Status { Name = "NEW", Code = "NEW" },
                    new Status { Name = "IN_PROGRESS", Code = "IN_PROGRESS" },
                    new Status { Name = "CLOSED", Code = "CLOSED" },
                    new Status { Name = "DENIED", Code = "DENIED" }
                );
                db.SaveChanges();
            }
        }
    }
}
