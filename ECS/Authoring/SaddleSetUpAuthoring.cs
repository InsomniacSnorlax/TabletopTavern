using Unity.Entities;
using UnityEngine;

public class SaddleSetUpAuthoring : MonoBehaviour
{
    public class Baker : Baker<SaddleSetUpAuthoring> {
        public override void Bake(SaddleSetUpAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new SaddleSetUpEntity {
            });
        }
    }
}

public struct SaddleSetUpEntity : IComponentData {
}