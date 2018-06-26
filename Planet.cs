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


        public static Planet Update(int ID)
        {
            string JSON = Network.Get("GetPlanet", Network.EndPoint.ITerritoryControlMinigameService, "id", ID.ToString(), "language", "english").GetAwaiter().GetResult();
            Planet planet = Network.Deserialize<PlanetResponse>(JSON).Planets[0];
            return _cache.AddOrUpdate(ID, planet, (key, oldvalue) => oldvalue = planet);
        }
        public static Planet Update(Planet planet) => Update(planet.ID);
        public static Planet Get(int ID) => _cache.GetOrAdd(ID, Update(ID));
        public static IReadOnlyList<Planet> UpdateActive() => MultiUpdate(true);
        public static IReadOnlyList<Planet> UpdateAll() => MultiUpdate(false); //Usually you only need to call this ONE time.


        /// <summary>
        /// Gets the planets in the game.
        /// </summary>
        /// <param name="ActiveOnly">Only get the planets that are currently active.</param>
        /// <returns>Planets that match the filter.</returns>
        private static IReadOnlyList<Planet> MultiUpdate(bool ActiveOnly = true)
        {
            string JSON = Network.Get("GetPlanets", Network.EndPoint.ITerritoryControlMinigameService, "active_only", ActiveOnly.AsInt(), "language", "english").GetAwaiter().GetResult();
            List<Planet> planets = Network.Deserialize<PlanetResponse>(JSON).Planets;

            if (ActiveOnly) planets.AddRange(Active.Except(planets, PlanetEqualityComparer));

            foreach (Planet planet in planets)
            {
                Update(planet);
            }

            return planets;
        }

        [JsonIgnore]
        public static IEnumerable<Planet> All => _cache.Values;

        [JsonIgnore]
        public static IEnumerable<Planet> Active => _cache.Values.Where(x => x.Info.Active && !x.Info.Captured);

        [JsonIgnore]
        public static IEnumerable<Planet> Captured => _cache.Values.Where(x => x.Info.Captured);

        [JsonIgnore]
        public static IEnumerable<Planet> Locked => _cache.Values.Where(x => !x.Info.Active && !x.Info.Captured);



        [JsonIgnore]
        public IEnumerable<Zone> ActiveZones => Zones.Where(z => !z.Captured);

        [JsonIgnore]
        public IEnumerable<Zone> CapturedZones => Zones.Where(z => z.Captured);

        /// <summary>
        /// Finds the first planet with the hardest difficulty zone, then least captured.
        /// </summary>
        [JsonIgnore]
        public static Planet FirstAvailable
        {
            get
            {
                ZoneDifficulty difficulty = ZoneDifficulty.Boss;

                while (difficulty != 0)
                {
                    foreach (Planet planet in Active.OrderBy(x => x.Info.CaptureProgress))
                    {
                        if (planet.FilterAvailableZones(difficulty).Count() > 0) return planet;
                    }
                    difficulty--;
                }
                throw new NoPlanetException();
            }
        }
    }

    public static class IEnumerablePlanetExtensions
    {
        public static IEnumerable<Zone> AllZones(this IEnumerable<Planet> planets) => planets.SelectMany(p => p.Zones);
        public static IEnumerable<Zone> ActiveZones(this IEnumerable<Planet> planets) => planets.SelectMany(p => p.ActiveZones);
        public static IEnumerable<Zone> CompletedZones(this IEnumerable<Planet> planets) => planets.SelectMany(p => p.CapturedZones);
        public static double CompletionPercent(this IEnumerable<Planet> planets) => Math.Round(planets.Sum(x => x.Info.CaptureProgress) / planets.Count() * 100, 2);

    }
}
