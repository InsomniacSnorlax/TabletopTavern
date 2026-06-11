using Unity.Entities;
using UnityEngine;

public struct CameraTag : IComponentData
{
}
// [DisallowMultipleComponent]
public class CameraTagAuthoring : MonoBehaviour
{
    class Baker : Baker<CameraTagAuthoring>
    {
        public override void Bake(CameraTagAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent<CameraTag>(entity);
        }
    }
}
