using Unity.Entities;
using UnityEngine;

public class ExecuteAuthoring : MonoBehaviour
{
    public bool BattlePhase;
    public bool FindTargets;
    // public bool AllowRandomWalking;
    public bool UseFixedSimulationDeltaTime = true;
    public float FixedDeltaTime = 0.0333333f;
    class Baker : Baker<ExecuteAuthoring>
    {
        public override void Bake(ExecuteAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            // if(authoring.BattlePhase) AddComponent<BattlePhase>(entity);
            // if(authoring.BattlePhase) AddComponent<BattleHasStarted>(entity);
            if (authoring.FindTargets) AddComponent<FindTargets>(entity);
            // if (authoring.AllowRandomWalking) AddComponent<AllowRandomWalking>(entity);
            
            AddComponent(entity, new SimulationRate
            {
                UseFixedRate = authoring.UseFixedSimulationDeltaTime,
                FixedTimeStep = authoring.FixedDeltaTime,
                TimeScale = 1f,
                Update = true,
            });
        }
    }
}