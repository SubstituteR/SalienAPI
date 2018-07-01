using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Saliens
{
    public class BossPlayer
    {
        [JsonProperty(PropertyName = "accountid", Required = Required.Always)]
        public long AccountID { get; private set; }

        [JsonProperty(PropertyName = "clan_info", Required = Required.DisallowNull)]
        public ClanInfo Clan { get; private set; }

        [JsonProperty(PropertyName = "time_joined", Required = Required.DisallowNull)]
        public long TimeJoined { get; private set; }

        [JsonProperty(PropertyName = "time_last_seen", Required = Required.DisallowNull)]
        public long TimeLastSeen { get; private set; }

        [JsonProperty(PropertyName = "name", Required = Required.DisallowNull)]
        public string Name { get; private set; }

        [JsonProperty(PropertyName = "hp", Required = Required.DisallowNull)]
        public int HP { get; private set; }

        [JsonProperty(PropertyName = "max_hp", Required = Required.DisallowNull)]
        public int MaxHP { get; private set; }

        [JsonProperty(PropertyName = "salien", Required = Required.DisallowNull)]
        public Salien Salien { get; private set; }

        [JsonProperty(PropertyName = "score_on_join", Required = Required.DisallowNull)]
        public int CurrentScore { get; private set; }

        [JsonProperty(PropertyName = "level_on_join", Required = Required.DisallowNull)]
        public int CurrentLevel { get; private set; }

        [JsonProperty(PropertyName = "xp_earned", Required = Required.DisallowNull)]
        public int XPEarned { get; private set; }

        [JsonProperty(PropertyName = "new_level", Required = Required.DisallowNull)]
        public int NextLevel { get; private set; }

        [JsonProperty(PropertyName = "next_level_score", Required = Required.DisallowNull)]
        public int NextLevelScore { get; private set; }
    }
}
