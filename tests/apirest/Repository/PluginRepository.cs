using System.Collections.Generic;
using System.Linq;
using Contracts;
using Entities;
using Entities.Models;

namespace Repository
{
    public class PluginRepository : RepositoryBase<Plugin>, IPluginRepository
    {
        public PluginRepository(RepositoryContext repositoryContext)
            : base(repositoryContext)
        {
        }

        public IEnumerable<Plugin> GetAllPlugins()
        {
            return FindAll()
                .OrderBy(x => x.Name)
                .ToList();
        }
    }
}
