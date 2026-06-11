using UnityEngine;
using Shapes;
using System.Collections;
using Unity.Mathematics;
using System.Collections.Generic;
using Unity.Entities;
using TJ.Shapes;
using Memori.Input;

namespace TJ
{
    public class AttackArrowDrawer : MonoBehaviour
    {
        [System.Serializable] enum ArrowState { Off, Hovered, Selected }
        [System.Serializable] enum ArrowToggleState { ToggledOff, ToggledOn }
        [System.Serializable] enum SquadDestinationType { Movement, Attack }

        [Header("State")]
        [SerializeField] private ArrowState _activeArrowState;
        [SerializeField] private ArrowToggleState _arrowToggleState;
        [SerializeField] private bool isRanged;
        [SerializeField] private SquadDestinationType _squadDestinationType;

        [Header("References")]
        [SerializeField] private GameObject _arrowParent;
        [SerializeField] private Polyline movementLine;
        [SerializeField] private Polyline archerAttackArc;
        [SerializeField] private ShapeRenderer pointTriangle;

        [Header("Settings")]
        [SerializeField] private Color attackColor, movementColor;
        [SerializeField] private AnimationCurve polylineCurve;

        //Local
        private EntityManager EntityManager;
        private SquadEntity squadEntity;
        private ShapesBloom movementLineBloom, triangleBloom, archerRangeBloom;
        private int archerAttackArcPoints = 20;
        private float polylineHeightMultiplier = 7;
        private Vector3 startPoint;
        private List<Vector3> _destinationPoints = new List<Vector3>();
        private bool isInRangedFire = false;
        private bool _hasValidPath = false;


        private void Start()
        {
            InputHandler.Instance.OnShowUnitMovement += ToggleArrowSateToToggledOn;
            // InputHandler.Instance.OnCancelShowUnitMovement += ToggleArrowSateToToggledOff;
        }
        private void ToggleArrowSateToToggledOn()
        {
            if(_arrowToggleState == ArrowToggleState.ToggledOff)
                SetArrowToggleState(ArrowToggleState.ToggledOn);
            else
                SetArrowToggleState(ArrowToggleState.ToggledOff);
        }
        // private void ToggleArrowSateToToggledOff()
        // {
        //     SetArrowToggleState(ArrowToggleState.ToggledOff);
        // }

        private void SetArrowState(ArrowState arrowState)
        {
            _activeArrowState = arrowState;
            switch (arrowState)
            {
                case ArrowState.Off:
                    if(_arrowToggleState == ArrowToggleState.ToggledOff)
                    {
                        TurnOffArrow();
                    }
                    break;
                case ArrowState.Hovered:
                    if(_arrowToggleState == ArrowToggleState.ToggledOff)
                    {
                        TurnOnArrow();
                    }
                    break;
                case ArrowState.Selected:
                    if(_arrowToggleState == ArrowToggleState.ToggledOff)
                    {
                        TurnOnArrow();
                    }
                    break;
            }
        }
        private void SetArrowToggleState(ArrowToggleState arrowToggleState)
        {
            _arrowToggleState = arrowToggleState;
            switch (arrowToggleState)
            {
                case ArrowToggleState.ToggledOn:
                    if(_activeArrowState == ArrowState.Off)
                    {
                        TurnOnArrow();
                    }
                    break;
                case ArrowToggleState.ToggledOff:
                    if(_activeArrowState == ArrowState.Off)
                    {
                        TurnOffArrow();
                    }
                    break;
            }
        }
        public void SetUp(SquadEntity _squadEntity)
        {
            EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            squadEntity = _squadEntity;

            if (squadEntity.SquadId < 0)
            {
                Destroy(gameObject);
                return;
            }

            isRanged = TabletopTavernData.Instance.GetUnitTypeFromUnitName(squadEntity.UnitName) != UnitType.Melee;
            movementLineBloom = movementLine.GetComponent<ShapesBloom>();
            triangleBloom = pointTriangle.GetComponent<ShapesBloom>();
            archerRangeBloom = archerAttackArc.GetComponent<ShapesBloom>();
            gameObject.name = $"AttackArrow_{squadEntity.SquadId}_{squadEntity.UnitName}";

            SetArrowState(ArrowState.Off);
            SetArrowToggleState(ArrowToggleState.ToggledOff);
            TurnOffRangedFire();
        }

