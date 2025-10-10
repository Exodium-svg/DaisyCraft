using DaisyCraft.Game.Registry;
using DaisyCraft.Utils;
using Nbt.Tags;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Game.Registry
{
    public class CodecLoader
    {
        public string RootDir { get; init; }
        private ConcurrentDictionary<string, RegistryObject> registries = new();
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
            foreach (string path in Directory.GetFileSystemEntries(registryPath))
            {
                if (Directory.Exists(path))
                {
                    string newNamespace = $"{nameSpace}/{Path.GetFileName(path)}";
                    await ParseRegisteryFolder(path, datapack, newNamespace, logger);
                    continue;
                }

                await ParseRegistryFile(datapack, nameSpace, path, logger);
            }
        }


        private async Task ParseRegistryFile(string datapack, string nameSpace, string path, Logger logger)
        {
            if (Path.GetExtension(path) != ".json")
                return;

            try
            {
                string content = await File.ReadAllTextAsync(path);
                JsonDocument document = JsonDocument.Parse(content);

                string name = Path.GetFileNameWithoutExtension(path);

                string tag = $"{datapack}:{nameSpace}/{name}";
                registries[tag] = new RegistryObject(name, nameSpace, ConvertJsonToNbtCompound(document.RootElement, tag));
            }
            catch (Exception ex)
            {
                logger.Warn($"Failed to parse {path}: {ex.Message}");
            }
        }
        private NbtList BuildNbtListFromJson(JsonElement jsonVal, string key = "")
        {
            if (jsonVal.ValueKind != JsonValueKind.Array)
                return new NbtList(key, TagType.String, Array.Empty<INbtTag>());

            var tags = new List<INbtTag>();
            TagType elementType = TagType.String;

            int index = 0;
            foreach (var elem in jsonVal.EnumerateArray())
            {
                var tag = ConvertJsonToNbtDynamic(elem, index.ToString());
                if (tag is NbtCompound) elementType = TagType.Compound;
                else if (tag is NbtList) elementType = TagType.List;
                tags.Add(tag);
                index++;
            }

            return new NbtList(key, elementType, tags);
        }


        private NbtCompound ConvertJsonToNbtCompound(JsonElement jsonVal, string key = "")
        {
            var compound = new NbtCompound(key);
            if (jsonVal.ValueKind != JsonValueKind.Object) return compound;

            foreach (var prop in jsonVal.EnumerateObject())
            {
                compound[prop.Name] = ConvertJsonToNbtDynamic(prop.Value, prop.Name);
            }

            return compound;
        }


        private INbtTag ConvertJsonToNbtDynamic(JsonElement jsonVal, string key = "")
        {
            return jsonVal.ValueKind switch
            {
                JsonValueKind.Object => ConvertJsonToNbtCompound(jsonVal, key),
                JsonValueKind.Array => BuildNbtListFromJson(jsonVal, key),
                JsonValueKind.String => new NbtString(key, jsonVal.GetString() ?? string.Empty),
                JsonValueKind.Number => jsonVal.TryGetInt32(out var i) ? new NbtInt(key, i) : new NbtDouble(key, jsonVal.GetDouble()),
                JsonValueKind.True => new NbtByte(key, 1),
                JsonValueKind.False => new NbtByte(key, 0),
                _ => new NbtString(key, jsonVal.ToString())
            };
        }


        public RegistryCodec CreateCodec() => new(registries.ToDictionary());
    }
}
