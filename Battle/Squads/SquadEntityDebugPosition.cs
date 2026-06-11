using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using TMPro;
using Unity.Mathematics;

namespace TJ
{
public class SquadEntityDebugPosition : MonoBehaviour
{
    public Entity Entity;
    public EntityManager EntityManager;
    [SerializeField] private TMP_Text mainText;

    private float3 RoundToTwoDecimals(float3 value){
        return new float3(math.round(value.x * 100) / 100, math.round(value.y * 100) / 100, math.round(value.z * 100) / 100);
    }
    void Update()
    {
        if (EntityManager.Exists(Entity)){
            if(EntityManager.HasComponent<SquadEntity>(Entity)){
                SquadEntity squadEntity = EntityManager.GetComponentData<SquadEntity>(Entity);
                mainText.text = $"<color={(squadEntity.SquadId > 0 ? ColorData.Player : ColorData.Enemy) }>{squadEntity.UnitName}</color> {Entity.Index}";
                // mainText.text += $"{(squadEntity.GuardMode ? "\nGuardMode" : "")}";
                if(squadEntity.TargetSquadEntity.Index != 0){
                    mainText.text += $"\nTarget: {squadEntity.TargetSquadEntity.Index}";
                }
                mainText.text += $"\n{squadEntity.SquadCommand}";
                // mainText.text += $"\nCenter {RoundToTwoDecimals(squadEntity.CenterPosition)}";
                // mainText.text += $"\nCached {RoundToTwoDecimals(squadEntity.CachedSquadCenter)}";

                // transform.position = squadEntity.CenterPosition;
                //rotate to face camera
                transform.LookAt(Camera.main.transform);
            } else {
                gameObject.SetActive(false);
            }
        } else {
            gameObject.SetActive(false);
        }
    }
}
}