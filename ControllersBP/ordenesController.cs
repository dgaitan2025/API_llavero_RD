using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using ProyDesaWeb2025.DTOs;
using ProyDesaWeb2025.Hubs;
using ProyDesaWeb2025.ModelosBP;
using ProyDesaWeb2025.ModelosBP.Dto;
using System.Data;

namespace ProyDesaWeb2025.ControllersBP
{
    [Route("api/[controller]")]
    [ApiController]
    public class ordenesController : ControllerBase
    {
        private int? idRegistro;
        private readonly DBDesWeb _context;
        private readonly IHubContext<OrdenesHub> _hub; // ✅ inyectamos SignalR

        public ordenesController(DBDesWeb context, IHubContext<OrdenesHub> hub)
        {
            _context = context;
            _hub = hub;
        }

        // ✅ Endpoint de prueba para SignalR
        // Ejemplo: POST https://localhost:5001/api/ordenes/saludo?mensaje=hola
        [HttpPost("saludo")]
        public async Task<IActionResult> EnviarSaludo([FromQuery] string mensaje, int avan)
        {
            if (string.IsNullOrWhiteSpace(mensaje))
                return BadRequest(new { mensaje = "Debes enviar un mensaje en el parámetro 'mensaje'." });

            var data = new
            {
                orden = 1,
                estado = mensaje,
                avance = avan
            };

            // Envía el mensaje a todos los clientes conectados
            await _hub.Clients.All.SendAsync("RecibirSaludo", data);

            return Ok(data);
        }

        [HttpPost("ActualizarEstadoOrden")]
        public async Task<IActionResult> ActualizarEstadoOrden([FromBody] int id_Detalle)
        {
            

             
            if (id_Detalle <= 0)
                return BadRequest(new { mensaje = "El Id del registro es inválido." });

            var cs = _context.Database.GetDbConnection().ConnectionString;

            try
            {
                

                await using var conn = new MySqlConnection(cs);



                await conn.OpenAsync();
                //Estrae el ultimo Id_registro
                idRegistro = await conn.QueryFirstOrDefaultAsync<int?>(
                    @"SELECT Id_Registro
                      FROM ordenesdetalle_fases
                      WHERE Id_Detalle = @Id_Detalle
                      ORDER BY Id_Registro DESC
                      LIMIT 1;",
                    new { Id_Detalle = id_Detalle }
                );

                // 1) Avanza la fase
                await conn.ExecuteAsync(
                    "llaveros_pf.SP_Avanzar_Fase",
                    new { Id_Registro = idRegistro },
                    commandType: CommandType.StoredProcedure
                );

                // 2) Obtén el Id_Detalle del registro original
                var idDetalle = await conn.QueryFirstOrDefaultAsync<int?>(
                    @"SELECT Id_Detalle
                    FROM ordenesdetalle_fases
                    WHERE Id_Registro = @Id_Registro
                    LIMIT 1;",
                    new { Id_Registro = idRegistro }
                );

                if (idDetalle is null)
                    return Ok(new { mensaje = "Fase avanzada, pero no se pudo leer el detalle asociado.", idRegistro });

                // 3) Lee OrdenId, FaseActual, PasoActual, TotalPasos y calcula Porcentaje (último estado del detalle)
                var info = await conn.QueryFirstOrDefaultAsync<(int OrdenId, string FaseActual, int PasoActual, int TotalPasos, decimal Porcentaje)>(
                    @"
                    SELECT
                        o.Id_Orden                                           AS OrdenId,
                        f.Descripcion                                        AS FaseActual,
                        faCurr.No_Paso                                       AS PasoActual,
                        ROUND(faCurr.No_Paso * 100.0 /
                            NULLIF((SELECT COUNT(*) FROM fases_articulos faTot
                                    WHERE faTot.Id_Articulo = od.Id_Articulo), 0), 2) AS Porcentaje
                    FROM ordenesdetalle_fases odf
                    INNER JOIN ordenes_detalle od    ON od.Id_Detalle = odf.Id_Detalle
                    INNER JOIN ordenes o             ON o.Id_Orden    = od.Id_Orden
                    INNER JOIN fases f               ON f.Id_Fase     = odf.Id_Fase
                    INNER JOIN fases_articulos faCurr
                            ON faCurr.Id_Fase    = odf.Id_Fase
                        AND faCurr.Id_Articulo = od.Id_Articulo
                    WHERE odf.Id_Detalle = @Id_Detalle
                    ORDER BY odf.Fecha_Inicio DESC, odf.Id_Registro DESC
                    LIMIT 1;",
                    new { Id_Detalle = idDetalle }
                );

                if (info.Equals(default((int, string, int, int, decimal))))
                    return Ok(new { mensaje = "Fase avanzada, pero no se encontró el estado actual.", idRegistro });

                // 4) Enviar por SignalR SOLO lo necesario (incluyendo paso actual y total)
                var payload = new
                {
                    orden      = info.OrdenId,
                    estado     = info.FaseActual,
                    avance     = info.Porcentaje,
                    pasoActual = info.PasoActual
                };


                await _hub.Clients.All.SendAsync("RecibirSaludo", payload);

                return Ok(new
                {
                    mensaje = "Fase avanzada correctamente.",
                    data = payload
                });
            }
            catch (MySqlException ex) when (ex.Number == 1644) // SIGNAL SQLSTATE '45000'
            {
                return Conflict(new { mensaje = ex.Message, codigo = ex.Number, idRegistro });
            }
            catch (MySqlException ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error al ejecutar el procedimiento almacenado.",
                    codigo = ex.Number,
                    detalle = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno del servidor.", detalle = ex.Message });
            }
        }

