using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class ReturnToIdleAuthoring : MonoBehaviour
{
    class Baker : Baker<ReturnToIdleAuthoring>
    {
        public override void Bake(ReturnToIdleAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ReturnToIdle {
            });
            SetComponentEnabled<ReturnToIdle>(entity, true);
        }
    }
}

public struct ReturnToIdle : IComponentData, IEnableableComponent
{
    
}