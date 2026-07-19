using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using Unity.Collections;

public struct SquadEntity : IComponentData
{
    [Header("Base Stats")]
    public Entity SelfEntity;
    public int SquadId;
    public UnitName UnitName;
    public int initialSquadSize;
    public Team Team;

    [Header("Status")]
    public bool IsSelected;
    public SquadCommand SquadCommand;
    public Entity TargetSquadEntity;
}
public struct SquadTargettingComponent : IComponentData, IEnableableComponent
{
    public float UpdateTargetDestinationRefreshRate;
}
public struct SquadMovementComponent : IComponentData
{
    public Entity SelfEntity;
    private int2 squadWidthAndDepth;
    public readonly int2 SquadWidthAndDepth => squadWidthAndDepth;
    public float3 SquadCenter;
    private quaternion squadRotation;
    public readonly quaternion SquadRotation => squadRotation;
    public float3 GoalPosition;
    public float3 BoundsMin;            // NEW – AABB min
    public float3 BoundsMax;            // NEW – AABB max
    public float  BoundsRadius;          // NEW – bounding sphere radius
    public void SetWidthAndDepth(int2 widthAndDepth)
    {
        squadWidthAndDepth = widthAndDepth;
    }
    public void SetRotation(quaternion rotation)
    {
        squadRotation = rotation;
    }
}

[InternalBufferCapacity(64)] // Optional: set the internal buffer capacity
public struct EntityReferenceBufferElement : IBufferElementData
{
    public Entity Entity;
    public float3 PositionOffset;
    public Entity DebugEntity;
}
public struct SquadCameraDistanceComponent : IComponentData
{
    public float DistanceToCamera;
}
public struct SquadOverridesComponent : IComponentData
{
    public int SquadId;
    public bool GuardMode;
    public UnitType UnitType;
    public bool AutoTarget;
    public bool MeleeMode;
    public RangedFireMode FireMode;
    public ShieldedStance ShieldedStance;
    public bool CeaseFire;
}