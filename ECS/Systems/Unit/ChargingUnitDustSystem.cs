using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
partial struct ChargingUnitDustSystem : ISystem
{
    private float _timer;
    private Random _random;
    private const float Interval = 0.5f;
    private const float SpawnChance = 0.1f;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<DustCloudBufferElement>();
        _random = Random.CreateFromIndex(0);
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        _timer += SystemAPI.Time.DeltaTime;
        if (_timer < Interval) return;
        _timer = 0f;

        var dustBuffer = SystemAPI.GetSingletonBuffer<DustCloudBufferElement>();

        foreach (var (unit, transform) in
            SystemAPI.Query<RefRO<Unit>, RefRO<LocalTransform>>())
        {
            if (unit.ValueRO.unitState != UnitState.Charge) continue;
            if (_random.NextFloat() > SpawnChance) continue;

            dustBuffer.Add(new DustCloudBufferElement { Position = transform.ValueRO.Position });
        }
    }
}
