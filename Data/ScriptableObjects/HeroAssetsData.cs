using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TJ
{
    [Serializable]
    public class HeroAssetEntry
    {
        public int HeroID;
        public AssetReferenceSprite Sprite;
        public AssetReferenceGameObject Prefab;
    }

    [CreateAssetMenu(fileName = "HeroAssetsData", menuName = "GameData/HeroAssetsData", order = 1)]
    public class HeroAssetsData : ScriptableObject
    {
        public List<HeroAssetEntry> Heroes;

        public HeroAssetEntry GetByID(int heroID)
        {
            foreach (HeroAssetEntry entry in Heroes)
            {
                if (entry.HeroID == heroID)
                    return entry;
            }
            return null;
        }
    }
}
