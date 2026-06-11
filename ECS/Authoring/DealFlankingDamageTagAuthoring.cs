using Unity.Entities;
using UnityEngine;

public class DealFlankingDamageTagAuthoring : MonoBehaviour
{
    class Baker : Baker<DealFlankingDamageTagAuthoring>
    {
        public override void Bake(DealFlankingDamageTagAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent<DealFlankingDamageTag>(entity);
            SetComponentEnabled<DealFlankingDamageTag>(entity, false);
        }
    }
}
