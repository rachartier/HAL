using System;
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
    }
}
