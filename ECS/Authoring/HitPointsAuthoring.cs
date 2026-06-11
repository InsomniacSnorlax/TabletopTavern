// using Unity.Entities;
// using UnityEngine;

// //https://www.youtube.com/watch?v=SWXYpWtJZ5k
// class HitPointsAuthoring : MonoBehaviour
// {
//     public int MaxHitPoints;
//     public bool MultiplyPhysicalDamage;
//     public float PhysicalDamageMultiplier;
//     public bool MultiplyMagicalDamage;
//     public float MagicalDamageMultiplier;
//     public bool ShouldIgnoreDamageMultiplication;
//     public class HitPointsAuthoringBaker : Baker<HitPointsAuthoring>
//     {
//         public override void Bake(HitPointsAuthoring authoring)
//         {
//             var entity = GetEntity(TransformUsageFlags.Dynamic);
//             AddComponent(entity, new MaxHitPoints { Value = authoring.MaxHitPoints });
//             // AddComponent(entity, new CurrentHitPoints { Value = authoring.MaxHitPoints });
//             AddComponent(entity, new Health {
//                 healthAmount = authoring.MaxHitPoints,
//                 onHealthChanged = true,
//             });
//             AddBuffer<DamageBufferElement>(entity);

//             if (authoring.MultiplyPhysicalDamage)
//             {
//                 AddComponent(entity, new PhysicalDamageMultiplier { 
//                     Value = authoring.PhysicalDamageMultiplier 
//                 });
//             }

//             if (authoring.MultiplyMagicalDamage)
//             {
//                 AddComponent(entity, new MagicalDamageMultiplier { 
//                     Value = authoring.MagicalDamageMultiplier 
//                 });
//             }
//         }
//     }
// }