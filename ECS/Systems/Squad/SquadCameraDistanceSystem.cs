using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

[BurstCompile]
partial struct SquadCameraDistanceSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<CameraPositionComponent>();
        state.RequireForUpdate<SquadCameraDistanceComponent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float3 cameraPos = SystemAPI.GetSingleton<CameraPositionComponent>().Position;

        foreach (var (movement, distance) in
            SystemAPI.Query<RefRO<SquadMovementComponent>, RefRW<SquadCameraDistanceComponent>>())
        {
            distance.ValueRW.DistanceToCamera = math.distance(movement.ValueRO.SquadCenter, cameraPos);
        }
    }
}
