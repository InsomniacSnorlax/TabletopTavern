using System.Collections;
using UnityEngine;
using Memori.Audio;
using Unity.Mathematics;
using Unity.Entities;

namespace TJ.Spells
{
public class ActiveSpell : MonoBehaviour
{
    [Header("Visual Effect")]
    [SerializeField] private GameObject spellWarmupEffect;
    [SerializeField] private GameObject spellVisualEffect;

    private SpellData spellData;
    private Entity targetSquadEntity = Entity.Null;

    public void Load(SpellData _spellData, float3 position, Entity _targetSquadEntity = default)
    {
        spellData = _spellData;
        targetSquadEntity = _targetSquadEntity;
        transform.position = position;
        StartCoroutine(WarmUpSpell());
    }
    private void Update()
    {
        if(spellData == null) return;

        if(spellData.SpellTargetingType == SpellTargetingType.Squad && targetSquadEntity != Entity.Null) {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            if(entityManager.Exists(targetSquadEntity) && entityManager.HasComponent<SquadMovementComponent>(targetSquadEntity)) {
                transform.position = entityManager.GetComponentData<SquadMovementComponent>(targetSquadEntity).SquadCenter;
            }
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
        Entity spellEntity = entityManager.CreateEntity();

        ecb.AddComponent(spellEntity, new SpellEntity {
            Entity = spellEntity,
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
