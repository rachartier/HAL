using System;
using Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
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
    }
}
