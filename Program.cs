using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ShiftHandoverAPI.Data;
using ShiftHandoverAPI.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();

// Swagger with Authorize button
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "IOCL Panipat Refinery - Shift Handover API",
        Version = "v1",
        Description = "Shift Handover Management System API for IOCL Panipat Refinery"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Database - Render compatible
var dbPath = Environment.GetEnvironmentVariable("DB_PATH") ?? "/tmp/ShiftHandover.db";
Console.WriteLine($"📁 Database: {dbPath}");

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
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Shift Handover API v1");
    });
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Shift Handover API v1");
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

// ============================================================
// ✅ SEED ALL USERS - COMPLETE LIST
// ============================================================
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.EnsureCreated();
        Console.WriteLine("✅ Database created/ensured!");

        // Check if any users exist
        var userCount = dbContext.Users.Count();
        Console.WriteLine($"👥 Current users in database: {userCount}");

        // Only seed if no users exist
        if (userCount == 0)
        {
            Console.WriteLine("📝 Creating all users...");

            var users = new List<User>
            {
                // ============================================
                // 1. ADMIN
                // ============================================
                new User
                {
                    Name = "Admin User",
                    Email = "admin@iocl.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    Role = "Admin",
                    Department = "Management",
                    EmployeeId = "IOCL/A/000",
                    ShiftAssigned = "General",
                    Batch = "A",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                
                // ============================================
                // 2. G.C. Sikder - Admin 2
                // ============================================
                new User
                {
                    Name = "G.C. Sikder",
                    Email = "gc.sikder@iocl.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    Role = "Admin",
                    Department = "Management",
                    EmployeeId = "IOCL/ADM/002",
                    ShiftAssigned = "General",
                    Batch = "A",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                
                // ============================================
                // 3. Rahul Verma - Supervisor (CDU)
                // ============================================
                new User
                {
                    Name = "Rahul Verma",
                    Email = "rahul@iocl.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Super@123"),
                    Role = "Supervisor",
                    Department = "CDU/VDU - Crude Distillation",
                    EmployeeId = "IOCL/SUP/001",
                    ShiftAssigned = "Morning",
                    Batch = "B",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                
                // ============================================
                // 4. Anil Mehta - Supervisor (VDU)
                // ============================================
                new User
                {
                    Name = "Anil Mehta",
                    Email = "anil.mehta@iocl.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Super@123"),
                    Role = "Supervisor",
                    Department = "VDU - Vacuum Distillation",
                    EmployeeId = "IOCL/SUP/002",
                    ShiftAssigned = "Evening",
                    Batch = "B",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                
                // ============================================
                // 5. Priya Sharma - Supervisor (RFCCU)
                // ============================================
                new User
                {
                    Name = "Priya Sharma",
                    Email = "priya@iocl.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Super@123"),
                    Role = "Supervisor",
                    Department = "RFCCU",
                    EmployeeId = "IOCL/SUP/003",
                    ShiftAssigned = "Morning",
                    Batch = "B",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                
                // ============================================
                // 6. Amit Singh - Supervisor (DCU)
                // ============================================
                new User
                {
                    Name = "Amit Singh",
                    Email = "amit@iocl.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Super@123"),
                    Role = "Supervisor",
                    Department = "DCU",
                    EmployeeId = "IOCL/SUP/004",
                    ShiftAssigned = "Night",
                    Batch = "B",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                
                // ============================================
                // 7. Rajesh Kumar - Supervisor (OHCU)
                // ============================================
                new User
                {
                    Name = "Rajesh Kumar",
                    Email = "rajesh.kumar@iocl.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Super@123"),
                    Role = "Supervisor",
                    Department = "OHCU",
                    EmployeeId = "IOCL/SUP/005",
                    ShiftAssigned = "Morning",
                    Batch = "B",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                
                // ============================================
                // 8. Priya Sharma - Supervisor (HCU)
                // ============================================
                new User
                {
                    Name = "Priya Sharma",
                    Email = "priya.sharma@iocl.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Super@123"),
                    Role = "Supervisor",
                    Department = "HCU",
                    EmployeeId = "IOCL/SUP/006",
                    ShiftAssigned = "Evening",
                    Batch = "B",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                
                // ============================================
                // 9. Vikram Singh - Supervisor (DHDS)
                // ============================================
                new User
                {
                    Name = "Vikram Singh",
                    Email = "vikram.singh@iocl.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Super@123"),
                    Role = "Supervisor",
                    Department = "DHDS",
                    EmployeeId = "IOCL/SUP/007",
                    ShiftAssigned = "Morning",
                    Batch = "B",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                
                // ============================================
                // 10. Rakesh Yadav - Supervisor (CCRU)
                // ============================================
                new User
                {
                    Name = "Rakesh Yadav",
                    Email = "rakesh.yadav@iocl.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Super@123"),
                    Role = "Supervisor",
                    Department = "CCRU",
                    EmployeeId = "IOCL/SUP/008",
                    ShiftAssigned = "General",
                    Batch = "B",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                
                // ============================================
                // 11. Deepak Kumar - Supervisor (NCU)
                // ============================================
                new User
                {
                    Name = "Deepak Kumar",
                    Email = "deepak.kumar@iocl.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Super@123"),
                    Role = "Supervisor",
                    Department = "NCU",
                    EmployeeId = "IOCL/SUP/009",
                    ShiftAssigned = "Morning",
                    Batch = "B",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                
                // ============================================
                // 12. Manish Arora - Supervisor (PP Unit)
                // ============================================
                new User
                {
                    Name = "Manish Arora",
                    Email = "manish.arora@iocl.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Super@123"),
                    Role = "Supervisor",
                    Department = "PP Unit",
                    EmployeeId = "IOCL/SUP/010",
                    ShiftAssigned = "Evening",
                    Batch = "B",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                
                // ============================================
                // 13. Tarun Malik - Supervisor (HDPE)
                // ============================================
                new User
                {
                    Name = "Tarun Malik",
                    Email = "tarun.malik@iocl.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Super@123"),
                    Role = "Supervisor",
                    Department = "HDPE",
                    EmployeeId = "IOCL/SUP/011",
                    ShiftAssigned = "Night",
                    Batch = "B",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                
                // ============================================
                // 14. Sandeep Rana - Supervisor (PX/PTA)
                // ============================================
                new User
                {
                    Name = "Sandeep Rana",
                    Email = "sandeep.rana@iocl.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Super@123"),
                    Role = "Supervisor",
                    Department = "PX/PTA",
                    EmployeeId = "IOCL/SUP/012",
                    ShiftAssigned = "Morning",
                    Batch = "B",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                
                // ============================================
                // 15. Pankaj Gupta - Supervisor (Tank Farms)
                // ============================================
                new User
                {
                    Name = "Pankaj Gupta",
                    Email = "pankaj.gupta@iocl.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Super@123"),
                    Role = "Supervisor",
                    Department = "Tank Farms",
                    EmployeeId = "IOCL/SUP/017",
                    ShiftAssigned = "General",
                    Batch = "B",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                
                // ============================================
                // 16. Vipin Kumar - Supervisor (Lab-QC)
                // ============================================
                new User
                {
                    Name = "Vipin Kumar",
                    Email = "vipin.kumar@iocl.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Super@123"),
                    Role = "Supervisor",
                    Department = "Laboratory - QC",
                    EmployeeId = "IOCL/SUP/018",
                    ShiftAssigned = "Morning",
                    Batch = "B",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                
                // ============================================
                // 17. Suresh Mehta - Supervisor (General)
                // ============================================
                new User
                {
                    Name = "Suresh Mehta",
                    Email = "suresh.mehta@iocl.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Super@123"),
                    Role = "Supervisor",
                    Department = "General Operations",
                    EmployeeId = "IOCL/SUP/019",
                    ShiftAssigned = "General",
                    Batch = "B",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                }
            };

            dbContext.Users.AddRange(users);
            dbContext.SaveChanges();
            Console.WriteLine($"✅ {users.Count} users created successfully!");

            // Print all users
            foreach (var user in users)
            {
                Console.WriteLine($"   👤 {user.Name} - {user.Email} ({user.Role})");
            }
        }
        else
        {
            Console.WriteLine($"✅ Users already exist: {userCount} users");

            // Print existing users
            var existingUsers = dbContext.Users.ToList();
            foreach (var user in existingUsers)
            {
                Console.WriteLine($"   👤 {user.Name} - {user.Email} ({user.Role})");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Database error: {ex.Message}");
        Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
    }
}

app.MapGet("/", async context =>
{
    context.Response.Redirect("/Index");
});

app.Run();