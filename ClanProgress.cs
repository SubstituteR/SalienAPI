using Newtonsoft.Json;

namespace Saliens
{
    /// <summary>
    /// Data Class Only
    /// </summary>
    public class ClanProgress
    {
        [JsonProperty(PropertyName = "clan_info", Required = Required.Always)]
        public ClanInfo Clan { get; private set; }

        [JsonProperty(PropertyName = "num_zones_controled", Required = Required.Always)]
        public int TotalZonesControled { get; private set; }
    }
}
