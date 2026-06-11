using Unity.Entities;
using UnityEngine;

public class MeleeUnitAuthoring : MonoBehaviour
{
    class Baker : Baker<MeleeUnitAuthoring>
    {
        public override void Bake(MeleeUnitAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent<MeleeUnitTag>(entity);
        }
    }
}
