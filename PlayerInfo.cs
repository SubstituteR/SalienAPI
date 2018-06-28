using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace Saliens
{
    public class PlayerInfo
    {
        private static int[] XPTable = new int[]
        {
            0,1200,2400,4800,12000,30000,72000,180000,450000,1200000,2400000,3600000,4800000,6000000, 7200000,8400000,9600000,10800000,12000000,14400000,16800000,19200000,21600000,24000000,26400000
        };
        public static int XPForLevel(int level)
        {
            if (level >= XPTable.Length)
            {
                return XPTable[XPTable.Length - 1];
            }
            if (level <= 1)
            {
                return XPTable[0];
            }
            return XPTable[level - 1];
        }
        private void UpdateValues(PlayerInfo B)
        {
            ActivePlanetID = B.ActivePlanetID;
            ActiveZoneID = B.ActiveZoneID;
            ActiveZonePosition = B.ActiveZonePosition;
            Clan = B.Clan;
            Level = B.Level;
            NextLevelScore = B.NextLevelScore;
            Score = B.Score;
            TimeOnPlanet = B.TimeOnPlanet;
        }
        #region JSON

        [JsonProperty(PropertyName = "active_planet", Required = Required.DisallowNull)]
        private int ActivePlanetID { get; set; }

        [JsonProperty(PropertyName = "active_zone_game", Required = Required.DisallowNull)]
        private int ActiveZoneID { get; set; }

        [JsonProperty(PropertyName = "active_zone_position", Required = Required.DisallowNull)]
        private int ActiveZonePosition { get; set; }

        [JsonProperty(PropertyName = "clan_info", Required = Required.DisallowNull)]
        public ClanInfo Clan { get; private set; }

        [JsonProperty(PropertyName = "level", Required = Required.Always)]
        public int Level { get; private set; }

        [JsonProperty(PropertyName = "next_level_score", Required = Required.Always)]
        public int NextLevelScore { get; private set; }

        [JsonProperty(PropertyName = "score", Required = Required.Always)]
        public int Score { get; private set; }

        [JsonProperty(PropertyName = "time_on_planet", Required = Required.DisallowNull)]
        public int TimeOnPlanet { get; private set; }

        [JsonProperty(PropertyName = "time_in_zone", Required = Required.DisallowNull)]
        public int TimeInZone { get; private set; }

        #endregion

        [JsonIgnore]
        public Planet Planet
        {
            get
            {
                if (ActivePlanetID != 0)
                {
                    return Planet.Get(ActivePlanetID).GetAwaiter().GetResult();
                }
                else
                {
                    return null;
                }
            }
        }


        [JsonIgnore]
        public Zone Zone => Planet?.Zones.Where(x => x.GameID == ActiveZoneID).FirstOrDefault();

        [JsonIgnore]
        public string Token { get; private set; }

        [JsonConstructor]
        private PlayerInfo() { } //Required blank constructor for deserialization.

        public PlayerInfo(string Token)
        {
            this.Token = Token;
            GetPlayerInfo().GetAwaiter().GetResult();
        }
        private async Task GetPlayerInfo()
        {
            string JSON = await Network.Post("GetPlayerInfo", Network.EndPoint.ITerritoryControlMinigameService, "access_token", Token);
            UpdateValues(Network.Deserialize<PlayerInfo>(JSON));
        }
        public async Task JoinPlanet(int PlanetID)
        {
            if (Planet != null)
            {
                if (Planet.ID == PlanetID) return;
                await LeavePlanet();
            }
            await Network.Post("JoinPlanet", Network.EndPoint.ITerritoryControlMinigameService, "access_token", Token, "id", PlanetID);
            await GetPlayerInfo();
        }
        public async Task JoinPlanet(Planet planet) => await JoinPlanet(planet.ID);

        public async Task JoinZone(int ZonePosition)
        {
            if(Zone != null)
            {
                if (Zone.Position == ZonePosition) return;
                await LeaveZone();
            }
            await Network.Post("JoinZone", Network.EndPoint.ITerritoryControlMinigameService, "access_token", Token, "zone_position", ZonePosition);
            await GetPlayerInfo();
        }

        private async Task Leave(int GameID)
        {
            await Network.Post("LeaveGame", Network.EndPoint.IMiniGameService, "access_token", Token, "gameid", GameID);
            await GetPlayerInfo();
        }
        public async Task LeaveZone()
        {
            if (Zone != null) await Leave(ActiveZoneID);
        }

        public async Task LeavePlanet()
        {
            if (Zone != null) await LeaveZone();
            if (Planet != null) await Leave(ActivePlanetID);
        }
        public async Task ReportScore()
        {
            if (Zone != null)
            {
                await Network.Post("ReportScore", Network.EndPoint.ITerritoryControlMinigameService, "access_token", Token, "score", Zone.Score);
                await GetPlayerInfo();
            }
        }
        public async Task RepresentClan(int ClanID)
        {
            await Network.Post("RepresentClan", Network.EndPoint.ITerritoryControlMinigameService, "access_token", Token, "clanid", ClanID);
            await GetPlayerInfo();
        }

    }
}
