// using System.Collections;
// using System.Collections.Generic;
// using System.Threading.Tasks;
// using UnityEngine;
// using Memori.Utilities;

// namespace TJ.Spells
// {
// public class AOESpell : Spell
// {
//     [SerializeField] private int castCount = 1;
//     [SerializeField] private int castIntervalMs = 1000;
//     [SerializeField] private float fallSpeed, aoeRadius;
//     [SerializeField] private string tagToHit, castSound;
//     [SerializeField] private GameObject spellProjectile;

//     List<GameObject> projectiles = new();
//     public override void LoadSpell(Vector3 _spellCastPoint, int _spellId)
//     {
//         // Debug.Log($"Loading spell {_spellId} at {_spellCastPoint}");wwwww
//         base.LoadSpell(_spellCastPoint, _spellId);

//         CastSpell();
//     }
//     public override void RemoveSpell()
//     {
//         foreach (var p in projectiles) {
//             p.SetActive(false);
//         }
//         base.RemoveSpell();
//     }
//     private async void CastSpell()
//     {
//         spellProjectile = Instantiate(spellPrefab, transform);
//         spellProjectile.layer = LayerMask.NameToLayer("Spell");
        
//         spellProjectile.transform.position = spellCastPoint;
//         SpellProjectile p = spellProjectile.GetComponentInChildren<SpellProjectile>(true);
//         p.DisableTurnOff();
//         projectiles.Add(spellProjectile);
//         spellProjectile.SetActive(true);

//         // if(!string.IsNullOrEmpty(castSound)) Runtime.Instance.IAudioRequester.PlaySFX(castSound);

//         // float damageMultiplier = GameManager.Instance.GlobalModifierManager.GetGlobalModifier(ModifierType.SpellsDamage, TowerType.Global);
//         // damage.magicDamage *= damageMultiplier;
//         // damage.physicalDamage *= damageMultiplier;

//         while (castCount > 0)
//         {
//             p.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
//             // p.SetStatsNoTarget(spellCastPoint, damage, fallSpeed, tagToHit, new(), 0, 
//             //     ProjectileType.AreaOfEffect, aoeRadius, damage.statusEffectEnums);

//             castCount--;
//             await HelperFunctions.DelayWithTimeScale(castIntervalMs);
//         }
//         // Debug.Log($"Spell {spellId} has finished casting");
//     }
// }
// }
