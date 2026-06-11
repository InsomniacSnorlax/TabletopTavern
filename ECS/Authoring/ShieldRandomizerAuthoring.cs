using Unity.Entities;
using UnityEngine;

public class ShieldRandomizerAuthoring : MonoBehaviour
{
    public class Baker : Baker<ShieldRandomizerAuthoring> {
        public override void Bake(ShieldRandomizerAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ShieldRandomMesh { });
        }
    }
}

public struct ShieldRandomMesh : IComponentData {
}