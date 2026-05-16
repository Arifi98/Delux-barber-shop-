using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using DeluxBarberShop.API.Data;
using DeluxBarberShop.API.Hubs;
using DeluxBarberShop.API.Options;
using DeluxBarberShop.API.Services;
using DeluxBarberShop.API.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Email"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(token) && context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                    context.Token = token;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy.WithOrigins("http://localhost:4200", "http://127.0.0.1:4200")
              .AllowAnyHeader().AllowAnyMethod().AllowCredentials());
});

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IBarberService, BarberService>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IEmailNotificationService, EmailNotificationService>();
builder.Services.AddHostedService<AppointmentReminderService>();

var app = builder.Build();

// Seed database on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        db.Database.Migrate();

        if (!db.Users.Any())
        {
            db.Users.Add(new DeluxBarberShop.API.Models.User
            {
                FirstName    = "Admin",
                LastName     = "Delux",
                Email        = "admin@deluxbarbershop.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Role         = "Admin",
                IsActive     = true
            });
        }

        if (!db.Services.Any())
        {
            var services = new[]
            {
                new DeluxBarberShop.API.Models.Service { Name = "Haircut",           Description = "Classic haircut, wash & style",     Price = 1500, DurationMinutes = 30, IsActive = true },
                new DeluxBarberShop.API.Models.Service { Name = "Beard Trim",        Description = "Shape & trim beard with precision",  Price = 800,  DurationMinutes = 20, IsActive = true },
                new DeluxBarberShop.API.Models.Service { Name = "Haircut + Beard",   Description = "Full combo: haircut & beard trim",   Price = 2000, DurationMinutes = 50, IsActive = true },
                new DeluxBarberShop.API.Models.Service { Name = "Fade",              Description = "Skin fade or taper fade",            Price = 1800, DurationMinutes = 40, IsActive = true },
                new DeluxBarberShop.API.Models.Service { Name = "Kids Haircut",      Description = "Haircut for kids under 12",         Price = 1000, DurationMinutes = 25, IsActive = true },
                new DeluxBarberShop.API.Models.Service { Name = "Deluxe Package",    Description = "Full grooming: cut, beard, styling", Price = 2800, DurationMinutes = 75, IsActive = true },
            };
            db.Services.AddRange(services);
        }

        if (!db.Barbers.Any())
        {
            var barbers = new[]
            {
                new DeluxBarberShop.API.Models.Barber { FirstName = "Arjon",  LastName = "Muça",    Specialty = "Fades & Classic Cuts",   Bio = "10 vjet eksperiencë. Specialist i fade dhe cut klasik.", IsActive = true },
                new DeluxBarberShop.API.Models.Barber { FirstName = "Blerim", LastName = "Hoxha",   Specialty = "Beard Styling",           Bio = "Ekspert i stilimit të mjekrës dhe lookeve moderne.",     IsActive = true },
                new DeluxBarberShop.API.Models.Barber { FirstName = "Erion",  LastName = "Kastrati", Specialty = "Kids & Family Cuts",     Bio = "Durim dhe profesionalizëm për çdo moshë.",              IsActive = true },
            };
            db.Barbers.AddRange(barbers);
        }

        db.SaveChanges();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Database seed failed.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<BarberShopHub>("/hubs/barbershop");

app.Run();
