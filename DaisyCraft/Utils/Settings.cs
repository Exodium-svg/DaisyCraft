using DaisyCraft.Utils;
using System.Text.RegularExpressions;

namespace Utils
{
    public class Settings
    {
        private readonly Dictionary<string, object> values = new();

        public async Task StartAsync(string location, Logger logger)
        {
            if (!File.Exists(location))
            {
                logger.Warn("Couldn't find settings file: " + location);
                return;
            }

            const string pattern = @"([^:]+):([^=]+)=(.*)";
            Regex regex = new(pattern);

            int settingCount = 0;
            foreach (string line in await File.ReadAllLinesAsync(location))
            {
                if (line.StartsWith("//") || string.IsNullOrWhiteSpace(line)) continue;
                #region parse line
                Match match = regex.Match(line);
                if (!match.Success) continue;

                string
                    type = match.Groups[1].Value.Trim(),
                    key = match.Groups[2].Value.Trim(),
                    value = match.Groups[3].Value.Trim();
                #endregion

                switch (type)
                {
                    case "string":
                        values[key] = value;
                        break;
                    case "int":
                        values[key] = int.Parse(value);
                        break;
                    case "float":
                        values[key] = float.Parse(value);
                        break;
                    default:
                        throw new FormatException($"Unsupported type: {type}");
                }

                settingCount++;
            }
            //logger.Info($"Loaded {settingCount} settings");
        }


        public T GetVar<T>(string key, T fallback)
        {
            if (!values.TryGetValue(key, out var value))
                return fallback;
            

            return (T)value;
        }
    }
}
