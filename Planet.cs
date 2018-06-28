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
        #region Static Fields
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

        #region Instance Properties
        [JsonIgnore]
        public IEnumerable<Zone> ActiveZones => Zones.Where(z => !z.Captured);
        [JsonIgnore]
        public IEnumerable<Zone> CapturedZones => Zones.Where(z => z.Captured);
        /// <summary>
        /// Gets only zones that match the difficulty.
        /// </summary>
        /// <param name="difficulty">Difficulty of the zones to find.  (Easy, Medium, Hard)</param>
        /// <returns>The zones that matched the filter.</returns>
        public IEnumerable<Zone> FilterAvailableZones(ZoneDifficulty difficulty, ZoneType type = ZoneType.Normal) => Zones.Where(x => x.Captured == false && x.Difficulty == difficulty);
        /// <summary>
        /// Gets the first zone it can find.  Sorted by difficulty, then by capture progress.
        /// </summary>
        public Zone FirstAvailableZone => Zones.Where(x => !x.Captured).OrderByDescending(x => x.Type).ThenByDescending(x => x.Difficulty).ThenByDescending(x => x.CaptureProgress).First();
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
        #endregion

        #region Static Properties
        [JsonIgnore]
        public static IEnumerable<Planet> All => _cache.Values;
        [JsonIgnore]
        public static IEnumerable<Planet> Active => _cache.Values.Where(x => x.Info.Active && !x.Info.Captured);
        [JsonIgnore]
        public static IEnumerable<Planet> Captured => _cache.Values.Where(x => x.Info.Captured);
        /// <summary>
        /// Finds the first planet with the hardest difficulty zone, then least captured.
        /// </summary>
        [JsonIgnore]
        public static IEnumerable<Planet> SortedPlanets
        {
            get
            {
                ZoneType type = ZoneType.Boss;
                ZoneDifficulty difficulty = ZoneDifficulty.Hard;
                List<Planet> planets = new List<Planet>();
                while (type != ZoneType.Invalid)
                {
                    while (difficulty != ZoneDifficulty.Invalid)
                    {
                        foreach (Planet planet in Active.Except(planets).OrderByDescending(x => x.Info.CaptureProgress))
                        {
                            if (planet.FilterAvailableZones(difficulty, type).Count() > 0) planets.Add(planet);
                        }
                        difficulty--;
                    }
                    type--;
                }
                if (planets.Count() > 0) return planets;
                throw new NoPlanetException();
            }
        }
        [JsonIgnore]
        public static IEnumerable<Planet> Locked => _cache.Values.Where(x => !x.Info.Active && !x.Info.Captured);
        #endregion

        #region Static Methods
        public static async Task<Planet> Get(int ID) => _cache.GetOrAdd(ID, await Update(ID));
        /// <summary>
        /// Gets the planets in the game.
        /// </summary>
        /// <param name="ActiveOnly">Only get the planets that are currently active.</param>
        /// <returns>Planets that match the filter.</returns>
        private static async Task<IReadOnlyList<Planet>> MultiUpdate(bool ActiveOnly = true)
        {
            string JSON = await Network.Get("GetPlanets", Network.EndPoint.ITerritoryControlMinigameService, "active_only", ActiveOnly.AsInt(), "language", "english");
            List<Planet> planets = Network.Deserialize<PlanetResponse>(JSON).Planets;
            if (ActiveOnly) planets.AddRange(Active.Except(planets, PlanetEqualityComparer));
            return await Task.WhenAll(planets.Select(x => Update(x)));
        }
        public static async Task<Planet> Update(int ID)
        {
            string JSON = await Network.Get("GetPlanet", Network.EndPoint.ITerritoryControlMinigameService, "id", ID.ToString(), "language", "english");
            Planet planet = Network.Deserialize<PlanetResponse>(JSON).Planets[0];
            return _cache.AddOrUpdate(ID, planet, (key, oldvalue) => oldvalue = planet);
        }
        public static async Task<Planet> Update(Planet planet) => await Update(planet.ID);
        public static async Task<IReadOnlyList<Planet>> UpdateActive() => await MultiUpdate(true);
        public static async Task<IReadOnlyList<Planet>> UpdateAll() => await MultiUpdate(false); //Usually you only need to call this ONE time.
        #endregion
    }

    public static class IEnumerablePlanetExtensions
    {
        public static IEnumerable<Zone> AllZones(this IEnumerable<Planet> planets) => planets.SelectMany(p => p.Zones);
        public static IEnumerable<Zone> ActiveZones(this IEnumerable<Planet> planets) => planets.SelectMany(p => p.ActiveZones);
        public static IEnumerable<Zone> CompletedZones(this IEnumerable<Planet> planets) => planets.SelectMany(p => p.CapturedZones);
        public static double CompletionPercent(this IEnumerable<Planet> planets) => Math.Round(planets.Sum(x => x.Info.CaptureProgress) / planets.Count() * 100, 2);

    }
}
