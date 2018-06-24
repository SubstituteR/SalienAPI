using Newtonsoft.Json;

namespace Saliens
{
    /// <summary>
    /// Data Class Only
    /// </summary>
    public class ClanInfo
    {
        [JsonProperty(PropertyName = "accountid", Required = Required.DisallowNull)]
        public long ID { get; private set; }

        [JsonProperty(PropertyName = "name", Required = Required.DisallowNull)]
        public string Name { get; private set; }

        [JsonProperty(PropertyName = "avatar", Required = Required.DisallowNull)]
        public string Avatar { get; private set; } //TODO download the image at this hash

        [JsonProperty(PropertyName = "url", Required = Required.DisallowNull)]
        public string URL { get; private set; }
    }
}
