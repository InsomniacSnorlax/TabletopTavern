using Unity.Entities;
using UnityEngine;

public class BowSetUpAuthoring : MonoBehaviour
{
    public class Baker : Baker<BowSetUpAuthoring> {
        public override void Bake(BowSetUpAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new BowSetUpEntity {
            });
        }
    }
}

public struct BowSetUpEntity : IComponentData {
}