using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
public class GameCameraAuthoring : MonoBehaviour
{
    [Header("General")]
    public float MaxVAngle = 89f;
    public float MinVAngle = -89f;

    [Header("Height")]
    public float minHeight = 2f;
    public float maxHeight = 100f;


    [Header("Free Fly")]
    public float FlyRotationSpeed = 999999f;
    public float FlyRotationSharpness = 999999f;
    public float FlyMoveSharpness = 10f;
    public float FlyMaxMoveSpeed = 10f;
    public float FlySprintSpeedBoost = 5f;

    public class Baker : Baker<GameCameraAuthoring>
    {
        public override void Bake(GameCameraAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

            AddComponent(entity, new GameCamera
            {
                MinHeight = authoring.minHeight,
                MaxHeight = authoring.maxHeight,

                MaxVAngle = authoring.MaxVAngle,
                MinVAngle = authoring.MinVAngle,

                FlyRotationSpeed = authoring.FlyRotationSpeed,
                FlyRotationSharpness = authoring.FlyRotationSharpness,
                FlyMoveSharpness = authoring.FlyMoveSharpness,
                FlyMaxMoveSpeed = authoring.FlyMaxMoveSpeed,
                FlySprintSpeedBoost = authoring.FlySprintSpeedBoost,
                
                CameraMode = GameCamera.Mode.Fly,
                PlanarForward = math.forward(),

                // PitchAngle = 46f,//for starting it looking down
                PitchAngle = 30f
            });
            AddComponent(entity, new MainCamera());
        }
    }
}