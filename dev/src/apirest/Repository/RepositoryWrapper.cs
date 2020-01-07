using Contracts;
using Entities;

namespace Repository
{
    public class RepositoryWrapper : IRepositoryWrapper
    {
        private RepositoryContext repositoryContext;
        private IPluginRepository plugin;

        public IPluginRepository Plugin
        {
            get
            {
                plugin = plugin ?? new PluginRepository(repositoryContext);
                return plugin;
            }
        }

        public RepositoryWrapper(RepositoryContext repositoryContext)
        {
            this.repositoryContext = repositoryContext;
        }

		public void Save() {
			repositoryContext.SaveChanges();
		}
    }
}
