using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class SquadPositionDebugAuthoring : MonoBehaviour {
    public class Baker : Baker<SquadPositionDebugAuthoring> {

        public override void Bake(SquadPositionDebugAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new SquadPositionDebug {
            });
        }
    }
}
public struct SquadPositionDebug : IComponentData {
}