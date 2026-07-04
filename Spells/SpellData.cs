using System.Collections.Generic;
using UnityEngine;

namespace TJ.Spells
{
    public enum SpellType { None, LightningStrike, NaturesWrath, Heal, Fireball }
    public enum SpellTargetingType { World, Squad }

    [CreateAssetMenu(fileName = "SpellData", menuName = "GameData/SpellData", order = 1)]
    public class SpellData : ScriptableObject
    {
        public SpellType SpellType;
        public Race Race;
        public List<int> SpellModifierValues;
        public SpellTargetingType SpellTargetingType;
        public float SpellCooldown;
        public Sprite SpellSprite;
        public DamageBufferElement damageBufferElement;
        public float SpellRadius;
        public float SpellWarmUpDuration;
        public float SpellDuration;
        public float SpellForce;
        public bool IsOneOff;
        public string spellWarmupSound;
        public string spellHitSound;
        public Team TargetTeam;
        public ActiveSpell SpellPrefab;
    }
}