using System;
using Contracts;
using Entities.Models;
using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
    [ApiVersion("1.0")]
    [Route("api/plugin")]
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
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("{name}")]
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

        [HttpPost]
        public IActionResult CreatePlugin([FromBody]Plugin plugin)
        {
            try
            {
                if (plugin is null)
                {
                    logger.LogError("Plugin object sent from client is null.");
                    return BadRequest("Plugin object is null.");
                }

                if (!ModelState.IsValid)
                {
                    logger.LogError("Invalid plugin object sent from client.");
                    return BadRequest("Invalid plugin model.");
                }

                repository.Plugin.CreatePlugin(plugin);
                repository.Save();

                return CreatedAtRoute("", new { name = plugin.Name }, plugin);
            }
            catch (Exception e)
            {
                logger.LogError($"Error in CreatePlugin: {e.Message}");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpDelete("{name}")]
        public IActionResult DeletePlugin(string name)
        {
            try
            {
                var plugin = repository.Plugin.GetPluginByName(name);

                if (plugin is null)
                {
                    logger.LogError($"Plugin \"{name}\" not found in db.");
                    return NotFound();
                }

                repository.Plugin.DeletePlugin(plugin);
                repository.Save();

                return NoContent();
            }
            catch (Exception e)
            {
                logger.LogError($"Error in DeletePlugin: {e.Message}");
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}
