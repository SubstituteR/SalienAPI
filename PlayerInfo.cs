using Newtonsoft.Json;
using System;

namespace Saliens
{
    public class PlayerInfo
    {
        [JsonProperty(PropertyName = "active_planet", Required = Required.DisallowNull)]
        public int ActivePlanetID { get; private set; }

        [JsonProperty(PropertyName = "time_on_planet", Required = Required.DisallowNull)]
        public int TimeOnPlanet { get; private set; }

        [JsonProperty(PropertyName = "active_zone_game", Required = Required.DisallowNull)]
        public int ActiveZoneID { get; private set; }

        [JsonProperty(PropertyName = "active_zone_position", Required = Required.DisallowNull)]
        public int ActiveZonePosition { get; private set; }

        [JsonProperty(PropertyName = "clan_info", Required = Required.DisallowNull)]
        public ClanInfo ClanInfo { get; private set; }
    
        [JsonProperty(PropertyName = "score", Required = Required.Always)]
        public int Score { get; private set; }

        [JsonProperty(PropertyName = "level", Required = Required.Always)]
        public int Level { get; private set; }

        [JsonProperty(PropertyName = "next_level_score", Required = Required.Always)]
        public int NextLevelScore { get; private set; }

        [JsonIgnore]
        public Planet Planet { get; set; }

        [JsonIgnore]
        public Zone Zone { get; set; }

        [JsonIgnore]
        private string Token { get; set; }


        /// <summary>
        /// Joins the planet specified.
        /// </summary>
        /// <param name="PlanetID">The planet to join.</param>
        /// <param name="refresh">Refresh player data.  If you are unsure, leave this as the default value.</param>
        public void JoinPlanet(int PlanetID, bool refresh = true)
        {
            LeavePlanet(false);
            Network.Post("JoinPlanet", Network.EndPoint.ITerritoryControlMinigameService, "access_token", Token, "id", PlanetID).GetAwaiter().GetResult();
            if (refresh) Update();
        }

        /// <summary>
        /// Joins the zone specified
        /// </summary>
        /// <param name="ZonePosition">The Position of the zone</param>
        //// <param name="refresh">Refresh player data.  If you are unsure, leave this as the default value.</param>
        public void JoinZone(int ZonePosition, bool refresh = true)
        {
            Network.Post("JoinZone", Network.EndPoint.ITerritoryControlMinigameService, "access_token", Token, "zone_position", ZonePosition).GetAwaiter().GetResult();
            if (refresh) Update();
        }

        /// <summary>
        /// Leaves your current game.
        /// </summary>
        /// <param name="refresh">Refresh player data.  If you are unsure, leave this as the default value.</param>
        public void LeaveGame(bool refresh = true)
        {
            if (ActiveZoneID != 0)
            {
                Leave(ActiveZoneID, refresh);
            }
        }

        /// <summary>
        /// Leaves your current planet.
        /// </summary>
        /// <param name="refresh">Refresh player data.  If you are unsure, leave this as the default value.</param>
        public void LeavePlanet(bool refresh = true)
        {
            LeaveGame(false);
            if (ActivePlanetID != 0)
            {
                Leave(ActivePlanetID, refresh);
            }
        }

        /// <summary>
        /// Forces the PlayerInfo values to be redownloaded from Steam.
        /// </summary>
        public void Update()
        {
            Console.WriteLine("Updated PlayerInfo");
            Network.PopulateObject(this, Get(Token));
        }



        /// <summary>
        /// Internal function for POSTing leave messages.
        /// </summary>
        /// <param name="GameID">Game to leave.</param>
        /// <param name="refresh">Decides if player data is refreshed.</param>
        private void Leave(int GameID, bool refresh = true)
        {
            Network.Post("LeaveGame", Network.EndPoint.IMiniGameService, "access_token", Token, "gameid", GameID).GetAwaiter().GetResult();
            if (refresh) Update();
        }

        /// <summary>
        /// Changes the clan that you represent.
        /// </summary>
        /// <param name="ClanID">Clan to Represent</param>
        /// <param name="refresh">Refresh player data.  If you are unsure, leave this as the default value.</param>
        public void RepresentClan(int ClanID, bool refresh = true)
        {
            Network.Post("RepresentClan", Network.EndPoint.ITerritoryControlMinigameService, "access_token", Token, "clanid", ClanID).GetAwaiter().GetResult();
            if (refresh) Update();
        }

        /// <summary>
        /// Reports the score for the current zone to Steam.
        /// </summary>
        /// <param name="refresh">Refresh player data.  If you are unsure, leave this as the default value.</param>
        public void ReportScore(bool refresh = true)
        {
            try
            {
                Network.Post("ReportScore", Network.EndPoint.ITerritoryControlMinigameService, "access_token", Token, "score", Zone.Score).GetAwaiter().GetResult();
                if (refresh) Update();
            }
            catch (Exception)
            {
                Console.WriteLine("Score Submission Failed");
            }
        }


        /// <summary>
        /// Retrieves the player data for the provided token.
        /// </summary>
        /// <param name="Token">Token to get player data for.</param>
        /// <returns>A PlayerInfo object.</returns>
        public static PlayerInfo Get(string Token)
        {
            string JSON = Network.Post("GetPlayerInfo", Network.EndPoint.ITerritoryControlMinigameService, "access_token", Token).GetAwaiter().GetResult();
            PlayerInfo player = Network.Deserialize<PlayerInfo>(JSON);
            player.Token = Token;
            if (player.ActivePlanetID != 0)
            {
                player.Planet = Planet.Get(player.ActivePlanetID);
                if (player.ActiveZoneID != 0)
                {
                    player.Zone = player.Planet.GetZoneFromID(player.ActiveZoneID);
                }
            }
            return player;
        }

    }
}
