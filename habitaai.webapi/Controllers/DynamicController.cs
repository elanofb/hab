// Controllers/DynamicController.cs
using habitaai.webapi.Dynamic;
using habitaai.webapi.domain;
using Microsoft.AspNetCore.Mvc;
using habitaai.webapi.Utils;
using System.Text.Json;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.InteropServices;
using System.Text.Unicode;

namespace habitaai.webapi.Dynamic;

[ApiController]
[Route("api/[controller]")]
public class DynamicController : ControllerBase
{
    private static readonly Dictionary<string, RequestDelegate> _handlers = new();

    private readonly ILogger<DynamicController> _logger;
    //private readonly IEndpointRouteBuilder _routes;
    private readonly AppDbContext _context;

    public DynamicController(ILogger<DynamicController> logger,
                                //IEndpointRouteBuilder routes,
                                AppDbContext context)
    {
        _logger = logger;
        //_routes = routes;
        _context = context;
    }

    [HttpPost("registrar")]
    public async Task<IActionResult> Registrar([FromBody] DynamicEndpoint endpoint)
    {
        try
        {
            if (endpoint.SourceCode.Contains("\\n")) { 
                endpoint.SourceCode = endpoint.SourceCode.Replace("\\n", "\n").Replace("\\t", "\t").Replace("\\\"", "\""); 
            }

            //context.Request.EnableBuffering();
            //using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            //var body = await reader.ReadToEndAsync();
            //context.Request.Body.Position = 0;
            //var entrada = JsonSerializer.Deserialize < Entrada_" + endpoint.MethodName + @" > (body);
            ////var entrada = JsonSerializer.Deserialize<JsonElement>(body);
            ////var entrada = JsonSerializer.Deserialize<Entrada_" + endpoint.MethodName + @">(body);
            //context.Response.ContentType = ""application / json; charset = utf - 8"";

            //" + endpoint.SourceCode + @"


            ////GET
            //var result = new { Mensagem = ""Método GET chamado"" };
            //var json = JsonSerializer.Serialize(result);
            //await context.Response.WriteAsync(json);

            endpoint.SourceCodeFull = @"
                using System;
                using System.Net.Http;
                using System.Text.Encodings.Web;
                using System.Collections.Generic;
                using Microsoft.Data.SqlClient;
                using System.Text;
                using System.Text.Json;
                using System.Threading.Tasks;
                using Microsoft.AspNetCore.Http;
                using habitaai.webapi.Utils;    
                using System.IO;

                public class Entrada_" + endpoint.MethodName + @"
                {
                    public string Nome { get; set; }
                }

                public class DynamicHandler_" + endpoint.MethodName + @"
                {
                    public static async Task " + endpoint.MethodName + @"(HttpContext context)
                    {                    
                        


                        var metodo = context.Request.Method;

                        if (metodo == ""GET"")
                        {
                            await Helper.GetHttp(context);
                            
                        }
                        else if (metodo == ""POST"")
                        {
                            context.Request.EnableBuffering();
                            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                            var body = await reader.ReadToEndAsync();
                            context.Request.Body.Position = 0;                            
                            var entrada = JsonSerializer.Deserialize<JsonElement>(body);

                            var nome = entrada.GetProperty(""nome"").GetString();
                            var result = new { Mensagem = $""Olá {nome}, método POST recebido!"" };
                            var json = JsonSerializer.Serialize(result);
                            await context.Response.WriteAsync(json);
                        }
                        else
                        {
                            context.Response.StatusCode = 405;
                            await context.Response.WriteAsync(""Método não permitido."");
                        }
                    }
                }
            ";

            var handler = DynamicCompiler.Compile(endpoint.SourceCodeFull, endpoint.MethodName);
            _handlers[endpoint.Route] = handler;

            // Normaliza a rota antes de registrar
            var routeKey = endpoint.Route.Trim().TrimStart('/').ToLowerInvariant();

            // Registra na memória
            DynamicControllerHelper.Register(routeKey, handler);

            // Salva no banco
            endpoint.RouteUniqueId = Guid.NewGuid().ToString();
            _context.DynamicEndpoints.Add(endpoint);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Endpoint registrado com sucesso." });
        }
        catch (Exception ex)
        {
            Logger.Log(endpoint.Route, endpoint, null, ex);
            return BadRequest(new { erro = ex.Message });
        }
    }

