using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
namespace TJ
{
    public class GarrisonGateSquadHelper : MonoBehaviour
    {

        [SerializeField] private GameObject _gateGameObject;
        private Entity _gateSquadEntity;
        private EntityManager _entityManager;
        public int GateIndex { get; private set; }

        public void Initialize(Entity gateSquadEntity, GameObject gateGameObject)
        {
            _gateSquadEntity = gateSquadEntity;
            _gateGameObject = gateGameObject;
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            GateIndex = _entityManager.GetComponentData<GarrisonGateSquadTag>(gateSquadEntity).GateIndex;

            GarrisonGateRangeDrawer drawer = _gateGameObject.GetComponent<GarrisonGateRangeDrawer>();
            if (drawer != null && drawer.FiringPointA != null && drawer.FiringPointB != null)
            {
                Vector3 fwd = _gateGameObject.transform.forward;
                _entityManager.AddComponentData(_gateSquadEntity, new GateFiringPoints
                {
                    PointA    = (float3)drawer.FiringPointA.position,
                    PointB    = (float3)drawer.FiringPointB.position,
                    UsePointA = true,
                    ForwardXZ = math.normalize(new float2(fwd.x, fwd.z)),
                });
            }

            InvokeRepeating(nameof(PollGate), 1f, 1f);
            BattleManager.Instance.OnGamePhaseChanged += OnGamePhaseChanged;
        }

        private void OnGamePhaseChanged(GamePhase gamePhase)
        {
            if (gamePhase == GamePhase.PostGame)
            {
                if (this != null)
                    CancelInvoke(nameof(PollGate));

                if(BattleManager.HasInstance)
                    BattleManager.Instance.OnGamePhaseChanged -= OnGamePhaseChanged;
            }
        }

        private void PollGate()
        {
            if (World.DefaultGameObjectInjectionWorld == null)
            {
                AlertOfDestruction();
                return;
            }

            if (!_entityManager.Exists(_gateSquadEntity))
            {
                AlertOfDestruction();
                return;
            }

            // TODO: add gate polling logic here
        }

        private void AlertOfDestruction()
        {
            BattleManager.Instance.NotifyGateDestroyed(GateIndex);
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            CancelInvoke(nameof(PollGate));

            if(BattleManager.HasInstance)
                BattleManager.Instance.OnGamePhaseChanged -= OnGamePhaseChanged;
        }
    }
}
