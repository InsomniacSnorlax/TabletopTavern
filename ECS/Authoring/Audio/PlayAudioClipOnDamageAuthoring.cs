// using UnityEngine;
// using Unity.Entities;
// using Memori.Audio;


// public struct PlayAudioClipOnDamageData : IComponentData, IEnableableComponent
// {
//     public UnityObjectRef<AudioClip> AudioClip;
// }
// public class PlayAudioClipOnDamageAuthoring : MonoBehaviour
// {
//     public AudioClip AudioClip;
//     public class Baker : Baker<PlayAudioClipOnDamageAuthoring>
//     {
//         public override void Bake(PlayAudioClipOnDamageAuthoring authoring)
//         {
//             var entity = GetEntity(TransformUsageFlags.None);
//             AddComponent(entity, new PlayAudioClipOnDamageData
//             {
//                 AudioClip = authoring.AudioClip
//             });
//             SetComponentEnabled<PlayAudioClipOnDamageData>(entity, false);
//         }
//     }
// }
// public partial struct PlayAudioClipOnDamageSystem : ISystem
// {
//     public void OnUpdate(ref SystemState state)
//     {
//         foreach (var (audioClipData, playAudioClip) in SystemAPI.Query<PlayAudioClipOnDamageData, EnabledRefRW<PlayAudioClipOnDamageData>>())
//         {
//             BattleAudioController.Instance.PlayAudioClip(audioClipData.AudioClip);
//             playAudioClip.ValueRW = false;
//         }
//     }
// }
