using Unity.Entities;

public struct Health : IComponentData
{
    public int Value;
    public bool onHealthChanged;
}
public struct ModifyHealthOnSpawn : IComponentData { public int Value; }
