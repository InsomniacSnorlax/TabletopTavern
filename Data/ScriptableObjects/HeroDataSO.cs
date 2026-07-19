using UnityEngine;

namespace TJ
{
    [CreateAssetMenu(fileName = "HeroData", menuName = "GameData/HeroData", order = 1)]
    public class HeroDataSO : ScriptableObject
    {
        public Hero heroData;
    }
}
