using Unity.Entities;
using UnityEngine;

public class RangedUnitAuthoring : MonoBehaviour
{
    class Baker : Baker<RangedUnitAuthoring>
    {
        public override void Bake(RangedUnitAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent<RangedUnitTag>(entity);
        }
    }
}
