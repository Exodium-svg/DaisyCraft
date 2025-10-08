using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Nbt.Components
{
    public partial struct Music : INbtComponent
    {
        [JsonPropertyName("sound")]
        [NbtComponentTypeAttribute("sound", ComponentType.String)]
        public string Sound { get; set; }
        [JsonPropertyName("min_delay")]
        [NbtComponentTypeAttribute("min_delay", ComponentType.Int)]
        public int MinDelay { get; set; }
        [JsonPropertyName("max_delay")]
        [NbtComponentTypeAttribute("max_delay", ComponentType.Int)]
        public int MaxDelay { get; set; }
        [JsonPropertyName("replace_current_music")]
        [NbtComponentTypeAttribute("replace_current_music", ComponentType.Byte)]
        public byte ReplaceCurrentMusic { get; set; }
    }
}
