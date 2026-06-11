using Unity.Entities;
using UnityEngine;

public class SwordSetUpAuthoring : MonoBehaviour
{
    public class Baker : Baker<SwordSetUpAuthoring> {
        public override void Bake(SwordSetUpAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new SwordSetUpEntity {
            });
        }
    }
}

public struct SwordSetUpEntity : IComponentData {
}