using Newtonsoft.Json;

namespace Saliens
{
    public class Salien
    {
        [JsonProperty(PropertyName = "body_type", Required = Required.Always)]
        public int Body { get; private set; }

        [JsonProperty(PropertyName = "mouth", Required = Required.Always)]
        public int Mouth { get; private set; }

        [JsonProperty(PropertyName = "eyes", Required = Required.Always)]
        public int Eyes { get; private set; }

        [JsonProperty(PropertyName = "arms", Required = Required.Always)]
        public int Arms { get; private set; }

        [JsonProperty(PropertyName = "legs", Required = Required.Always)]
        public int Legs { get; private set; }

        [JsonProperty(PropertyName = "hat_itemid", Required = Required.DisallowNull)]
        public long Hat { get; private set; }

        [JsonProperty(PropertyName = "shirt_itemid", Required = Required.DisallowNull)]
        public long Shirt { get; private set; }

        [JsonProperty(PropertyName = "hat_image", Required = Required.DisallowNull)]
        public string HatImage { get; private set; }

        [JsonProperty(PropertyName = "shirt_image", Required = Required.DisallowNull)]
        public string ShirtImage { get; private set; }

    }
}
