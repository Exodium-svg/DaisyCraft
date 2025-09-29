using DaisyCraft.Utils;
using Nbt.Tags;

namespace Game.Registery
{
    public class RegisteryLoader
    {
        public string RootDir { get; init; }
        public List<INbtTag> Tags { get; init; } = new();
        public RegisteryLoader(string rootDir) => RootDir = rootDir;
        private int fileCount = 0;
        public async Task Load(Logger logger)
        {

            if (!Directory.Exists(RootDir))
                throw new DirectoryNotFoundException("Failed to locate " + RootDir);

            List<Task> tasks = new List<Task>();
            foreach(string directory in Directory.EnumerateDirectories(RootDir))
                 tasks.Add(ParseDirectory(directory, logger));

            await Task.WhenAll(tasks);
            logger.Info($"{fileCount} files");
        }

        private async Task ParseDirectory(string directory, Logger logger)
        {
            foreach(string path in Directory.GetFiles(directory))
            {

                if (Directory.Exists(path))
                {
                    await ParseDirectory(path, logger);
                    continue;
                }

                else
                    fileCount++;

                string json = File.ReadAllText(path);
            }
        }
    }
}
