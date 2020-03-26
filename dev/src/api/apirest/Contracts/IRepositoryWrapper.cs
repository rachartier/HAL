namespace Contracts
{
    public interface IRepositoryWrapper
    {
        IPluginRepository Plugin { get; }

        void Save();
    }
}
