using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(PresentationSystemGroup))]
[UpdateBefore(typeof(TriangleColorLerpSystem))]
partial struct HoveredEventSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach ((
            RefRW<Hovered> hovered,
            RefRW<TriangleEntity> triangle,
            Entity entity)
            in SystemAPI.Query<
                RefRW<Hovered>,
                RefRW<TriangleEntity>>()
                .WithPresent<TriangleEntity>()
                .WithEntityAccess())
        {
            bool isSelected = SystemAPI.IsComponentEnabled<Selected>(entity);

            if (hovered.ValueRO.onSelected) {
                triangle.ValueRW.colorTarget = triangle.ValueRO.selectedColor;
                SystemAPI.SetComponentEnabled<TriangleEntity>(entity, true);
            } else if (hovered.ValueRO.onDeselected) {
                triangle.ValueRW.colorTarget = triangle.ValueRO.disabledColor;
                SystemAPI.SetComponentEnabled<TriangleEntity>(entity, true);
            } else if (hovered.ValueRO.onHover && !isSelected) {
                triangle.ValueRW.colorTarget = triangle.ValueRO.hoverColor;
                SystemAPI.SetComponentEnabled<TriangleEntity>(entity, true);
            } else if (hovered.ValueRO.onUnhover && !isSelected) {
                triangle.ValueRW.colorTarget = triangle.ValueRO.disabledColor;
                SystemAPI.SetComponentEnabled<TriangleEntity>(entity, true);
            }

            hovered.ValueRW.onHover = false;
            hovered.ValueRW.onUnhover = false;
            hovered.ValueRW.onSelected = false;
            hovered.ValueRW.onDeselected = false;
        }
    }
}

[UpdateInGroup(typeof(PresentationSystemGroup))]
partial struct TriangleColorLerpSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = Time.unscaledDeltaTime;

        foreach ((RefRW<TriangleEntity> triangle, Entity entity) in SystemAPI.Query<RefRW<TriangleEntity>>().WithEntityAccess())
        {
            if (triangle.ValueRO.activeColor != triangle.ValueRO.colorTarget) {
                triangle.ValueRW.activeColor = Color.Lerp(triangle.ValueRO.activeColor, triangle.ValueRO.colorTarget, deltaTime * 10);
            } else if (triangle.ValueRO.colorTarget == triangle.ValueRO.disabledColor) {
                SystemAPI.SetComponentEnabled<TriangleEntity>(entity, false);
            }
        }
    }
}
