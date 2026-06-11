using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.SocialPlatforms;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(TransformSystemGroup))]
[UpdateBefore(typeof(EndSimulationEntityCommandBufferSystem))]
public partial struct GameCameraSystem : ISystem
{
    private Unity.Mathematics.Random _random;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _random = Unity.Mathematics.Random.CreateFromIndex(0);
        
        state.RequireForUpdate<SimulationRate>();
        state.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<GameCamera>().Build());
        // Debug.Log($"GameCameraSystem OnCreate");
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float2 lookInput = new float2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        if (!Input.GetMouseButton(2)) {
            lookInput = float2.zero;
        }

        lookInput+= new float2((Input.GetKey(KeyCode.E) ? 0.2f : 0f) + (Input.GetKey(KeyCode.Q) ? -0.2f : 0f), 0);

        // Collect input
        CameraInputs cameraInputs = new CameraInputs
        {
            Move = new float3(
                (Input.GetKey(KeyCode.D) ? 1f : 0f) + (Input.GetKey(KeyCode.A) ? -1f : 0f),
                // (Input.GetKey(KeyCode.E) ? 1f : 0f) + (Input.GetKey(KeyCode.Q) ? -1f : 0f),
                -Input.mouseScrollDelta.y,
                (Input.GetKey(KeyCode.W) ? 1f : 0f) + (Input.GetKey(KeyCode.S) ? -1f : 0f)),
            
            Look = lookInput,
            Zoom = -Input.mouseScrollDelta.y,
            Sprint = Input.GetKey(KeyCode.LeftShift),
            SwitchMode = Input.GetKeyDown(KeyCode.Z),
        };
        cameraInputs.Move = math.normalizesafe(cameraInputs.Move) *
                            math.saturate(math.length(cameraInputs.Move)); // Clamp move inputs magnitude to 1

        // Camera target switching
        Entity nextTargetPlanet = Entity.Null;
        Entity nextTargetShip = Entity.Null;
        Entity otherCameraEntity = Entity.Null;

        GameCameraJob job = new GameCameraJob
        {
            DeltaTime = Time.unscaledDeltaTime,
            // DeltaTime = SystemAPI.GetSingleton<SimulationRate>().UnscaledDeltaTime,
            CameraInputs = cameraInputs,
            NextTargetPlanet = nextTargetPlanet,
            NextTargetShip = nextTargetShip,
            LocalToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>(false),
            OtherCamera = otherCameraEntity,
        };
        state.Dependency = job.Schedule(state.Dependency);
    }

    [BurstCompile]
    [WithAll(typeof(Simulate))]
    public partial struct GameCameraJob : IJobEntity
    {
        public float DeltaTime;
        public CameraInputs CameraInputs;
        public Entity NextTargetPlanet;
        public Entity NextTargetShip;
        public Entity OtherCamera;

        public ComponentLookup<LocalToWorld> LocalToWorldLookup;

        void Execute(Entity entity, ref LocalTransform transform, ref GameCamera gameCamera)
        {
            if (gameCamera.IgnoreInput)
                return;

            // Mode switch
            if (CameraInputs.SwitchMode)
            {
                switch (gameCamera.CameraMode)
                {
                    case GameCamera.Mode.Fly:
                        gameCamera.CameraMode = GameCamera.Mode.OrbitPlanet;
                        break;
                    case GameCamera.Mode.OrbitPlanet:
                        gameCamera.CameraMode = GameCamera.Mode.OrbitShip;
                        break;
                    case GameCamera.Mode.OrbitShip:
                        gameCamera.CameraMode = GameCamera.Mode.Fly;
                        break;
                }
            }

            switch (gameCamera.CameraMode)
            {
                case GameCamera.Mode.Fly:
                {
                    // Yaw
                    float yawAngleChange = CameraInputs.Look.x * gameCamera.FlyRotationSpeed;
                    quaternion yawRotation = quaternion.Euler(math.up() * math.radians(yawAngleChange));
                    gameCamera.PlanarForward = math.mul(yawRotation, gameCamera.PlanarForward);

                    // Pitch
                    gameCamera.PitchAngle += -CameraInputs.Look.y * gameCamera.FlyRotationSpeed;
                    gameCamera.PitchAngle = math.clamp(gameCamera.PitchAngle, gameCamera.MinVAngle,
                        gameCamera.MaxVAngle);
                    quaternion pitchRotation = quaternion.Euler(math.right() * math.radians(gameCamera.PitchAngle));

                    // Final rotation
                    quaternion targetRotation =
                        math.mul(quaternion.LookRotationSafe(gameCamera.PlanarForward, math.up()), pitchRotation);
                    transform.Rotation = math.slerp(transform.Rotation, targetRotation,
                        MathUtilities.GetSharpnessInterpolant(gameCamera.FlyRotationSharpness, DeltaTime));

                    // Move
                    float3 worldMoveInputs = math.rotate(transform.Rotation, CameraInputs.Move);
                    worldMoveInputs = new float3(worldMoveInputs.x, worldMoveInputs.y*(CameraInputs.Zoom!=0 ? 5 : 1), worldMoveInputs.z);
                    float finalMaxSpeed = gameCamera.FlyMaxMoveSpeed;
                    if (CameraInputs.Sprint)
                    {
                        finalMaxSpeed *= gameCamera.FlySprintSpeedBoost;
                    }

                    //transform.Position.y /10 will give us a value between 1 and 10, we will use this to scale the speed
                    float heightMultiplier = transform.Position.y / 25;
                    finalMaxSpeed *= (heightMultiplier + 1);

                    gameCamera.CurrentMoveVelocity = math.lerp(gameCamera.CurrentMoveVelocity,
                        worldMoveInputs * finalMaxSpeed,
                        MathUtilities.GetSharpnessInterpolant(gameCamera.FlyMoveSharpness, DeltaTime));
                    transform.Position += gameCamera.CurrentMoveVelocity * DeltaTime;

                    transform.Position.y = math.clamp(transform.Position.y, gameCamera.MinHeight, gameCamera.MaxHeight);

                    break;
                }
                case GameCamera.Mode.None:
                    break;
            }

            // Manually calculate the LocalToWorld since this is updating after the Transform systems, and the LtW is what rendering uses
            LocalToWorld cameraLocalToWorld = new LocalToWorld();
            cameraLocalToWorld.Value = new float4x4(transform.Rotation, transform.Position);
            LocalToWorldLookup[entity] = cameraLocalToWorld;

            // LocalToWorld cameraLocalToWorld = new LocalToWorld();
            // cameraLocalToWorld.Value = new float4x4(transform.Rotation, transform.Position);
            // LocalToWorldLookup[OtherCamera] = cameraLocalToWorld;
        }
    }
}

public partial struct IgnoreCameraSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        // state.RequireForUpdate<IgnoreCameraInputRequest>();
        state.RequireForUpdate<GameCamera>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // EntityCommandBuffer ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
        //     .CreateCommandBuffer(state.WorldUnmanaged);
        // RefRW<GameCamera> gameCamera = SystemAPI.GetSingletonRW<GameCamera>();

        // foreach (var (ignoreCameraInputRequest, entity) in SystemAPI.Query<IgnoreCameraInputRequest>()
        //              .WithEntityAccess())
        // {
        //     gameCamera.ValueRW.IgnoreInput = ignoreCameraInputRequest.Value;
        //     ecb.DestroyEntity(entity);
        // }
    }
}
