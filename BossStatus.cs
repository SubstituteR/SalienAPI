using Newtonsoft.Json;
using System.Collections.Generic;

namespace Saliens
{
    public class BossData
    {
        [JsonProperty(PropertyName = "boss_status", Required = Required.Always)]
        public BossStatus Status { get; private set; }

        [JsonProperty(PropertyName = "waiting_for_players", Required = Required.Always)]
        public bool Waiting { get; private set; } //for players

        [JsonProperty(PropertyName = "game_over", Required = Required.Always)]
        public bool GameOver { get; private set; }

        [JsonProperty(PropertyName = "num_laser_uses", Required = Required.Always)]
        public int LasersUsed { get; private set; }

        [JsonProperty(PropertyName = "num_team_heals", Required = Required.Always)]
        public int TeamHealsUsed { get; private set; }
    }


    public class BossStatus
    {
        [JsonProperty(PropertyName = "boss_hp", Required = Required.Always)]
        public long HP { get; private set; }

        [JsonProperty(PropertyName = "boss_max_hp", Required = Required.Always)]
        public long MaxHP { get; private set; }

        [JsonProperty(PropertyName = "boss_players", Required = Required.DisallowNull)]
        public IEnumerable<BossPlayer> Players { get; private set; }
    }
}
