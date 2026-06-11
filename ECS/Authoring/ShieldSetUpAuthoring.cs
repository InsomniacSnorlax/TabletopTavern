using Unity.Entities;
using UnityEngine;

public class ShieldSetUpAuthoring : MonoBehaviour
{
    public class Baker : Baker<ShieldSetUpAuthoring> {
        public override void Bake(ShieldSetUpAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ShieldSetUpEntity {
            });
        }
    }
}

public struct ShieldSetUpEntity : IComponentData {
}