using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ShiftHandoverAPI.Data;
using ShiftHandoverAPI.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ FIX: Use /tmp folder for Render
// Render par /tmp folder mein write permission hota hai
var dbPath = "/tmp/ShiftHandover.db";
Console.WriteLine($"📁 Database path: {dbPath}");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// JWT Authentication
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "YourSuperSecretKeyHere12345678901234567890");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "ShiftHandoverAPI",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "ShiftHandoverClient",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Production ke liye
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

// ✅ FIX: Database seed - Admin user create karo
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Database create karo
        dbContext.Database.EnsureCreated();
        Console.WriteLine("✅ Database created/ensured!");

        // Check if admin exists
        if (!dbContext.Users.Any())
        {
            Console.WriteLine("📝 Creating admin user...");
            var admin = new User
            {
                Name = "Admin User",
                Email = "admin@iocl.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = "Admin",
                Department = "Management",
                EmployeeId = "IOCL/ADMIN/001",
                ShiftAssigned = "General",
                Batch = "A",
                IsActive = true,
                CreatedAt = DateTime.Now
            };
            dbContext.Users.Add(admin);
            dbContext.SaveChanges();
            Console.WriteLine("✅ Admin user created!");
        }
        else
        {
            Console.WriteLine("✅ Admin user already exists.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Database error: {ex.Message}");
        // Error log karo but app crash mat karo
    }
}

app.MapGet("/", async context =>
{
    context.Response.Redirect("/Index");
});

app.Run();