        // GET: api/ordenes/GetordenesUsuario?UsuarioId=123
        [HttpGet("GetordenesUsuario")]
        public async Task<ActionResult<IEnumerable<OrdenUsuarioDto>>> GetordenesUsuario([FromQuery] long UsuarioId)
        {
            try
            {
                // Reutilizamos el connection string de tu DbContext
                string cs = _context.Database.GetDbConnection().ConnectionString;

                using var cn = new MySqlConnection(cs);
                await cn.OpenAsync();

                var datos = await cn.QueryAsync<OrdenUsuarioDto>(
                    "sp_ordenes_progreso_por_usuario",
                    new { pUsuarioId = UsuarioId },
                    commandType: CommandType.StoredProcedure
                );

                return Ok(datos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        // GET: api/ordenes/5
        [HttpGet("OrdenesDetails/{id}")]
        public async Task<ActionResult<ordene>> Getordene(int id)
        {
            var ordene = await _context.ordenes.FindAsync(id);

            if (ordene == null)
            {
                return NotFound();
            }

            return ordene;
        }

        // PUT: api/ordenes/5
        [HttpPut("OrdenesActualizar/{id}")]
        public async Task<IActionResult> Putordene(int id, ordene ordene)
        {
            if (id != ordene.Id_Orden)
            {
                return BadRequest();
            }

            _context.Entry(ordene).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ordeneExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/ordenes
        [HttpPost("CrearOrden")]
        public async Task<IActionResult> CrearOrden([FromBody] OrdenesViewModel orden)
        {
            try
            {
                string connectionString = _context.Database.GetDbConnection().ConnectionString;

                using var conn = new MySqlConnection(connectionString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand("SP_ordenes_crear", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("p_id_usuario", orden.Id_Usuario);
                cmd.Parameters.AddWithValue("p_id_tipo_pago", orden.Id_Tipo_Pago);
                cmd.Parameters.AddWithValue("p_fecha", orden.Fecha);
                cmd.Parameters.AddWithValue("p_total", orden.Total);
                cmd.Parameters.AddWithValue("p_persona_entregar", orden.Persona_Entregar);
                cmd.Parameters.AddWithValue("p_direccion", orden.Direccion_entrega);
                cmd.Parameters.AddWithValue("p_telefono", orden.Telefono);
                cmd.Parameters.AddWithValue("p_etrega_domicilio", orden.entrega_domicilio);
                cmd.Parameters.AddWithValue("p_detalles", orden.detalles);

                var result = await cmd.ExecuteScalarAsync();

                if (result == null)
                    return BadRequest(new { success = false, message = "Error al crear la orden" });

                return Ok(new { success = true, id_orden = Convert.ToInt32(result) });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("OrdenesEliminar/{id}")]
        public async Task<IActionResult> Deleteordene(int id)
        {
            var ordene = await _context.ordenes.FindAsync(id);
            if (ordene == null)
            {
                return NotFound();
            }

            _context.ordenes.Remove(ordene);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ordeneExists(int id)
        {
            return _context.ordenes.Any(e => e.Id_Orden == id);
        }

        [HttpGet("ObtenerGrabados")]
        public async Task<ActionResult<IEnumerable<string>>> Getgrabados()
        {
            var ListaGrabados = await _context.tipo_grabados_nfcs
                .Where(d => d.estado == 1 && d.Descripcion != null)
                .Select(d => d.Descripcion)
                .ToListAsync();
            return Ok(ListaGrabados);
        }

        [HttpGet("OrdenesPendientes/{operador}")]
        public async Task<ActionResult> GetYAsignarOrdenPendiente(int operador)
        {
            try
            {
                // Ejecutar el procedimiento y obtener la vista resultante
                var resultado = await _context.vw_ordenes_pendientes
                    .FromSqlRaw("CALL sp_ordenes_pendientes_asignar({0})", operador)
                    .ToListAsync();
            
                // Si el procedimiento devuelve un mensaje de error o está vacío
                if (resultado == null || resultado.Count == 0)
                {
                    return NotFound(new { mensaje = "No hay órdenes disponibles para asignar." });
                }

                // Devuelve la primera (única) orden resultante
                return Ok(resultado.First());
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al asignar orden: {ex.Message}");
            }
        }


        [HttpGet("OrdenesFinalizadas/{id}")]
        public async Task<ActionResult<IEnumerable<vw_ordenes_finalizada>>> Getfinalizadas(ulong id)
        {
           return await _context.vw_ordenes_finalizadas
                .Where(d => d.UsuarioId == id)
                .ToListAsync();
        }
    }
}