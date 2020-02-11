using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
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

        [HttpGet("ws/{name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPluginByNameWs(string name)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                
                var buffer = new byte[1024 * 4];
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                
                while (!result.CloseStatus.HasValue)
                {
                    await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);

                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }
                await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            }
        }
    }
}
