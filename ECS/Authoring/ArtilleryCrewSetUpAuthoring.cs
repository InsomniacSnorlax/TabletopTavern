using Unity.Entities;
using UnityEngine;

public class ArtilleryCrewSetUpAuthoring : MonoBehaviour
{
    public class Baker : Baker<ArtilleryCrewSetUpAuthoring> {
        public override void Bake(ArtilleryCrewSetUpAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ArtilleryCrewSetUpEntity {
            });
        }
    }
}

public struct ArtilleryCrewSetUpEntity : IComponentData {
}