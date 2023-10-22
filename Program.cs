using webapi_80.src.Shared.Contract;
using webapi_80.src.Shared.DatabaseContext;
using webapi_80.src.Tenant.SchemaTenant.SchemaContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using webapi_80.src.Tenant.SchemaTenant;
using Microsoft.AspNetCore.Identity;
using webapi_80.src.User.Models;
using webapi_80.src.Tenant.Service;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using webapi_80.src.Shared.Utilities;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json");

// ===== Add Jwt Authentication ========
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(cfg =>
  {
      cfg.RequireHttpsMetadata = false;
      cfg.SaveToken = true;
      cfg.TokenValidationParameters = new TokenValidationParameters
      {
          ValidIssuer = JwtokenOptions.Issuer,
          ValidAudience = JwtokenOptions.Issuer,
          IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtokenOptions.Key)),
          ClockSkew = TimeSpan.Zero // remove delay of token when expire
      };
  });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SubdomainPolicy", policy =>
    {
        policy.RequireAssertion(context =>
        {
            
            TenantSchema tenantSchema = new TenantSchema();
            var httpContext = (DefaultHttpContext) context.Resource;
            if (httpContext != null)
            {
                var currentSubdomain = tenantSchema.ExtractSubdomainFromRequest(httpContext);

                // Get the claim from the user's identity
                string? groupSidClaim = context.User.FindFirst(ClaimTypes.GroupSid)?.Value;

                // Check if the subdomain from the claim matches the current subdomain
                return currentSubdomain == groupSidClaim;
            }
            else
            {
                return true;
            }

        });
    });
});


builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
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
            new string[] { }
        }
    });
});



builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<ApplicationDbContext>(dbContextBuilder => dbContextBuilder.UseSqlServer(
                    builder.Configuration.GetConnectionString("MultiTenantBlog"))
                    .ReplaceService<IMigrationsAssembly, DbSchemaAwareMigrationAssembly>()
                    .ReplaceService<IModelCacheKeyFactory, DbSchemaAwareModelCacheKeyFactory>()
                    ).AddSingleton<IDbContextSchema>(new DbContextSchema());
builder.Services.AddScoped<IUnitofwork, Unitofwork>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<ITenantSchema, TenantSchema>();
builder.Services.AddScoped<TenantService>();
builder.Services.AddTransient<IPasswordHasher<UserModel>, PasswordHasher<UserModel>>();
builder.WebHost.UseUrls(new string[] { "http://0.0.0.0:5043", "https://0.0.0.0:7207" });
var app = builder.Build();
app.CustomEnginerInterceptor();
var scope = app.Services.CreateScope();
var tenantService = scope.ServiceProvider.GetService<TenantService>();
tenantService.MigrateTenants();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAllOrigins");

app.MapControllers();
app.Run();

/**
- Db connection string *
- DbSchemaAwareMigrationAssembly *
- DbSchemaAwareModelCacheKeyFactory *
- Migration file *
- DbContextSchema *
- Unitofwork *
- TenantSchema
- TenantService *
- UseUrls *
- /etc/hosts *
- CustomEnginerInterceptor *
- MigrateTenants *
- user controller or blog (something showing the migrations in use)
*/