// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// namespace TJ.Spells
// {
// public class MeteorRainSpell : SpellName
// {
//     [SerializeField] private float fallSpeed, aoeRadius;
//     [SerializeField] private string tagToHit;
//     GameObject spellProjectile;
//     SpellProjectile projectile;
//     public override void LoadSpell(Vector3 _spellCastPoint, int spellId)
//     {
//         base.LoadSpell(_spellCastPoint, spellId);

//         // GameManager.Instance.ObjectPoolingManager.CreateSpellPool(this);

//         //load gameobject from pool
//         // spellProjectile = GameManager.Instance.ObjectPoolingManager.GetSpellProjectile(spellId);
//         spellProjectile.transform.position = _spellCastPoint;

//         // float damageMultiplier = GameManager.Instance.GlobalModifierManager.GetGlobalModifier(ModifierType.SpellsDamage, TowerType.Global);
//         // damage.magicDamage *= damageMultiplier;
//         // damage.physicalDamage *= damageMultiplier;
        
//         //reset the projectile
//         SpellProjectile p = spellProjectile.GetComponentInChildren<SpellProjectile>(true);
//         p.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
//         // p.SetStatsNoTarget(_spellCastPoint, damage, fallSpeed, tagToHit, new(), 0, ProjectileType.AreaOfEffect, aoeRadius);
//         spellProjectile.SetActive(true);

//         Invoke(nameof(TurnOffSpellAtEnd), spellLifetime);
//         Invoke(nameof(RemoveSpell), spellLifetime);
//     }
//     public override void RemoveSpell()
//     {
//         base.RemoveSpell();
//     }
//     private void TurnOffSpellAtEnd()
//     {
//         spellProjectile.SetActive(false);
//     }
// }
// }
