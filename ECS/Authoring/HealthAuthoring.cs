using Unity.Entities;
using UnityEngine;

public class HealthAuthoring : MonoBehaviour {
    public int MaxHitPoints;
    public bool MultiplyPhysicalDamage;
    public float PhysicalDamageMultiplier;
    public bool MultiplyMagicalDamage;
    public float MagicalDamageMultiplier;
    public bool ShouldIgnoreDamageMultiplication;
    public class Baker : Baker<HealthAuthoring> {
        public override void Bake(HealthAuthoring authoring) {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new MaxHitPoints { Value = authoring.MaxHitPoints });
            // AddComponent(entity, new CurrentHitPoints { Value = authoring.MaxHitPoints });
            AddComponent(entity, new Health {
                Value = authoring.MaxHitPoints,
                onHealthChanged = true,
            });
            AddBuffer<DamageBufferElement>(entity);

            if (authoring.MultiplyPhysicalDamage)
            {
                AddComponent(entity, new PhysicalDamageMultiplier { 
                    Value = authoring.PhysicalDamageMultiplier 
                });
            }

            if (authoring.MultiplyMagicalDamage)
            {
                AddComponent(entity, new MagicalDamageMultiplier { 
                    Value = authoring.MagicalDamageMultiplier 
                });
            }
            if (authoring.ShouldIgnoreDamageMultiplication)
            {
                AddComponent<IgnoreDamageMultiplicationTag>(entity);
            }
        }
    }
}