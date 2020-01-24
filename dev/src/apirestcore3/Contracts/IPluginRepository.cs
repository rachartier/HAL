using System.Collections.Generic;
using Entities.Models;

namespace Contracts
{
    public interface IPluginRepository : IRepositoryBase<Plugin>
    {
        IEnumerable<Plugin> GetAllPlugins();
        Plugin GetPluginByName(string name);

        void CreatePlugin(Plugin plugin);
		void DeletePlugin(Plugin plugin);
    }
}
