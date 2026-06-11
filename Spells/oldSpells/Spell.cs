// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// namespace TJ.Spells
// {
// public class Spell : MonoBehaviour
// {
//     [SerializeField] protected DamageBufferElement damage;
//     [SerializeField] protected GameObject spellPrefab;
//     [SerializeField] protected float spellLifetime;
//     [SerializeField] protected Vector3 spellCastPoint;
//     [SerializeField] protected int spellId;
//     [SerializeField] protected bool shakeOnSpellCast;
//     public bool ShakeOnSpellCast => shakeOnSpellCast;
//     public GameObject SpellPrefab => spellPrefab;
//     public int SpellType => spellId;
//     public int SpellId => spellId;

//     public virtual void LoadSpell(Vector3 _spellCastPoint, int _spellId)
//     {
//         spellCastPoint = _spellCastPoint;
//         spellId = _spellId;

//         Invoke(nameof(RemoveSpell), spellLifetime);
//     }
//     public virtual void RemoveSpell()
//     {
//         // Debug.Log($"Destroying spell {spellId} at {spellCastPoint} after {spellLifetime} seconds");
//         Destroy(gameObject);
//     }
// }
// }
