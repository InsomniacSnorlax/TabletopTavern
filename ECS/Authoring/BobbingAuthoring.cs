using UnityEngine;
using ProjectDawn.Navigation.Hybrid;
using Unity.Entities;
using Unity.Mathematics;
using ProjectDawn.Navigation;
using Unity.Transforms;

public class BobbingAuthoring : MonoBehaviour
{
    public GameObject entityToBob;
    public bool StartEnabled = false;
    // public bool playAnimations = false;
    public float Amplitude = 1f;
    public float Frequency = 30f;
    class Baker : Baker<BobbingAuthoring>
    {
        public override void Bake(BobbingAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new BobbingComponent
            {
                entityToBob = GetEntity(authoring.entityToBob, TransformUsageFlags.Dynamic),
                InitialPosition = authoring.transform.position,
                Amplitude = authoring.Amplitude,
                Frequency = authoring.Frequency,
                randomOffset = UnityEngine.Random.Range(0, 10),
                // playAnimations = authoring.playAnimations
            });
            SetComponentEnabled<BobbingComponent>(entity, authoring.StartEnabled);
        }
    }
}

public struct BobbingComponent : IComponentData, IEnableableComponent
{
    public Entity entityToBob;
    public float3 InitialPosition;
    // public bool playAnimations;
    public float Amplitude;
    public float Frequency;
    public float randomOffset;
}