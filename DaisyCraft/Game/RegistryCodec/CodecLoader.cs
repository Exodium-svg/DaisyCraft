using DaisyCraft.Utils;
using Game.RegistryCodec.Registries;
using Nbt.Tags;
using System.Collections.Concurrent;
using System.Reflection;

namespace Game.Registery
{
    public class CodecLoader
    {
        public string RootDir { get; init; }
        private ConcurrentDictionary<string, IRegistryEntry> registries = new();
        public CodecLoader(string rootDir)
        {
            RootDir = rootDir;
        }
        
        public async Task Load(Logger logger)
        {
            if (!Directory.Exists(RootDir))
                throw new DirectoryNotFoundException("Failed to locate " + RootDir);

            List<Task> tasks = new List<Task>();
            foreach (string path in Directory.EnumerateDirectories(RootDir))
            {
                string datapack = Path.GetFileName(path)!;


                try { tasks.Add(ParseDatapack(datapack, logger)); }
                catch (Exception ex) { logger.Exception(ex); }
            }

            await Task.WhenAll(tasks);
        }

        private async Task ParseDatapack(string datapack, Logger logger)
        {
            List<Task> tasks = new();

            string directory = Path.Combine(RootDir, datapack);
            foreach (string registryPaths in Directory.GetDirectories(directory))
            {

                if (!Directory.Exists(registryPaths))
                {
                    logger.Warn($"found file outside of registry ({registryPaths}), ignoring.");
                    continue;
                }
                tasks.Add(ParseRegisteryFolder(registryPaths, datapack, Path.GetFileName(registryPaths), logger));
            }

            await Task.WhenAll(tasks);
            logger.Info($"Loaded datapack: {datapack}");
        }

        private async Task ParseRegisteryFolder(string registryPath, string datapack, string nameSpace, Logger logger)
        {

            foreach (string path in Directory.GetFiles(registryPath))
            {
                if (Directory.Exists(path))
                {
                    string newNamespace = $"{nameSpace}/{Path.GetFileName(path)}";
                    await ParseRegisteryFolder(path, datapack, newNamespace, logger);
                }

                await ParseRegistryFile(datapack, nameSpace, path, logger);
            }
        }


        private async Task ParseRegistryFile(string datapack, string nameSpace, string path, Logger logger)
        {
            IRegistryEntry? entry = RegistryFactory.Create(nameSpace, await File.ReadAllTextAsync(path));

            if ( null == entry )
            {
                logger.Warn($"Registry not found: ({nameSpace}), ignoring.");
                return;
            }

            string tag = $"{datapack}:{Path.GetFileNameWithoutExtension(path)}";
            
            registries[tag] = entry;
        }
    }
}
