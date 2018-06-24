using Newtonsoft.Json;

namespace Saliens
{
    /// <summary>
    /// Data Class Only
    /// </summary>
    public class ClanInfo
    {
        [JsonProperty(PropertyName = "accountid", Required = Required.Always)]
        public long ID { get; private set; }

        [JsonProperty(PropertyName = "name", Required = Required.Always)]
        public string Name { get; private set; }

        [JsonProperty(PropertyName = "avatar", Required = Required.Always)]
        public string Avatar { get; private set; } //TODO download the image at this hash

        [JsonProperty(PropertyName = "url", Required = Required.Always)]
        public string URL { get; private set; }
    }
}
