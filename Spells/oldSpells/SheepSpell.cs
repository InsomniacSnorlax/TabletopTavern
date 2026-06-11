// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// namespace TJ.Spells
// {
//     public class SheepSpell : SpellName
//     {
//     [SerializeField] private int castCount = 1;
//     [SerializeField] private int castIntervalMs = 1000;
//     [SerializeField] private float fallSpeed, aoeRadius;
//     [SerializeField] private string tagToHit;
//     [SerializeField] private GameObject spellProjectile;
//     [SerializeField] private GameObject sheepPrefab;

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
//     private void CastSpell()
//     {
//         // spellProjectile = GameManager.Instance.ObjectPoolingManager.GetSpellProjectile(spellId);
//         spellProjectile.transform.position = spellCastPoint;
//         SpellProjectile p = spellProjectile.GetComponentInChildren<SpellProjectile>(true);
//         p.DisableTurnOff();
//         projectiles.Add(spellProjectile);
//         spellProjectile.SetActive(true);

//         p.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
//         // p.SetStatsNoTarget(spellCastPoint, damage, fallSpeed, tagToHit, 0,  ProjectileType.AreaOfEffect, aoeRadius);

//         AOEHit();
//     }
//     private void AOEHit()
//     {
//         Collider[] colliders = new Collider[10];

//         // instantiate a sphere collider at the hit point
//         Physics.OverlapSphereNonAlloc(transform.position, aoeRadius, colliders, LayerMask.GetMask(tagToHit));
//         //get closest collider ot hit point
//         float minDistance = Mathf.Infinity;
//         Collider closestCollider = null;

//         foreach(Collider collider1 in colliders){
//             if(collider1 == null) continue;
//             float distance = Vector3.Distance(transform.position, collider1.transform.position);
//             if(distance < minDistance){
//                 minDistance = distance;
//                 closestCollider = collider1;
//             }
//         }

//         if(closestCollider != null){
//             // Debug.Log($"Hit {closestCollider.name}");
//             Instantiate(sheepPrefab, closestCollider.transform.position, closestCollider.transform.rotation, closestCollider.transform);
//             // closestCollider.gameObject.GetComponent<Enemy>().Sheepify();
//         }
//     }
//     }
// }
