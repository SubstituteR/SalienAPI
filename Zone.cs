using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Saliens
{
    /// <summary>
    /// The difficulty of a zone.
    /// </summary>
    public enum ZoneDifficulty
    {
        Invalid,
        Easy,
        Medium,
        Hard,
    }

    public enum ZoneType
    {
        Invalid = 2,
        Normal = 3,
        Boss = 4
    }

    public class Zone
    {
        [JsonProperty(PropertyName = "zone_position", Required = Required.Always)]
        public int Position { get; private set; }

        [JsonProperty(PropertyName = "leader", Required = Required.DisallowNull)]
        public ClanInfo Leader { get; private set; }

        [JsonProperty(PropertyName = "type", Required = Required.Always)]
        public ZoneType @Type { get; private set; }

        [JsonProperty(PropertyName = "gameid", Required = Required.DisallowNull)]
        public int GameID { get; private set; }

        [JsonProperty(PropertyName = "difficulty", Required = Required.Always)]
        public ZoneDifficulty Difficulty { get; private set; }

        [JsonProperty(PropertyName = "captured", Required = Required.Always)]
        public bool Captured { get; private set; }

        [JsonProperty(PropertyName = "capture_progress", Required = Required.DisallowNull)]
        public float CaptureProgress { get; private set; }

        [JsonProperty(PropertyName = "top_clans", Required = Required.DisallowNull)]
        public ClanInfo[] TopClans { get; private set; }

        [JsonIgnore]
        public int Score
        {
            get
            {
                switch (Difficulty)
                {
                    case ZoneDifficulty.Easy:
                        return 600;
                    case ZoneDifficulty.Medium:
                        return 1200;
                    case ZoneDifficulty.Hard:
                        return 2400;
                    default:
                        return 0;
                }
            }
        }
    }
}
