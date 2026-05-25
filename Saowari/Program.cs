using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using Saowari.Data;
using Saowari.Extensions;
using Saowari.Services;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ── Controllers ───────────────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<SaowariDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("appCon")));

// ── AutoMapper ────────────────────────────────────────────────────────────────
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<AutoMapperProfile>());

// ── HTTP Context ──────────────────────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();

// ── Application Services ──────────────────────────────────────────────────────
builder.Services.AddApplicationServices();
builder.Services.AddSignalR();
builder.Services.AddHostedService<Saowari.Services.ChatCleanupService>();

// ── CORS ──────────────────────────────────────────────────────────────────────
// Configured to allow credentials specifically for real-time SignalR clients
builder.Services.AddCors(options =>
{
    options.AddPolicy("SaowariPolicy", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ── JWT Authentication ────────────────────────────────────────────────────────
var jwtSetting = builder.Configuration.GetSection("jwtSetting");
var key = Encoding.UTF8.GetBytes(jwtSetting["key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSetting["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSetting["Audience"],
        ClockSkew = TimeSpan.Zero
    };
});

// ── Authorization Policies ────────────────────────────────────────────────────
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly",    policy => policy.RequireRole("Admin"));
    options.AddPolicy("AgentOrAdmin", policy => policy.RequireRole("Agent", "Admin"));
    options.AddPolicy("CustomerOnly", policy => policy.RequireRole("Customer"));
    options.AddPolicy("DriverOnly",   policy => policy.RequireRole("Driver"));
    options.AddPolicy("AnyStaff",     policy => policy.RequireRole("Admin", "Agent", "Supervisor", "Driver"));
    options.AddPolicy("AdminOrManager", policy => policy.RequireRole("Admin", "CompanyManager"));
    options.AddPolicy("ManagerOrSupervisor", policy => policy.RequireRole("Admin", "CompanyManager", "Supervisor"));
});

// ─────────────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── Middleware Pipeline (ORDER IS CRITICAL) ───────────────────────────────────

// 1. CORS — must be FIRST, before UseAuthentication / UseAuthorization
app.UseCors("SaowariPolicy");

// 2. Serve static files (with explicit CORS headers so images can be fetched cross-origin for PDF generation)
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Headers", "*");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Methods", "GET");
    }
});

// 3. Seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SaowariDbContext>();
    await DataSeeder.SeedAsync(context);
}

// 3. Swagger (development only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Saowari API v1");
    });
}

// 4. Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// 5. Map Controllers & Hubs
app.MapControllers();
app.MapHub<Saowari.Hubs.ChatHub>("/chatHub");

app.Run();
