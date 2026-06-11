// using UnityEngine;
// using Unity.Entities;
// using Memori.Audio;
// using Unity.Transforms;

// public struct PlayAudioClipOnSpawnData : IComponentData, IEnableableComponent
// {
//     public UnityObjectRef<AudioClip> AudioClip;
// }
// public class PlayAudioClipOnSpawnAuthoring : MonoBehaviour
// {
//     public AudioClip AudioClip;
//     public class Baker : Baker<PlayAudioClipOnSpawnAuthoring>
//     {
//         public override void Bake(PlayAudioClipOnSpawnAuthoring authoring)
//         {
//             var entity = GetEntity(TransformUsageFlags.None);
//             AddComponent(entity, new PlayAudioClipOnSpawnData
//             {
//                 AudioClip = authoring.AudioClip
//             });
//         }
//     }
// }
// // public partial struct PlayAudioClipOnSpawnSystem : ISystem
// // {
// //     public void OnUpdate(ref SystemState state)
// //     {
// //         foreach (var (audioClipData, playAudioClip, localTransform) in SystemAPI.Query<PlayAudioClipOnSpawnData, EnabledRefRW<PlayAudioClipOnSpawnData>, LocalTransform>())
// //         {
// //             BattleAudioController.Instance.PlayAudioClip(audioClipData.AudioClip);
// //             playAudioClip.ValueRW = false;
// //         }
// //     }
// // }
