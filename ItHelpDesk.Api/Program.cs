using ItHelpDesk.Api.Auth;
using ItHelpDesk.Api.Data;
using ItHelpDesk.Api.Data.Entities;
using ItHelpDesk.Api.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var connectionString = builder.Configuration.GetConnectionString("ItHelpDesk");
builder.Services.AddDbContext<ItHelpDeskDbContext>(o =>
    o.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 4, 0))));

builder.Services.AddIdentity<ItHelpDeskUser, IdentityRole>()
    .AddEntityFrameworkStores<ItHelpDeskDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddTransient<AuthSeeder>();
builder.Services.AddTransient<JwtTokenService>();
builder.Services.AddScoped<SessionService>();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddScoped<ICategoriesRepository, CategoriesRepository>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.MapInboundClaims = false;
    options.TokenValidationParameters.ValidateIssuer = true;
    options.TokenValidationParameters.ValidIssuer = builder.Configuration["Jwt:ValidIssuer"];
    options.TokenValidationParameters.ValidateAudience = true;
    options.TokenValidationParameters.ValidAudience = builder.Configuration["Jwt:ValidAudience"];
    options.TokenValidationParameters.ValidateLifetime = true;
    options.TokenValidationParameters.IssuerSigningKey =
        new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!));
});
builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });
    o.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<ItHelpDeskDbContext>().Database.Migrate();
    await scope.ServiceProvider.GetRequiredService<AuthSeeder>().SeedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
