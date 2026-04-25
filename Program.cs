using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WorkTicketManager.Data;
using WorkTicketManager.Models;
using WorkTicketManager.Services;

namespace HelpdeskWM
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // --- Database ---
            builder.Services.AddDbContext<WMDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
            );

            // --- JWT Service ---
            builder.Services.AddScoped<JwtService>();

            // --- Authentication ---
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
                    };
                });

            builder.Services.AddAuthorization();

            builder.Services.AddControllers();

            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = 429;
                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = 429;
                    context.HttpContext.Response.ContentType = "application/json";
                    await context.HttpContext.Response.WriteAsync(
                        "{\"error\": \"Слишком много заявок. Подождите минуту и попробуйте снова.\"}",
                        token);
                };
                options.AddFixedWindowLimiter("tickets", limiter =>
                {
                    limiter.PermitLimit = 2;
                    limiter.Window = TimeSpan.FromMinutes(1);
                    limiter.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
                    limiter.QueueLimit = 0;
                });
            });


            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Введите: Bearer {токен}"
                });
                c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
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
                        Array.Empty<string>()
                    }
                });
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            var app = builder.Build();

            // --- Migrate & Seed ---
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
                    Console.WriteLine("Ошибка при миграции или seed: " + ex.Message);
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

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseRateLimiter();

            app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));
            app.MapControllers();

            app.Run();
        }

        private static void SeedDatabase(WMDbContext db)
        {
            // Company
            if (!db.Companies.Any())
            {
                db.Companies.Add(new Company
                {
                    Name = "Больница",
                    Description = "Городская больница",
                    IsActive = true
                });
                db.SaveChanges();
            }

            var company = db.Companies.First();

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

            // Departments
            if (!db.Departments.Any())
            {
                db.Departments.AddRange(
                    new Department { Name = "Терапия", CompanyId = company.Id },
                    new Department { Name = "Хирургия", CompanyId = company.Id },
                    new Department { Name = "Регистратура", CompanyId = company.Id },
                    new Department { Name = "Кардиология", CompanyId = company.Id },
                    new Department { Name = "Администрация", CompanyId = company.Id }
                );
                db.SaveChanges();
            }

            // SuperAdmin
            if (!db.AppUsers.Any(u => u.SystemRole == SystemRole.SuperAdmin))
            {
                db.AppUsers.Add(new AppUser
                {
                    Username = "superadmin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    FullName = "Super Admin",
                    SystemRole = SystemRole.SuperAdmin,
                    CompanyId = null,
                    IsActive = true
                });
                db.SaveChanges();
            }
        }
    }
}