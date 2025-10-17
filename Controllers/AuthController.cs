using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyDesaWeb2025.Funciones;
using ProyDesaWeb2025.ModelosBP;
using ProyDesaWeb2025.Models;
using ProyDesaWeb2025.Models.Dto;
using ProyDesaWeb2025.Repositories;
using ProyDesaWeb2025.Security;
using System.Diagnostics;

namespace ProyDesaWeb2025.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UsuariosRepository _repo;
    private readonly IConfiguration _cfg;
    private readonly JwtTokenService _jwt;
    private readonly DBDesWeb _context;

    public AuthController(UsuariosRepository repo, IConfiguration cfg, JwtTokenService jwt, DBDesWeb context)
    {
        _repo = repo;
        _cfg = cfg;
        _jwt = jwt;
        _context = context;
    }

    /// <summary>
    /// Inicia sesión de usuario y devuelve un token JWT si las credenciales son válidas.
    /// </summary>
    /// <param name="body">Objeto que contiene el identificador y la contraseña del usuario.</param>
    /// <returns>
    /// Devuelve una respuesta con los datos del usuario y el token JWT si el inicio de sesión es exitoso.
    /// Si las credenciales son inválidas o faltan datos obligatorios, retorna un error adecuado.
    /// </returns>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest body)
    {
        if (body is null || string.IsNullOrWhiteSpace(body.Identificador) || string.IsNullOrEmpty(body.Password))
            return BadRequest("Identificador y contraseña son obligatorios.");

        UsuarioCredencial? u = await _repo.ObtenerPorCredencialAsync(body.Identificador.Trim());

        if (u is null || !u.EstaActivo)
            return Unauthorized(new { success = false, message = "Usuario no existe." });

        // Verificación simple: BCrypt ($2a/$2b$) y (opcional) Argon2id si agregas el paquete.
        var ok = PasswordHasher.Verify(body.Password, u.PasswordHash);
        if (!ok)
            return Unauthorized(new { success = false, message = "Credenciales inválidas." });
        // verificamos que exista en nuestra base de datos
        

        var usuario = await _context.usuarios.FirstOrDefaultAsync(x => x.email == u.Email || x.Telefono == u.Telefono);
        usuario usuarioLocal;
        if (usuario == null)
        {
            // Si no existe, lo agregamos
            usuarioLocal = new usuario
            {
                email = u.Email,
                Telefono = u.Telefono,                
                nickname = u.Nickname,
                PasswordHash = u.PasswordHash,
                RolId = u.RolId,
                EstaActivo = u.EstaActivo,
                Fotografia2 = u.Fotografia2,
                Fotografia2Mime = u.Fotografia2Mime
            };
            _context.usuarios.Add(usuarioLocal);
            await _context.SaveChangesAsync();
        }
        else
        {
            usuarioLocal = usuario;
        }


        var token = _jwt.CreateToken(u);
        return Ok(new LoginResponse
        {
            UsuarioId = usuarioLocal.UsuarioId,
            Email = usuarioLocal.email,
            Telefono = usuarioLocal.Telefono,
            Nickname = usuarioLocal.nickname,
            RolId = usuarioLocal.RolId,
            EstaActivo = true,
            Token = token,
            Fotografia2 = usuarioLocal.Fotografia2 != null ? $"data:{usuarioLocal.Fotografia2Mime};base64,{Convert.ToBase64String(usuarioLocal.Fotografia2)}" : null
        });
    }

    /// <summary>
    /// Inicia sesión mediante reconocimiento facial.
    /// </summary>
    /// <param name="identificador">Identificador único del usuario (correo, teléfono o nickname).</param>
    /// <param name="foto">Fotografía del rostro enviada por el cliente para validar la identidad.</param>
    /// <returns>
    /// Devuelve una respuesta con los datos del usuario y el token JWT si el rostro es reconocido correctamente.<br/>
    /// Si faltan datos, retorna un error 400.<br/>
    /// Si el usuario no existe, está inactivo, no hay fotos registradas o el rostro no es reconocido, retorna un error 401.<br/>
    /// Si ocurre un error al obtener las fotos, retorna el código y mensaje correspondiente.
    /// </returns>
    [HttpPost("login-face")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LoginFace([FromBody] LoginFaceRequest form, [FromServices] FaceApiClient _face)
    {
        if (form is null || string.IsNullOrWhiteSpace(form.Identificador) || form.Foto is null || form.Foto.Length == 0)
            return BadRequest(new { success = false, message = "Debe enviar 'identificador' y una 'foto' válida." });

        var u = await _repo.ObtenerPorCredencialAsync(form.Identificador.Trim());
        if (u is null || !u.EstaActivo)
            return Unauthorized(new { success = false, message = "Usuario No existe" });

        // Leer la foto subida a byte[]
        
        

        // Obtener fotos registradas en BD
        var (fotos, codigo, mensaje) = await _repo.ObtenerFotosAsync(u.UsuarioId);
        if (codigo != 0)
            return StatusCode(codigo, new { success = false, message = mensaje });

        if (fotos is null || (fotos.Fotografia is null && fotos.Fotografia2 is null))
            return Unauthorized(new { success = false, message = "No hay fotos registradas para validar el rostro." });



        // TODO: Aquí tu se implementará la comparación con la API/ de reconocimiento facial.
        // Comparar con la API
        var match = await _face.CompareAsync(fotos.Fotografia , form.Foto);


        if (!match)
            return Unauthorized(new { success = false, message = "Rostro no reconocido o base64 inválido para su análisis." });


        var usuario = await _context.usuarios.FirstOrDefaultAsync(x => x.email == u.Email || x.Telefono == u.Telefono);
        usuario usuarioLocal;
        if (usuario == null)
        {
            // Si no existe, lo agregamos
            usuarioLocal = new usuario
            {
                email = u.Email,
                Telefono = u.Telefono,
                nickname = u.Nickname,
                PasswordHash = u.PasswordHash,
                RolId = u.RolId,
                EstaActivo = u.EstaActivo,
                Fotografia2 = u.Fotografia2,
                Fotografia2Mime = u.Fotografia2Mime
            };
            _context.usuarios.Add(usuarioLocal);
            await _context.SaveChangesAsync();
        }
        else
        {
            usuarioLocal = usuario;
        }


        var token = _jwt.CreateToken(u);
        return Ok(new
        {
            success = true,
            message = "Inicio de sesión exitoso.",
            usuario = new
            {
                usuarioLocal.UsuarioId,
                usuarioLocal.email,
                usuarioLocal.Telefono,
                usuarioLocal.nickname,
                usuarioLocal.RolId,
                usuarioLocal.EstaActivo,
                token,
                Fotografia2 = usuarioLocal.Fotografia2 != null
                ? $"data:{usuarioLocal.Fotografia2Mime};base64,{Convert.ToBase64String(usuarioLocal.Fotografia2)}"
                : null
            }
        });
    }
}