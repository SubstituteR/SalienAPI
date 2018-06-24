using Newtonsoft.Json;
using System.Linq;

namespace Saliens
{
    /// <summary>
    /// Data Class Only
    /// </summary>
    internal class PlanetResponse
    {
        [JsonProperty(PropertyName = "planets", Required = Required.Always)]
        public Planet[] Planets { get; private set; }
    }

    public class Planet
    {
        [JsonProperty(PropertyName = "id", Required = Required.Always)]
        public int ID { get; private set; }

        [JsonProperty(PropertyName = "state", Required = Required.Always)]
        public PlanetState Info { get; private set; }

        [JsonProperty(PropertyName = "giveaway_apps", Required = Required.DisallowNull)]
        public int[] GiveawayApps { get; private set; }

        [JsonProperty(PropertyName = "top_clans", Required = Required.Always)]
        public ClanProgress[] TopClans { get; private set; }

        [JsonProperty(PropertyName = "zones", Required = Required.DisallowNull)]
        public Zone[] Zones { get; private set; }
        
        /// <summary>
        /// Gets the first zone it can find.  Sorted by difficulty, then by capture progress.
        /// </summary>
        public Zone FirstAvailableZone => Zones.Where(x => !x.Captured).OrderByDescending(x => x.Difficulty).ThenByDescending(x => x.CaptureProgress).First();

        /// <summary>
        /// Gets only zones that match the difficulty.
        /// </summary>
        /// <param name="difficulty">Difficulty of the zones to find.  (Easy, Medium, Hard)</param>
        /// <returns>The zones that matched the filter.</returns>
        public Zone[] FilterAvailableZones(ZoneDifficulty difficulty)
        {
            return Zones.Where(x => x.Captured == false && x.Difficulty == difficulty).ToArray();
        }

        /// <summary>
        /// Gets the zone that matches zoneID.
        /// </summary>
        /// <param name="zoneID">ID of the zone.</param>
        /// <returns></returns>
        public Zone GetZoneFromID(int zoneID)
        {
            return Zones.Where(x => x.GameID == zoneID).FirstOrDefault();
        }

        /// <summary>
        /// Gets the zone that matches zonePosition.
        /// </summary>
        /// <param name="zonePosition">Position of the zone.</param>
        /// <returns></returns>
        public Zone GetZoneFromPosition(int zonePosition)
        {
            return Zones.Where(x => x.Position == zonePosition).FirstOrDefault();
        }

        /// <summary>
        /// Get a single planet's info
        /// </summary>
        /// <param name="ID">The planet's ID</param>
        /// <returns>The planet information.</returns>
        public static Planet Get(int ID)
        {
            string JSON = Network.Get("GetPlanet", Network.EndPoint.ITerritoryControlMinigameService, "id", ID.ToString(), "language", "english").GetAwaiter().GetResult();
            return Network.Deserialize<PlanetResponse>(JSON).Planets[0];
        }


        /// <summary>
        /// Gets the planets in the game.
        /// </summary>
        /// <param name="ActiveOnly">Only get the planets that are currently active.</param>
        /// <returns>Planets that match the filter.</returns>
        public static Planet[] All(bool ActiveOnly = true)
        {
            string JSON = Network.Get("GetPlanets", Network.EndPoint.ITerritoryControlMinigameService, "active_only", ActiveOnly.AsInt(), "language", "english").GetAwaiter().GetResult();
            return Network.Deserialize<PlanetResponse>(JSON).Planets;
        }

        /// <summary>
        /// Finds the first planet with the hardest difficulty zone, then least captured.
        /// </summary>
        public static Planet FirstAvailable
        {
            get
            {
                ZoneDifficulty difficulty = ZoneDifficulty.Hard;
                Planet[] planets = All(true);
                while (difficulty != 0)
                {
                    foreach (int id in planets.OrderBy(x => x.Info.CaptureProgress).Select(x => x.ID))
                    {
                        Planet planet = Get(id);
                        if (planet.FilterAvailableZones(difficulty).Count() > 0)
                        {
                            return planet;
                        }
                    }
                    difficulty--;
                }
                throw new NoPlanetException();
            }
        }
    }
}
