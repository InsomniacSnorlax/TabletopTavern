using Unity.Entities;
using UnityEngine;

public class UnitFindTargetAuthoring : MonoBehaviour {
    public float sightRange;
    public Team TargetTeam;
    public float timerMax;
    // public float timerNoneFound;

    public class Baker : Baker<UnitFindTargetAuthoring> {
        public override void Bake(UnitFindTargetAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new UnitFindTarget {
                sightRange = authoring.sightRange,
                TargetTeam = authoring.TargetTeam,
                timerMax = authoring.timerMax,
                // timerNoneFound = authoring.timerNoneFound
            });
            SetComponentEnabled<UnitFindTarget>(entity, false);
        }
    }
}

public struct UnitFindTarget : IComponentData, IEnableableComponent {
    public Entity TargetedSquadEntity;
    public float sightRange;
    public Team TargetTeam;
    public float timer;
    public float timerMax;
}