using Microsoft.AspNetCore.Mvc;
using ProyDesaWeb2025.Models;
using ProyDesaWeb2025.Models.Dto;
using ProyDesaWeb2025.Repositories;
using ProyDesaWeb2025.Security;

namespace ProyDesaWeb2025.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UsuariosRepository _repo;
    private readonly IConfiguration _cfg;
    private readonly JwtTokenService _jwt;

    public AuthController(UsuariosRepository repo, IConfiguration cfg, JwtTokenService jwt)
    {
        _repo = repo;
        _cfg = cfg;
        _jwt = jwt;
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

        var token = _jwt.CreateToken(u);
        return Ok(new LoginResponse
        {
			success = true,
            Email = u.Email,
            Telefono = u.Telefono,
            Nickname = u.Nickname,
            RolId = u.RolId,
            EstaActivo = u.EstaActivo,
            Token = token,
            Fotografia2 = u.Fotografia2 != null ? Convert.ToBase64String(u.Fotografia2) : null
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
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> LoginFace([FromForm] LoginFaceRequest form)
    {
        if (form is null || string.IsNullOrWhiteSpace(form.Identificador) || form.Foto is null || form.Foto.Length == 0)
            return BadRequest("Debe enviar 'identificador' y una 'foto' válida.");

        var u = await _repo.ObtenerPorCredencialAsync(form.Identificador.Trim());
        if (u is null || !u.EstaActivo)
            return Unauthorized("Credenciales inválidas o usuario inactivo.");

        // Leer la foto subida a byte[]
        byte[] fotoCliente;
        using (var ms = new MemoryStream())
        {
            await form.Foto.CopyToAsync(ms);
            fotoCliente = ms.ToArray();
        }

        // Obtener fotos registradas en BD
        var (fotos, codigo, mensaje) = await _repo.ObtenerFotosAsync(u.UsuarioId);
        if (codigo != 0)
            return StatusCode(codigo, new { codigo, mensaje });

        if (fotos is null || (fotos.Fotografia is null && fotos.Fotografia2 is null))
            return Unauthorized("No hay fotos registradas para validar el rostro.");

        // TODO: Aquí tu se implementará la comparación con la API/ de reconocimiento facial.
        // bool match = await _face.CompareAsync(fotoCliente, fotos.Fotografia ?? fotos.Fotografia2);
        bool match = false; // Por defecto, no permitir acceso hasta que se implemente.

        if (!match)
            return Unauthorized("Rostro no reconocido.");

        var token = _jwt.CreateToken(u);

        return Ok(new LoginResponse
        {
            UsuarioId = u.UsuarioId,
            Email = u.Email,
            Telefono = u.Telefono,
            Nickname = u.Nickname,
            RolId = u.RolId,
            EstaActivo = u.EstaActivo,
            Token = token
        });
    }
}