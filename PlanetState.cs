using Newtonsoft.Json;

namespace Saliens
{
    /// <summary>
    /// Data Class Only
    /// </summary>
    public class PlanetState
    {
        [JsonProperty(PropertyName = "name", Required = Required.Always)]
        public string Name { get; private set; }

        [JsonProperty(PropertyName = "image_filename", Required = Required.Always)]
        public string ImageFilename { get; private set; } //TODO Download this image automatically

        [JsonProperty(PropertyName = "map_filename", Required = Required.Always)]
        public string MapFilename { get; private set; }

        [JsonProperty(PropertyName = "cloud_filename", Required = Required.Always)]
        public string CloudFilename { get; private set; }

        [JsonProperty(PropertyName = "land_filename", Required = Required.Always)]
        public string LandFilename { get; private set; }

        [JsonProperty(PropertyName = "difficulty", Required = Required.Always)]
        public ZoneDifficulty Difficulty { get; private set; } //What are the values ??

        [JsonProperty(PropertyName = "giveaway_id", Required = Required.Always)]
        public string GiveawayID { get; private set; }

        [JsonProperty(PropertyName = "active", Required = Required.Always)]
        public bool Active { get; private set; }

        [JsonProperty(PropertyName = "activation_time", Required = Required.DisallowNull)]
        public long ActivationTime { get; private set; }

        [JsonProperty(PropertyName = "position", Required = Required.DisallowNull)]
        public int Position { get; private set; }

        [JsonProperty(PropertyName = "captured", Required = Required.Always)]
        public bool Captured { get; private set; }

        [JsonProperty(PropertyName = "capture_progress", Required = Required.DisallowNull)]
        public float CaptureProgress { get; private set; }

        [JsonProperty(PropertyName = "capture_time", Required = Required.DisallowNull)]
        public long CaptureTime { get; private set; }

        [JsonProperty(PropertyName = "total_joins", Required = Required.DisallowNull)]
        public int TotalJoins { get; private set; }

        [JsonProperty(PropertyName = "current_players", Required = Required.DisallowNull)]
        public int CurrentPlayers { get; private set; }

        [JsonProperty(PropertyName = "priority", Required = Required.Always)]
        public int Priority { get; private set; }

        [JsonProperty(PropertyName = "tag_ids", Required = Required.Always)]
        public string TagIDs { get; private set; }
    }
}
