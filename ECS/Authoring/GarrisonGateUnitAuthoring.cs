using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

namespace TJ
{
    /// <summary>
    /// Attach to a prefab to bake a minimal gate-unit ECS entity.
    /// The runtime values (squadId, squadEntity, Health) are patched in
    /// SpawnGateWithEntity after Instantiate.
    /// </summary>
    public class GarrisonGateUnitAuthoring : MonoBehaviour
    {
        [Tooltip("Width of the DOTS physics box collider (world units).")]
        public float width = 8f;
        [Tooltip("Height of the DOTS physics box collider (world units).")]
        public float height = 5f;
        [Tooltip("Depth of the DOTS physics box collider (world units).")]
        public float depth = 2f;

        class Baker : Baker<GarrisonGateUnitAuthoring>
        {
            public override void Bake(GarrisonGateUnitAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                // Unit component — Team/squadId/squadEntity patched at runtime
                AddComponent(entity, new Unit
                {
                    Team      = Team.Enemy,
                    unitType  = UnitType.Structure,
                    unitName  = UnitName.Gate,
                    unitState = UnitState.Idle,
                    squadId   = 0,
                });

                // Health placeholder — Value overwritten at runtime
                AddComponent(entity, new Health { Value = 1, onHealthChanged = false });
                AddComponent(entity, new MaxHitPoints { Value = 1 });
                AddComponent(entity, new EntityTeam { Value = Team.Enemy });
                AddBuffer<DamageBufferElement>(entity);

                // Marker — squadEntity patched at runtime
                AddComponent<GarrisonGateUnit>(entity);

                // MoveOverride (disabled) — FindTargetSystem calls IsComponentEnabled on
                // every potential target; the component must exist or it throws.
                AddComponent(entity, new MoveOverride());
                SetComponentEnabled<MoveOverride>(entity, false);

                // DOTS physics box collider on UNITS_LAYER so FindTargetSystem's
                // OverlapSphere detects the gate entity.
                var geometry = new BoxGeometry
                {
                    Center      = float3.zero,
                    Orientation = quaternion.identity,
                    Size        = new float3(authoring.width, authoring.height, authoring.depth),
                    BevelRadius = 0.05f
                };
                var filter = new CollisionFilter
                {
                    BelongsTo    = 1u << TabletopTavernConstants.UNITS_LAYER,
                    CollidesWith = ~0u,
                    GroupIndex   = 0
                };
                var colliderBlob = Unity.Physics.BoxCollider.Create(geometry, filter);
                AddBlobAsset(ref colliderBlob, out _);
                AddComponent(entity, new PhysicsCollider { Value = colliderBlob });
                AddComponent(entity, PhysicsMass.CreateKinematic(new MassProperties()));
                AddComponent(entity, new PhysicsVelocity());
                AddSharedComponent(entity, new PhysicsWorldIndex(0));
            }
        }
    }
}
