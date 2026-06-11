using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class RotateUnitAuthoring : MonoBehaviour 
{
    public class Baker : Baker<RotateUnitAuthoring> {
        public override void Bake(RotateUnitAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new RotateUnit {
                targetRotation = quaternion.identity,
            });
            SetComponentEnabled<RotateUnit>(entity, false);
            AddComponent<InMeleeRange>(entity);
            SetComponentEnabled<InMeleeRange>(entity, false);
        }
    }
}