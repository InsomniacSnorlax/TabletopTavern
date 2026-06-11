using UnityEngine;
using ProjectDawn.Navigation.Hybrid;
using Unity.Entities;
using Unity.Mathematics;
public class SetDestinationAuthoring : MonoBehaviour
{
    public Vector3 Target;
    void Start()
    {
        Target = transform.position;
        GetComponent<AgentAuthoring>().SetDestination(Target);
    }
}

// Bakes mono component into ecs component
class AgentSetDestinationBaker : Baker<SetDestinationAuthoring>
{
    public override void Bake(SetDestinationAuthoring authoring)
    {
        AddComponent(GetEntity(TransformUsageFlags.Dynamic),
            new SetDestination { 
                destinationPosition = authoring.transform.position,
                // guardPosition = authoring.transform.position,
                // destinationRotation = quaternion.identity,
            });
    }
}

