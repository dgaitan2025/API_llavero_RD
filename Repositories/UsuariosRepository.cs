using System.Data;
using Dapper;
using MySqlConnector;
using ProyDesaWeb2025.Models;


namespace ProyDesaWeb2025.Repositories;

public class UsuariosRepository
{
    private readonly IConfiguration _cfg;
    public UsuariosRepository(IConfiguration cfg) => _cfg = cfg;

    private IDbConnection Conn()
    {
        var cs = _cfg.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'DefaultConnection'.");
        return new MySqlConnection(cs);
    }
   

    public async Task<(int Codigo, string Mensaje, ulong? UsuarioId)> CrearAsync(
        string email, string? telefono, DateTime? fechaNac, string nickname,
        string passwordPHC,
        byte[]? foto, string? fotoMime,
        byte[]? foto2, string? foto2Mime,
        byte rolId)
    {
        using var db = Conn();
        var p = new DynamicParameters();

        p.Add("p_Email", email);
        p.Add("p_Telefono", telefono);
        p.Add("p_FechaNacimiento", fechaNac);
        p.Add("p_Nickname", nickname);
        p.Add("p_PasswordPHC", passwordPHC);

        p.Add("p_Fotografia", foto, DbType.Binary);
        p.Add("p_FotografiaMime", fotoMime);
        p.Add("p_Fotografia2", foto2, DbType.Binary);
        p.Add("p_Fotografia2Mime", foto2Mime);

        p.Add("p_RolId", rolId);

        p.Add("p_UsuarioId", dbType: DbType.UInt64, direction: ParameterDirection.Output);
        p.Add("p_Codigo", dbType: DbType.Int32, direction: ParameterDirection.Output);
        p.Add("p_Mensaje", dbType: DbType.String, size: 255, direction: ParameterDirection.Output);

        await db.ExecuteAsync("sp_usuarios_crear", p, commandType: CommandType.StoredProcedure);

        var codigo = p.Get<int>("p_Codigo");
        var mensaje = p.Get<string>("p_Mensaje");
        var usuarioId = p.Get<ulong?>("p_UsuarioId");

        return (codigo, mensaje, usuarioId);
    }
    
    public async Task<UsuarioCredencial?> ObtenerPorCredencialAsync(string identificador)
    {
        const string sql = "CALL bd_desaweb_2025_pf.sp_usuarios_obtener_por_credencial(@p_Identificador);";
        using var db = Conn();
        return await db.QueryFirstOrDefaultAsync<UsuarioCredencial>(sql, new { p_Identificador = identificador?.Trim() });
    }

    public async Task<(UsuarioFotos? Fotos, int Codigo, string Mensaje)> ObtenerFotosAsync(ulong usuarioId)
    {
        using var db = Conn();
        var p = new DynamicParameters();
        p.Add("p_UsuarioId", usuarioId, DbType.UInt64, ParameterDirection.Input);
        p.Add("p_Codigo", dbType: DbType.Int32, direction: ParameterDirection.Output);
        p.Add("p_Mensaje", dbType: DbType.String, size: 255, direction: ParameterDirection.Output);

        // Ejecuta el SP que devuelve un resultset (UsuarioId, Fotografia, Fotografia2) y OUT params (p_Codigo, p_Mensaje)
        var fotos = await db.QueryFirstOrDefaultAsync<UsuarioFotos>(
            "sp_usuarios_obtener_fotos",
            p,
            commandType: CommandType.StoredProcedure
        );

        var codigo = p.Get<int>("p_Codigo");
        var mensaje = p.Get<string>("p_Mensaje") ?? string.Empty;
        return (fotos, codigo, mensaje);
    }
}