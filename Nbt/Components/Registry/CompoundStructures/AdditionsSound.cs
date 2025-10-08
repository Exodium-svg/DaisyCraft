using System.Text.Json.Serialization;

namespace Nbt.Components
{
    public partial struct AdditionsSound : INbtComponent
    {
        [JsonPropertyName("sound")]
        [NbtComponentTypeAttribute("sound", ComponentType.String)]
        public string Sound { get; set; }
        [JsonPropertyName("probability")]
        [NbtComponentTypeAttribute("probability", ComponentType.Double)]
        public double TickChance { get; set; }
    }

}
