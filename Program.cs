// using webapi_80.src.Weather.Contracts;
// using webapi_80.src.Weather.Repositories.Weather;
// using webapi_80.src.Weather.Services;
using webapi_80.src.Shared.Contract;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using webapi_80.src.Shared.DatabaseContext;
using webapi_80.src.Tenant.SchemaTenant.SchemaContext;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using webapi_80.src.Tenant.SchemaTenant;
using Microsoft.AspNetCore.Identity;
using webapi_80.src.User.Models;
using webapi_80.src.Tenant.Service;

// IConfiguration Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

builder.Services.AddDbContext<ApplicationDbContext>(dbContextBuilder => dbContextBuilder.UseSqlServer(
                    builder.Configuration.GetConnectionString("MultiTenantBlog"))
                    .ReplaceService<IMigrationsAssembly, DbSchemaAwareMigrationAssembly>()
                    .ReplaceService<IModelCacheKeyFactory, DbSchemaAwareModelCacheKeyFactory>()
                    ).AddSingleton<IDbContextSchema>(new DbContextSchema());
builder.Services.AddScoped<IUnitofwork, Unitofwork>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
// builder.Services.AddSingleton<IConfiguration>(Configuration);
builder.Services.AddScoped<ITenantSchema, TenantSchema>();
builder.Services.AddScoped<TenantService>();
builder.Services.AddTransient<IPasswordHasher<UserModel>, PasswordHasher<UserModel>>();
// builder.Services.UseUrls(new string[] {"http://0.0.0.0:5043", "https://0.0.0.0:7207"});
builder.WebHost.UseUrls(new string[] {"http://0.0.0.0:5043", "https://0.0.0.0:7207"});
var app = builder.Build();
app.CustomEnginerInterceptor();
var scope = app.Services.CreateScope();
var tenantService = scope.ServiceProvider.GetService<TenantService>();
// var tenantService = app.Services.GetRequiredService<TenantService>();
tenantService.MigrateTenants();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAllOrigins");

// app.UseAuthorization();

app.MapControllers();
app.Run();