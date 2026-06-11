using UnityEngine;
using ProjectDawn.Navigation.Hybrid;
using Unity.Entities;
using Unity.Mathematics;
using ProjectDawn.Navigation;
using Unity.Transforms;
using Unity.Collections;

public class FlagAuthoring : MonoBehaviour
{
    public GameObject entityToBob;
    public float Amplitude = 1f;
    public float Frequency = 2f;
    class Baker : Baker<FlagAuthoring>
    {
        public override void Bake(FlagAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new FlagComponent
            {
                entityToBob = GetEntity(authoring.entityToBob, TransformUsageFlags.Dynamic),
                InitialPosition = authoring.transform.position,
                Amplitude = authoring.Amplitude,
                Frequency = authoring.Frequency
            });
        }
    }
}

public struct FlagComponent : IComponentData
{
    public Entity entityToBob;
    public int squadId;
    public float3 InitialPosition;
    public float Amplitude;
    public float Frequency;
}