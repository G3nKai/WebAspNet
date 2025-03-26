//C:\Users\Shaphran\Desktop\backend 2 attempt\webNET-Retake_2025_spring\backendRetake
using Microsoft.EntityFrameworkCore;
using backendRetake.Models;
using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = AuthOptions.ISSUER,

            ValidateAudience = true,
            ValidAudience = AuthOptions.AUDIENCE,

            ValidateLifetime = true,

            IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
            ValidateIssuerSigningKey = true,
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddSwaggerGen(options => {
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Please put your token here",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});

WebApplication app = builder.Build();

app.MapControllers();

app.UseSwaggerUI();

app.UseSwagger(x => x.SerializeAsV2 = true);

app.UseAuthentication();
app.UseAuthorization();

app.Run();
