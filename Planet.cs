#pragma warning disable 4014

using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Saliens
{
    /// <summary>
    /// Data Class Only
    /// </summary>
    internal class PlanetResponse
    {
        [JsonProperty(PropertyName = "planets", Required = Required.Always)]
        public List<Planet> Planets { get; private set; }
    }

    internal class PlanetEqualityComparer : IEqualityComparer<Planet>
    {
        public bool Equals(Planet A, Planet B)
        {
            if (ReferenceEquals(A, B) || A.ID == B.ID) return true;
            return false;
        }
        public int GetHashCode(Planet obj)
        {
            return base.GetHashCode();
        }
    }
    public class Planet
    {
        #region static fields
        private static PlanetEqualityComparer PlanetEqualityComparer = new PlanetEqualityComparer { };
        private static ConcurrentDictionary<int, Planet> _cache = new ConcurrentDictionary<int, Planet>();
        #endregion

        #region JSON   
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
        #endregion

        /// <summary>
        /// Gets the first zone it can find.  Sorted by difficulty, then by capture progress.
        /// </summary>
        public Zone FirstAvailableZone => Zones.Where(x => !x.Captured).OrderByDescending(x => x.Difficulty).ThenBy(x => x.CaptureProgress).First();

        /// <summary>
        /// Gets only zones that match the difficulty.
        /// </summary>
        /// <param name="difficulty">Difficulty of the zones to find.  (Easy, Medium, Hard)</param>
        /// <returns>The zones that matched the filter.</returns>
        public Zone[] FilterAvailableZones(ZoneDifficulty difficulty) => Zones.Where(x => x.Captured == false && x.Difficulty == difficulty).ToArray();


        /// <summary>
        /// Gets the zone that matches zoneID.
        /// </summary>
        /// <param name="zoneID">ID of the zone.</param>
        /// <returns></returns>
        public Zone GetZoneFromID(int zoneID) => Zones.Where(x => x.GameID == zoneID).FirstOrDefault();

        /// <summary>
        /// Gets the zone that matches zonePosition.
        /// </summary>
        /// <param name="zonePosition">Position of the zone.</param>
        /// <returns></returns>
        public Zone GetZoneFromPosition(int zonePosition) => Zones.Where(x => x.Position == zonePosition).FirstOrDefault();


        

        /// <summary>
        /// Get a single planet's info
        /// </summary>
        /// <param name="ID">The planet's ID</param>
        /// <returns>The planet reference.</returns>
        public static Planet UpdatePlanet(int ID)
        {
            string JSON = Network.Get("GetPlanet", Network.EndPoint.ITerritoryControlMinigameService, "id", ID.ToString(), "language", "english").GetAwaiter().GetResult();
            Planet planet = Network.Deserialize<PlanetResponse>(JSON).Planets[0];
            return _cache.AddOrUpdate(ID, planet, (key, oldvalue) => oldvalue = planet);
        }

        private static Planet UpdatePlanet(Planet planet) => UpdatePlanet(planet.ID);


        public static Planet Get(int ID) => _cache.GetOrAdd(ID, UpdatePlanet(ID));




        /// <summary>
        /// Gets the planets in the game.
        /// </summary>
        /// <param name="ActiveOnly">Only get the planets that are currently active.</param>
        /// <returns>Planets that match the filter.</returns>
        public static IReadOnlyList<Planet> GetPlanets(bool ActiveOnly = true)
        {
            string JSON = Network.Get("GetPlanets", Network.EndPoint.ITerritoryControlMinigameService, "active_only", ActiveOnly.AsInt(), "language", "english").GetAwaiter().GetResult();
            List<Planet> planets = Network.Deserialize<PlanetResponse>(JSON).Planets;
            foreach (Planet planet in Network.Deserialize<PlanetResponse>(JSON).Planets)
            {
                UpdatePlanet(planet);
            }
            return planets;
        }

        private static async Task<int> Setup()
        {
            try
            {
                GetPlanets(false);
                UpdateActivePlanets();
            }
            catch (GameException GameDown) when (GameDown is GameDownException || (GameDown is InvalidGameResponse InvalidResponse && InvalidResponse.EResult == 0))
            {
                Console.WriteLine("Game Down -> Waiting for 30 seconds.");
                await Task.Delay(60 * 1000);
                await Setup();
            }

            return 1;
            
        }

        
        private static async Task<int> UpdateActivePlanets()
        {
            while (true)
            {
                try
                {
                    await Task.Delay(15 * 1000);
                    IReadOnlyList<Planet> planets = GetPlanets(true);
                    foreach (Planet planet in _cache.Values.Except(planets, PlanetEqualityComparer).Where(x => x.Info.Active && !x.Info.Captured))
                    {
                        UpdatePlanet(UpdatePlanet(planet));
                    }
                    Console.WriteLine("Active Planets Updated.");
                }
                catch (GameException GameDown) when (GameDown is GameDownException || (GameDown is InvalidGameResponse InvalidResponse && InvalidResponse.EResult == 0))
                {
                    //Game Died, Ignore Error.
                }
            }
        }

        /// <summary>
        /// Gets the planets in the game.
        /// </summary>
        /// <returns>Planets that match the filter.</returns>
        /// <remarks>This will get all of the information, done in parallel.</remarks>
        public static IEnumerable<Planet> All => _cache.Values;

        public static IEnumerable<Planet> Active => _cache.Values.Where(x => x.Info.Active && !x.Info.Captured);
        public static IEnumerable<Planet> Captured => _cache.Values.Where(x => x.Info.Captured);
        public static IEnumerable<Planet> Locked => _cache.Values.Where(x => !x.Info.Active && !x.Info.Captured);

        /// <summary>
        /// Finds the first planet with the hardest difficulty zone, then least captured.
        /// </summary>
        public static Planet FirstAvailable
        {
            get
            {
                ZoneDifficulty difficulty = ZoneDifficulty.Hard;

                while (difficulty != 0)
                {
                    foreach (Planet planet in Active.OrderBy(x => x.Info.CaptureProgress))
                    {
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
