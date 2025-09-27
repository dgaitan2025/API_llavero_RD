using Api_Empleados.Funciones;
using Microsoft.AspNetCore.Mvc;
using ProyDesaWeb2025.Funciones;
using ProyDesaWeb2025.Models;
using ProyDesaWeb2025.Repositories;

namespace ProyDesaWeb2025.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly UsuariosRepository _repo;
    private readonly IConfiguration _cfg;
    private readonly IWebHostEnvironment _env;
    //public UsuariosController(UsuariosRepository repo) => _repo = repo;  Esto lo descomentaremos cuando mejoremos lo de Twilio

    public UsuariosController(UsuariosRepository repo, IConfiguration cfg, IWebHostEnvironment env)
    {
        _repo = repo;
        _cfg = cfg;
        _env = env;
    }

    /// <summary>
    /// Crea un nuevo usuario en el sistema.
    /// </summary>
    /// <param name="form">Formulario con los datos del usuario a crear, incluyendo archivos opcionales.</param>
    /// <returns>
    /// Devuelve 201 (Created) si el usuario se crea exitosamente, 400 (Bad Request) si hay un error en los datos enviados,
    /// o 500 (Internal Server Error) si ocurre un error inesperado en el servidor.
    /// </returns>
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost]
    
    public async Task<IActionResult> Crear([FromBody] UsuarioCreateForm form)
    {
        // Hashear el password plano
        var passwordPHC = BCrypt.Net.BCrypt.HashPassword(form.PasswordPlano, workFactor: 12);

        // Decodificar Base64 a byte[]
        byte[]? foto = null;
        byte[]? foto2 = null;

        if (!string.IsNullOrWhiteSpace(form.FotografiaBase64))
            foto = Convert.FromBase64String(form.FotografiaBase64);

        if (!string.IsNullOrWhiteSpace(form.Fotografia2Base64))
            foto2 = Convert.FromBase64String(form.Fotografia2Base64);

        var (codigo, mensaje, usuarioId) = await _repo.CrearAsync(
            email: form.Email,
            telefono: form.Telefono,
            fechaNac: form.FechaNacimiento,
            nickname: form.Nickname,
            passwordPHC: passwordPHC,
            foto: foto, fotoMime: form.FotografiaMime,
            foto2: foto2, foto2Mime: form.Fotografia2Mime,
            rolId: form.RolId
        );
        

        if (codigo != 0)
            return StatusCode(codigo, new { codigo, mensaje });

        //Llamamos all envío de mensajes (email y whatsapp)
        var twilio = new TwilioMsg(_cfg, _env);
        var envio = new EnvioMensajes(new EnvioCorreo(), twilio, new CarnetGenerador());

        await envio.EnviarTodoAsync(
            correo: form.Email,
            nombre: form.Nickname,
            nickname: form.Nickname,
            telefono: form.Telefono,
            contentSid: _cfg["Twilio:WhatsAppTemplateSid"]
        );
        

        return CreatedAtAction(nameof(GetById), new { id = usuarioId }, new { usuarioId, mensaje = "OK" });
    }

    /// <summary>
    /// Obtiene la información de un usuario por su identificador único.
    /// </summary>
    /// <param name="id">Identificador único del usuario.</param>
    /// <returns>
    /// Devuelve 200 (OK) con los datos del usuario si existe, o 404 (Not Found) si no se encuentra el usuario.
    /// </returns>
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("{id}")]
    public IActionResult GetById(ulong id) => Ok(new { id });
}