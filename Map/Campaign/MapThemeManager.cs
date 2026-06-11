using System.Collections.Generic;
using Memori.Utilities;
using UnityEngine;

namespace TJ.Map
{
    public class MapThemeManager : Singleton<MapThemeManager>
    {
        [SerializeField] private MapRegion GoblinHills;
        [SerializeField] private MapRegion PlagueLands;
        
        [SerializeField] private MapRegion TheMeadowlands;
        [SerializeField] private MapRegion GlacialWastes;
        [SerializeField] private MapRegion RisingIsles;

        [SerializeField] private MapRegion AncientGlade;
        [SerializeField] private MapRegion DeepJungle;
        [SerializeField] private MapRegion BarrenCliffs;


        public MapRegion GetMapRegion(Race race)
        {
            return race switch
            {
                Race.Gruntkin => GoblinHills,
                Race.SanguineCourt => PlagueLands,

                Race.IronLegion => TheMeadowlands,
                Race.RavenHost => GlacialWastes,
                Race.SakuraDynasty => RisingIsles,

                Race.TaelindorForest => AncientGlade,
                Race.DrakosaurBrood => DeepJungle,
                Race.DeepstoneHold => BarrenCliffs,

                _ => GoblinHills
            };
        }
        public List<MapRegion> GetAllMapRegions()
        {
            return new List<MapRegion> { GoblinHills, PlagueLands, GlacialWastes, TheMeadowlands,RisingIsles, AncientGlade, DeepJungle, BarrenCliffs };
        }
    }
}
