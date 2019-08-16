using System.Collections.Generic;
using Entities.Models;

namespace Contracts
{
    public interface IPluginRepository : IRepositoryBase<Plugin>
    {
        IEnumerable<Plugin> GetAllPlugins();
    }
}
