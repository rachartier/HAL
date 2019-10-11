using HAL.ResourcesManager;
using System;
using System.Resources.ResXResourceReader;

namespace HAL.ResourceManager.ResourceResXManager
{
    public class ResourceResXManager : IResourceManager
    {
        private ResourceSet resXResource;

        public ResourceResXManager(string filepath)
        {
            Load(filepath);
        }

        ~ResourceResXManager()
        {
            Unload();
        }

        public object GetObject()
        {
            resXResource.GetObject
        }

        public string GetString()
        {
        }

        public void Load(string filepath)
        {
            resXResource = new ResourceSet(filepath);
        }

        public void Unload()
        {
            resXResource.Close();
        }
    }
}