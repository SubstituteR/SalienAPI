namespace Saliens
{
    /// <summary>
    /// Data Class Only
    /// </summary>
    public class ClanInfo
    {
        public long ID { get; private set; }
        public string Name { get; private set; }
        public string Avatar { get; private set; } //TODO download the image at this hash
        public string URL { get; private set; }
    }
}
