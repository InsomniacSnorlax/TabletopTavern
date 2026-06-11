// using Unity.Entities;
// using UnityEngine;

// public class SFXIDHolderAuthoring : MonoBehaviour
// {
//     public Memori.Audio.SFXEntity attackSFXID;
//     public Memori.Audio.SFXEntity deathSFXID;
//     public Memori.Audio.SFXEntity idleSFXID;

//     public class Baker : Baker<SFXIDHolderAuthoring> {
//         public override void Bake(SFXIDHolderAuthoring authoring) {
//             Entity entity = GetEntity(TransformUsageFlags.Dynamic);
//             AddComponent(entity, new SFXIDHolder() {
//                 deathSFXID = authoring.deathSFXID,
//                 idleSFXID = authoring.idleSFXID
//             });
//         }
//     }
// }

// public struct SFXIDHolder : IComponentData, IEnableableComponent
// {
//     public Memori.Audio.SFXEntity deathSFXID;
//     public Memori.Audio.SFXEntity idleSFXID;
// }