    [HttpPut("atualizar")]
    public async Task<IActionResult> Atualizar([FromBody] DynamicEndpoint endpoint)
    {   
        try
        {
            endpoint.SourceCodeFull = Utils.Helper.RetornaSourceFull(endpoint.MethodName, endpoint.SourceCode, false);
            #region old - SourceCodeFull
            //endpoint.SourceCodeFull = @"
            //    using System;
            //    using System.Net.Http;
            //    using System.Text.Encodings.Web;
            //    using System.Collections.Generic;
            //    using Microsoft.Data.SqlClient;
            //    using System.Text;
            //    using System.Text.Json;
            //    using System.Threading.Tasks;
            //    using Microsoft.AspNetCore.Http;
            //    using habitaai.webapi.Utils;    
            //    using System.IO;

            //    public class Entrada_" + endpoint.MethodName + @"
            //    {
            //        public string Nome { get; set; }
            //    }

            //    public class DynamicHandler_" + endpoint.MethodName + @"
            //    {
            //        public static async Task " + endpoint.MethodName + @"(HttpContext context)
            //        {                    



            //            var metodo = context.Request.Method;

            //            if (metodo == ""GET"")
            //            {
            //                await Helper.GetHttp(context);

            //            }
            //            else if (metodo == ""POST"")
            //            {
            //                context.Request.EnableBuffering();
            //                using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            //                var body = await reader.ReadToEndAsync();
            //                context.Request.Body.Position = 0;                            
            //                var entrada = JsonSerializer.Deserialize<JsonElement>(body);

            //                var nome = entrada.GetProperty(""nome"").GetString();
            //                var result = new { Mensagem = $""Olá {nome}, método POST recebido!"" };
            //                var json = JsonSerializer.Serialize(result);
            //                await context.Response.WriteAsync(json);
            //            }
            //            else
            //            {
            //                context.Response.StatusCode = 405;
            //                await context.Response.WriteAsync(""Método não permitido."");
            //            }
            //        }
            //    }
            //";
            #endregion

            var routeKey = endpoint.Route.Trim().TrimStart('/').ToLowerInvariant();

            //DynamicControllerHelper.RegisteredHandlers.Remove(routeKey);

            // atualiza o status na tabela se existir
            //var existing = _context.DynamicEndpoints.FirstOrDefault(e => e.Route.ToLower() == routeKey && e.Enabled);
            var existing = _context.DynamicEndpoints.FirstOrDefault(e => e.Id == endpoint.Id && e.Enabled);
            if (existing != null)
            {
                var routeKeyLast = existing.Route.Trim().TrimStart('/').ToLowerInvariant();
                // Remove handler antigo se existir
                

                if (existing.Route != routeKey)
                {
                    endpoint.RouteLast = existing.Route;
                    DynamicControllerHelper.RegisteredHandlers.Remove(endpoint.RouteLast.Trim().TrimStart('/').ToLowerInvariant());
                }
                else { 
                    endpoint.RouteLast = existing.RouteLast;
                    DynamicControllerHelper.RegisteredHandlers.Remove(routeKeyLast);
                }

                //_context.DynamicEndpoints.Remove(existing);
                existing.Enabled = false;
                endpoint.RouteUniqueId = existing.RouteUniqueId ?? Guid.NewGuid().ToString(); ;
                _context.DynamicEndpoints.Update(existing);
                await _context.SaveChangesAsync();
            }

            // Reinsere NOVO
            var endpointNew = new DynamicEndpoint();
            endpointNew.Enabled = true;
            endpointNew.RouteLast = endpoint.RouteLast;
            endpointNew.RouteUniqueId = endpoint.RouteUniqueId;
            endpointNew.Route = endpoint.Route;
            endpointNew.SourceCode = endpoint.SourceCode;
            endpointNew.SourceCodeFull = endpoint.SourceCodeFull;
            endpointNew.CreatedAt = DateTime.UtcNow;
            endpointNew.MethodName = endpoint.MethodName;

            var handler = DynamicCompiler.Compile(endpointNew.SourceCodeFull, endpointNew.MethodName);
            DynamicControllerHelper.Register(routeKey, handler);
            _context.DynamicEndpoints.Add(endpointNew);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Endpoint atualizado com sucesso." });
        }
        catch (Exception ex)
        {
            Logger.Log(endpoint.Route, endpoint, null, ex);
            return BadRequest(new { erro = ex.Message });
        }

    }

