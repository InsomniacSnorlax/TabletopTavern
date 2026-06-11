using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;

public class SquadSpawnerAuthoring : MonoBehaviour {
    public class Baker : Baker<SquadSpawnerAuthoring> {
        public override void Bake(SquadSpawnerAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new SquadSpawner {
            });
        }
    }
}

public struct SquadSpawner : IComponentData 
{
    public int unitCount;
    public Team team;
    public UnitName unitName;
    public float noise;
}