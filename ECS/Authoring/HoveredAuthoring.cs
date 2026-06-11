using Unity.Entities;
using UnityEngine;

public class HoveredAuthoring : MonoBehaviour 
{
    public class Baker : Baker<HoveredAuthoring> {
        public override void Bake(HoveredAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Hovered {
            });
        }
    }
}
public struct Hovered : IComponentData
{
    public bool onHover;
    public bool onUnhover;
    public bool onDeselected;
    public bool onSelected;
}