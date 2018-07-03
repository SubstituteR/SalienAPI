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

        public BossMatch BossMatch { get; private set; } = new BossMatch { };

        private void UpdateValues(PlayerInfo B)
        {
            ActivePlanetID = B.ActivePlanetID;
            ActiveZoneID = B.ActiveZoneID;
            ActiveZonePosition = B.ActiveZonePosition;
            ActiveBossGame = B.ActiveBossGame;
            Clan = B.Clan;
            Level = B.Level;
            NextLevelScore = B.NextLevelScore;
            Score = B.Score;
            TimeOnPlanet = B.TimeOnPlanet;
        }
        #region JSON

        [JsonProperty(PropertyName = "active_planet", Required = Required.DisallowNull)]
        private int ActivePlanetID { get; set; }

        [JsonProperty(PropertyName = "active_boss_game", Required = Required.DisallowNull)]
        private int ActiveBossGame { get; set; }

        [JsonProperty(PropertyName = "active_zone_game", Required = Required.DisallowNull)]
        private int ActiveZoneID { get; set; }

        [JsonProperty(PropertyName = "active_zone_position", Required = Required.DisallowNull)]
        private int ActiveZonePosition { get; set; }

        [JsonProperty(PropertyName = "clan_info", Required = Required.DisallowNull)]
        public ClanInfo Clan { get; private set; }

        [JsonProperty(PropertyName = "level", Required = Required.Always)]
        public int Level { get; private set; }

        [JsonProperty(PropertyName = "next_level_score", Required = Required.DisallowNull)]
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
        public Zone Zone
        {
            get
            {
                return Planet?.Zones.Where(x => x.GameID == ActiveZoneID).FirstOrDefault();
            }
            private set
            {
                Zone zone = Planet?.Zones.Where(x => x.GameID == value.GameID).FirstOrDefault();
                if (zone != null) zone = value;
            }
        }

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


        public async Task JoinBossZone(int ZonePosition)
        {
            if (InBossMatch && Planet.Zones[ZonePosition].IsActiveBossZone) return; //we're already in boss battle.
            await LeaveZone();
            ZoneResponse zoneResponse;
            zoneResponse = Network.Deserialize<ZoneResponse>(await Network.Post("JoinBossZone", Network.EndPoint.ITerritoryControlMinigameService, "access_token", Token, "zone_position", ZonePosition));
            BossMatch.Data.Waiting = zoneResponse.Waiting;
            BossMatch.HealLastUsed = DateTimeOffset.Now.AddSeconds(120);
            Zone = zoneResponse.Zone;
            await GetPlayerInfo();
        }

        public async Task JoinZone(int ZonePosition)
        {
            if (InMatch && Planet.Zones[ZonePosition].GameID == ActiveZoneID) return; //we're already in this zone.
            await LeaveZone();
            ZoneResponse zoneResponse;
            zoneResponse = Network.Deserialize<ZoneResponse>(await Network.Post("JoinZone", Network.EndPoint.ITerritoryControlMinigameService, "access_token", Token, "zone_position", ZonePosition));
            Zone = zoneResponse.Zone;
            await GetPlayerInfo();
        }

        private async Task Leave(int GameID)
        {
            if (GameID == 0) return;
            await Network.Post("LeaveGame", Network.EndPoint.IMiniGameService, "access_token", Token, "gameid", GameID);
            await GetPlayerInfo();
        }
        public async Task LeaveZone()
        {
            if (InBossMatch) await Leave(ActiveBossGame);
            if (InMatch) await Leave(ActiveZoneID);
        }

        public async Task LeavePlanet()
        {
            await LeaveZone();
            if (Planet != null) await Leave(ActivePlanetID);
        }
        public async Task ReportScore()
        {
            await ReportScore(MaxMatchScore);
        }

        public int MaxMatchScore => Zone?.MaxScore ?? 40;

        public async Task ReportScore(int Score)
        {
            await Network.Post("ReportScore", Network.EndPoint.ITerritoryControlMinigameService, "access_token", Token, "score", Clamp(Score, 0, MaxMatchScore));
            await GetPlayerInfo();
        }

        public async Task ReportBossDamage(int DamageTaken = 0, bool UsedHeal = false)
        {
            await ReportBossDamage(MaxMatchScore, DamageTaken, UsedHeal);
        }

        public async Task ReportBossDamage(int DamageToBoss, int DamageTaken = 0, bool UsedHeal = false)
        {
            BossMatch.Data = Network.Deserialize<BossData>(await Network.Post("ReportBossDamage", Network.EndPoint.ITerritoryControlMinigameService, "access_token", Token, "damage_to_boss", Clamp(Score, 0, MaxMatchScore), "damage_taken", DamageTaken, "use_heal_ability", UsedHeal.AsInt()));
            await GetPlayerInfo();
        }

        public bool InBossMatch => ActiveBossGame > 0;
        public bool InMatch => ActiveZoneID > 0;

        public int Clamp(int A, int L, int U)
        {
            if (A < L) return L; if (A > U) return U; return A;
        }

        [JsonIgnore]
        public int ScoreForZone
        {
            get
            {
                if (InBossMatch) return 40;
                if (InMatch) return Zone.MaxScore;
                return 0;
            }
        }

        public async Task RepresentClan(int ClanID)
        {
            await Network.Post("RepresentClan", Network.EndPoint.ITerritoryControlMinigameService, "access_token", Token, "clanid", ClanID);
            await GetPlayerInfo();
        }

    }
}
