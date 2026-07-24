using System.Collections.Generic;
using UnityEngine;
using Memori.Audio;
using Memori.Localization;

namespace TJ.Spells
{
    public enum Spell { None, LesserMoraleSpell, LesserDamageSpell, LesserWindSpell, LesserWeaponStrengthSpell, LightningStrike, NaturesWrath, Heal, Fireball, SkeletalSummon }
    // World: raycast ground point, stays fixed. Squad: follows the target squad's live
    // position through warmup and damage resolution.
    public enum SpellTargetingType { World, Squad }
    public enum SpellType { AOE, SingleTarget }

    [CreateAssetMenu(fileName = "SpellData", menuName = "GameData/SpellData", order = 1)]
    public class SpellData : ScriptableObject
    {
        public Spell Spell;
        public Race Race;
        public int SpellModifierValue;
        public SpellTargetingType SpellTargetingType;
        public SpellType SpellType;
        public float SpellCooldown;
        public Sprite SpellSprite;
        public float SpellRadius;
        public float SpellWarmUpDuration;
        public float SpellDuration;
        public float SpellForce;
        public bool IsOneOff;
        public SFXReference warmupSound;
        public SFXReference hitSound;
        public Team TargetTeam;
        public ActiveSpell SpellPrefab;

        [Header("Battlefield Bonus")]
        public bool GrantsBattlefieldBonus;
        public UnitStat BonusUnitStat;
        public BattlefieldBonusEnum BonusType;

        [Header("Summon")]
        public bool SummonsSquad;
        // Spawns a friendly squad at the cast point that lasts until killed and is never written
        // back to the campaign save. Author these as SpellTargetingType.World.
        public UnitName SummonedUnitName;

        public string GetLocalizedSpellDescription()
        {
            string localizedSpellDescription = LocalizationManager.Instance.GetText(Spell.ToString() + "_Desc");
            if(string.IsNullOrEmpty(localizedSpellDescription)) return Spell.ToString();
            localizedSpellDescription = string.Format(localizedSpellDescription, SpellType, SpellModifierValue, SpellDuration);
            ColorData.XMLTagColorApplicator(ref localizedSpellDescription);
            return localizedSpellDescription;
        }
    }
}