        private void Update()
        {
            bool PassesSanityChecks()
            {
                if (BattleManager.Instance.GamePhase == GamePhase.PostGame) return false;

                if (!EntityManager.Exists(squadEntity.SelfEntity))
                {
                    Destroy(gameObject);
                    return false;
                }

                if (!EntityManager.HasComponent<SquadMovementComponent>(squadEntity.SelfEntity))
                {
                    Debug.Log($"wtf {squadEntity.SelfEntity}");
                    return false;
                }

                if (EntityManager.HasComponent<BrokenSquadTag>(squadEntity.SelfEntity))
                {
                    Destroy(gameObject);
                    return false;
                }

                return true;
            }
            
            if (!PassesSanityChecks()) return;

            DynamicBuffer<QueuedOrder> queuedOrders = EntityManager.GetBuffer<QueuedOrder>(squadEntity.SelfEntity);
            if (queuedOrders.Length == 0)
            {
                // Debug.Log($"[AttackArrow] Squad {squadEntity.SquadId}: SquadCommand is None → turning off");
                if(_activeArrowState != ArrowState.Off)
                {
                    _activeArrowState = ArrowState.Off;
                    if (_arrowToggleState == ArrowToggleState.ToggledOff)
                        TurnOffArrow();
                }
                _hasValidPath = false;
                pointTriangle.gameObject.SetActive(false);
                return;
            }
            if (_arrowToggleState == ArrowToggleState.ToggledOn)
                TurnOnArrow();

            SquadMovementComponent squadMovementComponent = EntityManager.GetComponentData<SquadMovementComponent>(squadEntity.SelfEntity);
            startPoint = squadMovementComponent.SquadCenter;
            _destinationPoints.Clear();
            if(queuedOrders.Length > 0)
            {
                _squadDestinationType = SquadDestinationType.Movement;
                foreach(QueuedOrder currentOrder in queuedOrders)
                {
                    if(currentOrder.Type == QueuedOrderType.Move)
                    {
                        _destinationPoints.Add(currentOrder.Goal);
                    }
                    else if (currentOrder.Type == QueuedOrderType.Attack)
                    {
                        //get squad to attack position from squad manager
                        int targetSquadId = currentOrder.TargetSquadId;
                        SquadEntity targetSquadEntity = BattleManager.Instance.SquadManager.GetSquadEntityFromId(targetSquadId, true);
                        if(targetSquadEntity.SelfEntity == Entity.Null)
                        {
                            // Debug.Log($"[AttackArrow] Squad {squadEntity.SquadId}: Attack order target ID {targetSquadId} not found → turning off");
                            if(_activeArrowState != ArrowState.Off)
                                SetArrowState(ArrowState.Off);

                            return;
                        }

                        if(EntityManager.Exists(targetSquadEntity.SelfEntity) && EntityManager.HasComponent<SquadMovementComponent>(targetSquadEntity.SelfEntity))
                        {
                            SquadMovementComponent targetSquadMovementComponent = EntityManager.GetComponentData<SquadMovementComponent>(targetSquadEntity.SelfEntity);
                            _destinationPoints.Add(targetSquadMovementComponent.SquadCenter);
                        }
                        else
                        {
                            // Debug.Log($"[AttackArrow] Squad {squadEntity.SquadId}: Attack target {targetSquadId} exists={EntityManager.Exists(targetSquadEntity.SelfEntity)} but missing SquadMovementComponent → no destination added");
                        }
                        _squadDestinationType = SquadDestinationType.Attack;
                    }
                }
            }
            else
            {
                // Debug.Log($"[AttackArrow] Squad {squadEntity.SquadId}: No queued orders (SquadCommand={squadCommand}) → turning off");
                if(_activeArrowState != ArrowState.Off)
                    SetArrowState(ArrowState.Off);
                return;
            }

            SetArrowColor();
            RecalculateArrowPath();

            bool isSelected = BattleManager.Instance.UnitSelectionManager.SelectedSquadIds.Contains(squadEntity.SquadId);
            bool isHovered = BattleManager.Instance.UIManager.HoveredSquadId == squadEntity.SquadId;
            // Debug.Log($"[AttackArrow] Squad {squadEntity.SquadId}: isSelected={isSelected} isHovered={isHovered}");

            ArrowState desiredState = ArrowState.Off;
            if (isSelected) desiredState = ArrowState.Selected;
            else if (isHovered) desiredState = ArrowState.Hovered;

            // Debug.Log($"[AttackArrow] Squad {squadEntity.SquadId}: cmd={squadCommand} selected={isSelected} hovered={isHovered} activeState={_activeArrowState} desiredState={desiredState} toggle={_arrowToggleState}");

            if (_activeArrowState != desiredState)
            {
                SetArrowState(desiredState);
                // Debug.Log($"[AttackArrow] Squad {squadEntity.SquadId}: Arrow state changed to {desiredState}");
            }

            if(isRanged)
            {
                if(!isInRangedFire && EntityManager.HasComponent<FormationEngagedInRangedCombat>(squadEntity.SelfEntity))
                {
                    TurnOnRangedFire();
                }
                else if (!EntityManager.HasComponent<FormationEngagedInRangedCombat>(squadEntity.SelfEntity) && isInRangedFire)
                {
                    TurnOffRangedFire();
                }
            }
        }
        public void SwitchToMelee(bool _toMelee)
        {
            isRanged = !_toMelee;
            if (_toMelee && isInRangedFire)
            {
                TurnOffRangedFire();
                TurnOffArrow();
                return;
            }

            TurnOffArrow();
            TurnOnArrow();
        }
        private void TurnOnArrow()
        {
            if(_arrowParent == null) return;

            _arrowParent.SetActive(true);
            // Debug.Log($"[AttackArrow] Squad {squadEntity.SquadId}: Arrow turned ON");
        }
        private void TurnOffArrow()
        {
            if(_arrowParent == null) return;
            
            _arrowParent.SetActive(false);
            // Debug.Log($"[AttackArrow] Squad {squadEntity.SquadId}: Arrow turned OFF");
        }
        private void TurnOnRangedFire()
        {
            isInRangedFire = true;
            movementLine.gameObject.SetActive(false);
            archerAttackArc.gameObject.SetActive(true);
            pointTriangle.gameObject.SetActive(_hasValidPath);
        }
        private void TurnOffRangedFire()
        {
            isInRangedFire = false;
            movementLine.gameObject.SetActive(true);
            archerAttackArc.gameObject.SetActive(false);
            pointTriangle.gameObject.SetActive(_hasValidPath);
        }
        private void RecalculateArrowPath()
        {
            if (_destinationPoints.Count == 0) return;

            //this needs to be changed to support multiple destination points
            Vector3[] points = new Vector3[_destinationPoints.Count + 1];
            points[0] = startPoint;
            for (int i = 0; i < _destinationPoints.Count; i++)
            {
                points[i + 1] = _destinationPoints[i];
            }
            movementLine.SetPoints(points);
            movementLine.UpdateMesh(true);
            
            pointTriangle.transform.position = points[^1] - (points[^1] - points[^2]).normalized;
            pointTriangle.transform.LookAt(points[^1] + (points[^1] - points[^2]));
            // pointTriangle.transform.Rotate(0, 180, 0);
            _hasValidPath = true;
            pointTriangle.gameObject.SetActive(true);

            if (isInRangedFire)
            {
                DrawArcherArc(points[0] + Vector3.up * 4, (points[^1] - (points[^1] - points[^2]).normalized * 3) + Vector3.up * 4);
            } 
        }
        private void DrawArcherArc(Vector3 start, Vector3 end)
        {
            //polyline will have archerAttackArcPoints points from start to end
            Vector3[] points = new Vector3[archerAttackArcPoints];
            for (int i = 0; i < archerAttackArcPoints; i++)
            {
                float t = i / (archerAttackArcPoints - 1f);
                points[i] = Vector3.Lerp(start, end, t);
                points[i].y += polylineCurve.Evaluate(t) * polylineHeightMultiplier;
            }
            archerAttackArc.SetPoints(points);
            archerAttackArc.UpdateMesh(true);

            //move to the 19th point and look at the 20th point
            pointTriangle.transform.position = points[19];
            pointTriangle.transform.LookAt(points[19] + (points[19] - points[18]));
            // pointTriangle.transform.Rotate(0, 180, 0);
        }
        private void SetArrowColor()
        {
            Color color = (_squadDestinationType == SquadDestinationType.Attack) ? attackColor : movementColor;
            movementLineBloom.SetColor(color);
            triangleBloom.SetColor(color);
            archerRangeBloom.SetColor(color);

            movementLineBloom.Bloom();
            triangleBloom.Bloom();
            archerRangeBloom.Bloom();
        }
        private void OnDestroy()
        {
            if (InputHandler.HasInstance)
            {
                InputHandler.Instance.OnShowUnitMovement -= ToggleArrowSateToToggledOn;
                // InputHandler.Instance.OnCancelShowUnitMovement -= ToggleArrowSateToToggledOff;
            }
        }
    }
}