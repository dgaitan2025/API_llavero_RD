using System.Text;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ProyDesaWeb2025.Repositories;
using ProyDesaWeb2025.Security;
using Api_Empleados.Funciones;
using ProyDesaWeb2025.Funciones;




var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<EnvioCorreo>();
builder.Services.AddScoped<TwilioMsg>();
builder.Services.AddScoped<CarnetGenerador>();
builder.Services.AddScoped<EnvioMensajes>(); // 👈 Aquí ya puedes inyectar IConfiguration

// Inyectar repos directamente
builder.Services.AddScoped<UsuariosRepository>();
builder.Services.AddSingleton<JwtTokenService>();

// ===================== JWT =====================
// Lee configuración desde appsettings.json -> "Jwt"
var jwtSection = builder.Configuration.GetSection("Jwt");
var issuer   = jwtSection.GetValue<string>("Issuer")   ?? "ProyDesaWeb2025";
var audience = jwtSection.GetValue<string>("Audience") ?? "ProyDesaWeb2025.Frontend";
var key      = jwtSection.GetValue<string>("Key") 
               ?? throw new InvalidOperationException("Configura Jwt:Key en appsettings.json");

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = issuer,
            ValidAudience            = audience,
            IssuerSigningKey         = signingKey,
            ClockSkew                = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();
// ==============================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("ANY",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueClient",
        policy =>
        {
            policy.WithOrigins(
                    "http://localhost:3000",
                    "https://llaverostec.onrender.com",
					"https://tecllaveros.onrender.com"
                )
                .AllowAnyHeader()
                .AllowAnyMethod();
                // .AllowCredentials(); // Descomenta si tu front realmente envía credenciales (cookies)
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Servir archivos estáticos desde "Recursos" en la raíz del proyecto
var recursosPath = Path.Combine(builder.Environment.ContentRootPath, "Recursos");
if (!Directory.Exists(recursosPath))
{
    Directory.CreateDirectory(recursosPath); // evita el DirectoryNotFoundException
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(recursosPath),
    RequestPath = "/Recursos/PDFS"
});


app.UseHttpsRedirection();

// Usar CORS
app.UseCors("AllowVueClient");

// ===== Orden correcto para auth =====
app.UseAuthentication();
app.UseAuthorization();
// ====================================

app.MapControllers();

app.Run();