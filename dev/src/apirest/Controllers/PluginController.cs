using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using apirest;
using Contracts;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
    
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/plugin")]
    [ApiController]
    public class PluginController : ControllerBase
    {
        private WebSocket webSocket;
        private ILoggerManager logger;
        private IRepositoryWrapper repository;

        public PluginController(ILoggerManager logger, IRepositoryWrapper repository)
        {
            this.logger = logger;
            this.repository = repository;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetAllPlugins()
        {
            try
            {
                var plugins = repository.Plugin.GetAllPlugins();

                logger.LogInfo("Returned all plugins from db.");

                return Ok(plugins);
            }
            catch (Exception e)
            {
                logger.LogError($"Error in GetAllPlugins: {e.Message}");
                return StatusCode(500, $"Internal server error: ${e.Message}");
            }
        }
/*
        [HttpGet("{name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetPluginByName(string name)
        {
            try
            {
                var plugin = repository.Plugin.GetPluginByName(name);

                if (plugin is null)
                {
                    logger.LogError($"Plugin \"{name}\" not found in db.");
                    return NotFound();
                }

                logger.LogInfo($"Returned plugin \"{name}\" from db.");
                return Ok(plugin);
            }
            catch (Exception e)
            {
                logger.LogError($"Error in GetPluginByName: {e.Message}");
                return StatusCode(500, "Internal server error.");
            }
        }
*/
        [HttpGet("{name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPluginByNameWs(string name)
        {
            if (ConfigurationOptions.UseWebSockets == false)
            {
                return StatusCode(403,
                    "Websockets not enabled. You should not enable it, but if you want, please set \"UseWebSockets\" in \"appsettings.json\" to true, then build the project.");
            }

            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                
                DateTime? lastDate = null;
            
                var timer = new Timer(async _ =>
                {
                
                    /*
                     * il faudrait que notre base mongddb integre automatiquement les dates
                     * algo:
                     *     récupérer la dernière date ajoutée (à l'instant où la ws est appelée)
                     *
                     *     attendre X secondes
                     *
                     *     récupérer tous les plugins, les ranger par ordre de date
                     *     récupérer jusqu'à la dernière date ajoutée
                     *
                     *     dernière date ajoutée = dernière date recupérée
                     */
//                var lastResults = fetchDatabase().

                    if (lastDate == null)
                    {
                        lastDate = fetchDatabase(name).Last().Date;
                        return;
                    }

                    IEnumerable<Plugin> lastResults = fetchDatabase(name).Where(p => p.Date.CompareTo(lastDate.Value) >= 0);

                    foreach (var result in lastResults)
                    {
                        var resultBytes = Encoding.ASCII.GetBytes(result.Result.ToString());
                        await webSocket.SendAsync(new ArraySegment<byte>(resultBytes, 0, resultBytes.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }, null, 0, 1000);
                
                while (webSocket.State == WebSocketState.Open)
                {
                    await Task.Delay(100);
                }

                return Ok();
                //await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            }

            return BadRequest();
        }

        public IEnumerable<Plugin> fetchDatabase(string name)
        {
            return repository.Plugin.GetPluginByName(name);
        }
    }
}