    [HttpPut("atualizarcode")]
    public async Task<IActionResult> AtualizarCodigo([FromBody] DynamicEndpoint endpoint)
    {
        try
        {
            endpoint.SourceCodeFull = Utils.Helper.RetornaSourceFull(endpoint.MethodName, endpoint.SourceCode, false);
            #region old - SourceCodeFull
            //endpoint.SourceCodeFull = @"
            //    using System;
            //    using System.Net.Http;
            //    using System.Text.Encodings.Web;
            //    using System.Collections.Generic;
            //    using Microsoft.Data.SqlClient;
            //    using System.Text;
            //    using System.Text.Json;
            //    using System.Threading.Tasks;
            //    using Microsoft.AspNetCore.Http;
            //    using habitaai.webapi.Utils;    
            //    using System.IO;

            //    public class Entrada_" + endpoint.MethodName + @"
            //    {
            //        public string Nome { get; set; }
            //    }

            //    public class DynamicHandler_" + endpoint.MethodName + @"
            //    {
            //        public static async Task " + endpoint.MethodName + @"(HttpContext context)
            //        {                    



            //            var metodo = context.Request.Method;

            //            if (metodo == ""GET"")
            //            {
            //                await Helper.GetHttp(context);

            //            }
            //            else if (metodo == ""POST"")
            //            {
            //                context.Request.EnableBuffering();
            //                using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            //                var body = await reader.ReadToEndAsync();
            //                context.Request.Body.Position = 0;                            
            //                var entrada = JsonSerializer.Deserialize<JsonElement>(body);

            //                var nome = entrada.GetProperty(""nome"").GetString();
            //                var result = new { Mensagem = $""Olá {nome}, método POST recebido!"" };
            //                var json = JsonSerializer.Serialize(result);
            //                await context.Response.WriteAsync(json);
            //            }
            //            else
            //            {
            //                context.Response.StatusCode = 405;
            //                await context.Response.WriteAsync(""Método não permitido."");
            //            }
            //        }
            //    }
            //";
            #endregion

            var routeKey = endpoint.Route.Trim().TrimStart('/').ToLowerInvariant();

            //DynamicControllerHelper.RegisteredHandlers.Remove(routeKey);


            //Desabilita todos os outros endpoints antigos
            var olds = _context.DynamicEndpoints.Where(e => e.RouteUniqueId == endpoint.RouteUniqueId).ToList();
            if (olds != null)
            { 
                foreach ( var old in olds ) {
                    old.Enabled = false;
                    _context.DynamicEndpoints.Update(old);
                }
                await _context.SaveChangesAsync();
            }

            // atualiza o status na tabela se existir
            //var existing = _context.DynamicEndpoints.FirstOrDefault(e => e.Route.ToLower() == routeKey && e.Enabled);
            //var existing = _context.DynamicEndpoints.FirstOrDefault(e => e.Id == endpoint.Id && e.Enabled);
            var existing = _context.DynamicEndpoints.FirstOrDefault(e => e.RouteUniqueId == endpoint.RouteUniqueId); // && e.Enabled
            if (existing != null)
            {
                var routeKeyLast = existing.Route.Trim().TrimStart('/').ToLowerInvariant();
                // Remove handler antigo se existir


                if (existing.Route != routeKey)
                {
                    endpoint.RouteLast = existing.Route;
                    DynamicControllerHelper.RegisteredHandlers.Remove(endpoint.RouteLast.Trim().TrimStart('/').ToLowerInvariant());
                }
                else
                {
                    endpoint.RouteLast = existing.RouteLast;
                    DynamicControllerHelper.RegisteredHandlers.Remove(routeKeyLast);
                }

                //_context.DynamicEndpoints.Remove(existing);
                existing.Enabled = false;

                if(existing.RouteUniqueId == null)
                {
                    existing.RouteUniqueId = Guid.NewGuid().ToString();
                }
                endpoint.RouteUniqueId = existing.RouteUniqueId;

                //endpoint.RouteUniqueId = existing.RouteUniqueId ?? Guid.NewGuid().ToString();
                //endpoint.RouteLast = existing.RouteLast;
                //endpoint.RouteUniqueId = existing.RouteUniqueId;
                //endpoint.Route = existing.Route;
                //endpoint.SourceCode = existing.SourceCode;
                //endpoint.SourceCodeFull = existing.SourceCodeFull;
                //endpoint.CreatedAt = DateTime.UtcNow;
                endpoint.MethodName = existing.MethodName;

                _context.DynamicEndpoints.Update(existing);
                await _context.SaveChangesAsync();
            }


            // Reinsere NOVO
            var endpointNew = new DynamicEndpoint();
            endpointNew.Enabled = true;
            endpointNew.RouteLast = endpoint.RouteLast;
            endpointNew.RouteUniqueId = !string.IsNullOrEmpty(endpoint.RouteUniqueId) ? endpoint.RouteUniqueId: Guid.NewGuid().ToString();
            endpointNew.Route = endpoint.Route;
            endpointNew.SourceCode = endpoint.SourceCode;
            endpointNew.SourceCodeFull = endpoint.SourceCodeFull;
            endpointNew.CreatedAt = DateTime.UtcNow;
            endpointNew.MethodName = endpoint.MethodName;

            var handler = DynamicCompiler.Compile(endpointNew.SourceCodeFull, endpointNew.MethodName);
            DynamicControllerHelper.Register(routeKey, handler);
            _context.DynamicEndpoints.Add(endpointNew);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Endpoint atualizado com sucesso." });
        }
        catch (Exception ex)
        {
            Logger.Log(endpoint.Route, endpoint, null, ex);
            return BadRequest(new { erro = ex.Message });
        }

    }

