using AuthService.Data;
using AuthService.Repositories;
using AuthService.Services;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// O segredo nao fica versionado: vem de user-secrets (dev) ou da variavel de
// ambiente JwtSettings__SecretKey. Falhar aqui e melhor que emitir tokens
// assinados com um segredo vazio.
var jwtSecret = builder.Configuration["JwtSettings:SecretKey"];

if (string.IsNullOrWhiteSpace(jwtSecret) || jwtSecret.Length < 32)
{
    throw new InvalidOperationException(
        "JwtSettings:SecretKey ausente ou com menos de 32 caracteres. " +
        "Configure via 'dotnet user-secrets set \"JwtSettings:SecretKey\" \"<segredo>\"' " +
        "ou pela variavel de ambiente JwtSettings__SecretKey.");
}

// Controllers
builder.Services.AddControllers();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.MigrationsHistoryTable("__MigrationsHistory_AuthService");
        }));

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Auth API",
        Version = "v1"
    });
});


// CORS: origens vem da configuracao. Lista vazia nao libera ninguem.
var origensPermitidas = builder.Configuration
    .GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Default", policy =>
    {
        policy.WithOrigins(origensPermitidas)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


// Rate limiting: o login e o alvo obvio de forca bruta. Particionado por IP.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("login", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1)
            }));
});


// Dependency Injection
// AddDbContext acima ja registra o AppDbContext com escopo; registra-lo de novo
// sobrepoe aquela configuracao.
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<TokenService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// A imagem aspnet define DOTNET_RUNNING_IN_CONTAINER. La so a porta HTTP e
// exposta, e sem porta HTTPS o middleware nao tem para onde redirecionar.
if (!builder.Configuration.GetValue<bool>("DOTNET_RUNNING_IN_CONTAINER"))
{
    app.UseHttpsRedirection();
}

app.UseCors("Default");

app.UseRateLimiter();

app.MapControllers();

app.Run();