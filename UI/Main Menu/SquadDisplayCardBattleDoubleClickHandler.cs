
using Memori.Core;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace TJ.MainMenu
{
    public class SquadDisplayCardBattleDoubleClickHandler : DoubleClickHandler
    {
        private Entity _squadEntity;

        public void SetUp(Entity squadEntity)
        {
            _squadEntity = squadEntity;
        }

        protected override void OnDoubleClick()
        {
            if (_squadEntity == Entity.Null) return;

            EntityManager em = World.DefaultGameObjectInjectionWorld.EntityManager;
            if (!em.HasComponent<SquadMovementComponent>(_squadEntity)) return;

            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                if (!em.HasComponent<SquadEntity>(_squadEntity)) return;
                UnitName unitName = em.GetComponentData<SquadEntity>(_squadEntity).UnitName;
                BattleManager.Instance.UnitSelectionManager.SelectAllPlayerSquadsWithName(unitName);
                return;
            }

            float3 center = em.GetComponentData<SquadMovementComponent>(_squadEntity).SquadCenter;
            BattleManager.Instance.BattleCameraScript.FocusOnPosition(new Vector3(center.x, center.y, center.z));
        }
    }
}