    [HttpGet("obter")]
    public IActionResult Obter([FromQuery] string route, [FromServices] AppDbContext db)
    {
        if (string.IsNullOrWhiteSpace(route))
            return BadRequest("Parâmetro 'route' é obrigatório.");

        var endpoint = db.Set<DynamicEndpoint>()
                         .FirstOrDefault(e => e.Route.ToLower() == route.Trim().ToLower() && e.Enabled);

        if (endpoint == null)
            return NotFound($"Endpoint '{route}' não encontrado.");

        return Ok(new
        {
            endpoint.Id,
            endpoint.Route,
            endpoint.MethodName,
            endpoint.Enabled,
            endpoint.CreatedAt,
            endpoint.SourceCode
        });
    }

    [HttpGet("obter/{id:int}")]
    public IActionResult ObterPorId(int id, [FromServices] AppDbContext db)
    {
        var endpoint = db.Set<DynamicEndpoint>().FirstOrDefault(e => e.Id == id);
        if (endpoint == null)
            return NotFound($"Endpoint ID {id} não encontrado.");

        return Ok(endpoint);
    }

    [HttpGet("obter/{uid}")]
    public IActionResult ObterPorUid(string uid, [FromServices] AppDbContext db)
    {
        //var endpoint = db.Set<DynamicEndpoint>().FirstOrDefault(e => e.RouteUniqueId == uid && e.Enabled);
        var endpoint = db.Set<DynamicEndpoint>().FirstOrDefault(e => e.RouteUniqueId == uid && e.Enabled);
        if (endpoint == null)
            return NotFound($"Endpoint UID {uid} não encontrado.");

        return Ok(endpoint);
    }

    //[HttpGet("obter")]
    //public IActionResult Obter([FromQuery] int id, string route, [FromServices] AppDbContext db)
    //{
    //    if (id != null && id > 0)
    //        return BadRequest("Parâmetro 'id' é obrigatório.");

    //    var endpoint = db.Set<DynamicEndpoint>()
    //                        .FirstOrDefault(e => e.Id == id);

    //    if (endpoint == null)
    //        return NotFound($"Endpoint '{route}' não encontrado.");

    //    return Ok(new
    //    {
    //        endpoint.Id,
    //        endpoint.Route,
    //        endpoint.MethodName,
    //        endpoint.Enabled,
    //        endpoint.CreatedAt,
    //        endpoint.SourceCode
    //    });
    //}


    [HttpGet("listar")]
    //public IActionResult Listar() => Ok(_handlers.Keys);
    public IActionResult Listar([FromServices] AppDbContext db)
    {
        var endpoints = db.Set<DynamicEndpoint>()
                          .Where(e => e.Enabled)
                          .Select(e => e.Route)
                          .ToList();

        return Ok(endpoints);
    }

    [HttpGet("testHandler")]
    //public IActionResult Listar() => Ok(_handlers.Keys);
    public IActionResult TestHandler([FromServices] AppDbContext db)
    {
        new DynamicHandler().ExecuteAsync();

        return Ok();
    }

    //
}
