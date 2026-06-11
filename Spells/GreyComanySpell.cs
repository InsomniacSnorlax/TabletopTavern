using System.Collections;
using UnityEngine;
using Memori.Audio;
using Unity.Mathematics;
using Unity.Entities;

namespace TJ.Spells
{
public class GreyComanySpell : MonoBehaviour
{
    [SerializeField] private SpellName spellName;
    public SpellName SpellName => spellName;

    [Header("Visual Effect")]
    [SerializeField] private GameObject spellWarmupEffect;
    [SerializeField] private GameObject spellVisualEffect;

    SquadEntity squadEntity;
    SpellData spellData;

    public void Load(float3 position, SquadEntity _squadEntity)
    {
        squadEntity = _squadEntity;
        transform.position = position;
        StartCoroutine(WarmUpSpell());
    }
    public void Load(SpellData _spellData, float3 position)
    {
        spellData = _spellData;
        transform.position = position;
        StartCoroutine(WarmUpSpell());
    }
    private void Update()
    {
        if(spellData.SpellTargetingType == SpellTargetingType.Squad) {
            // transform.position = squadEntity.TrueSquadCenter;
        }
    }
    private IEnumerator WarmUpSpell()
    {
        if (!string.IsNullOrEmpty(spellData.spellWarmupSound)) IAudioRequester.Instance.PlaySFX(spellData.spellWarmupSound);
        if (spellWarmupEffect != null) spellWarmupEffect.SetActive(true);
        yield return new WaitForSeconds(spellData.SpellWarmUpDuration);
        // if (spellWarmupEffect != null) spellWarmupEffect.SetActive(false);

        CastSpell();
    }
    private void CastSpell()
    {
        if (spellVisualEffect != null) spellVisualEffect.SetActive(true);
        if (!string.IsNullOrEmpty(spellData.spellHitSound)) IAudioRequester.Instance.PlaySFX(spellData.spellHitSound);

        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var ecb = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>().CreateCommandBuffer();
        Entity squadEntity = entityManager.CreateEntity();

        ecb.AddComponent(squadEntity, new SpellEntity { 
            Entity = squadEntity, 
            DamageBufferElement = spellData.damageBufferElement,
            SpellPosition = transform.position,
            SpellRadius = spellData.SpellRadius,
            IsOneOff = spellData.IsOneOff,
            SpellForce = spellData.SpellForce
        });

        StartCoroutine(CleanUpSpell());
    }
    private IEnumerator CleanUpSpell()
    {
        yield return new WaitForSeconds(spellData.SpellDuration);
        Destroy(gameObject);
    }
}
}
