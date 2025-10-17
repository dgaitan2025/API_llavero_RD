using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using ProyDesaWeb2025.Funciones;
using ProyDesaWeb2025.ModelosBP;
using ProyDesaWeb2025.Repositories;
using ProyDesaWeb2025.Security;
using ProyDesaWeb2025.Hubs; // <<--- Hub namespace
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Inyectar repos directamente
builder.Services.AddHttpClient<FaceApiClient>();

builder.Services.AddScoped<UsuariosRepository>();
builder.Services.AddSingleton<JwtTokenService>();

builder.Services.AddDbContext<DBDesWeb>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("MySqlConnectionRemote"),
        new MySqlServerVersion(new Version(8, 0, 40)) // versión de tu servidor MySQL
    )
);

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
        options.RequireHttpsMetadata = true;   // En local con HTTP puro, puedes poner false
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

        // <<--- Permitir token vía querystring para WebSockets / SignalR
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var path = context.HttpContext.Request.Path;
                if (path.StartsWithSegments("/hubs") &&
                    context.Request.Query.TryGetValue("access_token", out var token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// ===================== SignalR =====================
builder.Services.AddSignalR();
// ===================================================

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueClient",
        policy =>
        {
            policy.WithOrigins(
                    "http://localhost:3000",
                    "https://llaverostec.onrender.com",
                    "http://localhost:5173",
                    "https://localhost:5173"
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials(); // Necesario si pasas credenciales/token en websockets
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection();
}

// Servir archivos estáticos desde "Recursos" en la raíz del proyecto
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "Recursos")),
    RequestPath = "/Recursos"
});

app.UseHttpsRedirection();

// Usar CORS
app.UseCors("AllowVueClient");

// ===== Orden correcto para auth =====
app.UseAuthentication();
app.UseAuthorization();
// ====================================

// ===================== Endpoints =====================
// Hub de órdenes (ajusta el nombre/ruta si deseas)
app.MapHub<OrdenesHub>("/hubs/ordenes");

app.MapControllers();
// =====================================================

app.Run();