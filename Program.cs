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
                    "http://localhost:5173",
                    "https://localhost:5173",
                    "https://llaverostec.onrender.com",
                    "https://tecllaveros.onrender.com",
					"https://www.teckeygt.com",
                    "https://teckeygt.com"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();    
}

app.UseHttpsRedirection();

// Archivos estáticos desde **wwwroot/**
app.UseStaticFiles();


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

// ======== Asegurar estructura wwwroot ========
// ======== Asegurar estructura wwwroot y diagnosticar Recursos ========

// Si el WebRootPath no existe (por ejemplo, en Render), se define manualmente
if (string.IsNullOrEmpty(app.Environment.WebRootPath))
{
    app.Environment.WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
}

// Crear carpeta wwwroot si no existe
if (!Directory.Exists(app.Environment.WebRootPath))
{
    Directory.CreateDirectory(app.Environment.WebRootPath);
}

// Crear subcarpetas necesarias dentro de wwwroot
var rutasNecesarias = new[]
{
    Path.Combine(app.Environment.WebRootPath, "recursos"),
    Path.Combine(app.Environment.WebRootPath, "recursos", "imgs"),
    Path.Combine(app.Environment.WebRootPath, "recursos", "pdfs")
};

foreach (var ruta in rutasNecesarias)
{
    if (!Directory.Exists(ruta))
        Directory.CreateDirectory(ruta);
}

// ======== Diagnóstico: verificar carpeta Recursos en el contenedor ========

string recursosPath = Path.Combine(Directory.GetCurrentDirectory(), "Recursos");

if (Directory.Exists(recursosPath))
{
    Console.WriteLine($"✅ Carpeta 'Recursos' encontrada en: {recursosPath}");

    // Mostrar subdirectorios
    var subdirs = Directory.GetDirectories(recursosPath);
    if (subdirs.Length > 0)
    {
        Console.WriteLine("📁 Subcarpetas:");
        foreach (var sub in subdirs)
            Console.WriteLine($"   - {Path.GetFileName(sub)}");
    }

    // Mostrar archivos directos
    var archivos = Directory.GetFiles(recursosPath);
    if (archivos.Length > 0)
    {
        Console.WriteLine("📄 Archivos dentro de Recursos:");
        foreach (var file in archivos)
            Console.WriteLine($"   - {Path.GetFileName(file)}");
    }

    // Mostrar archivos dentro de subcarpetas (por ejemplo IMGS o PDFS)
    Console.WriteLine("🔍 Explorando subcarpetas:");
    foreach (var sub in subdirs)
    {
        var archivosSub = Directory.GetFiles(sub);
        Console.WriteLine($"   📂 {Path.GetFileName(sub)} ({archivosSub.Length} archivos)");
        foreach (var f in archivosSub)
            Console.WriteLine($"      - {Path.GetFileName(f)}");
    }
}
else
{
    Console.WriteLine($"❌ Carpeta 'Recursos' NO encontrada en: {recursosPath}");
}



// ============================================================

app.Run();