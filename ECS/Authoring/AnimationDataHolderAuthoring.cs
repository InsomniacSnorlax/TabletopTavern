using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class AnimationDataHolderAuthoring : MonoBehaviour
{
    [HideInInspector] public GameObject gpuEcsAnimatorEntity;
    // private int idleAnimationId = 0;
    // private int walkAnimationId = 1;
    // private int runningAnimationId = 2;
    // private int idleAttackAnimationId = 3;
    // private int attackanimationId = 4;
    // private int deathAnimationId1 = 8;
    // private int deathAnimationId2 = 9;
    // private int deathAnimationId3 = 10;
    // private int thrownAnimationId = 11;

    class Baker : Baker<AnimationDataHolderAuthoring>
    {
        public override void Bake(AnimationDataHolderAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new AnimationDataHolder {
                gpuEcsAnimatorEntity = GetEntity(authoring.gpuEcsAnimatorEntity, TransformUsageFlags.Dynamic),
                currentIdleAnimationId = 0,
                walkAnimationId = 1,
                runningAnimationId = 2,

                idleAnimationId = 0,
                attackIdleAnimationId = TabletopTavernConstants.MELEE_ATTACK_IDLE_ID,

                attackanimationId = TabletopTavernConstants.MELEE_ATTACK_ID,

                deathAnimationId1 = 8,
                deathAnimationId2 = 9,
                deathAnimationId3 = 10,
                thrownAnimationId = 11,
                altattackanimationId = 12,

                idleAnimationIds = new int3 { x = 5, y = 6, z = 7 }               
            });
            SetComponentEnabled<AnimationDataHolder>(entity, true);
        }
    }
}

public struct AnimationDataHolder : IComponentData, IEnableableComponent
{
    public Entity gpuEcsAnimatorEntity;
    public int currentIdleAnimationId;

    public int idleAnimationId;
    public int attackIdleAnimationId;

    public int walkAnimationId;
    public int runningAnimationId;

    public int attackanimationId;
    public int deathAnimationId1, deathAnimationId2, deathAnimationId3;
    public int thrownAnimationId;
    public int3 idleAnimationIds;
    public int altattackanimationId;
    public float RunSpeedThreshold;
    public float WalkSpeedThreshold;
}