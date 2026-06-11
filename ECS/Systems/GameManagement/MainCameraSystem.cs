using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Unity.Burst;
[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial struct MainCameraSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MainCamera>();
    }

    // [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        LocalToWorld mainCameraLtW = SystemAPI.GetComponent<LocalToWorld>(SystemAPI.GetSingletonEntity<MainCamera>());
        if (Camera.main != null)
        {
            Camera.main.transform.SetPositionAndRotation(mainCameraLtW.Position, mainCameraLtW.Rotation);
        }
    }
}