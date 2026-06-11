using UnityEngine;
using Shapes;
using System.Collections;
using Unity.Mathematics;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using TJ.Shapes;
using Memori.Input;

public class ArcherRangeDrawer : MonoBehaviour
{
    [SerializeField] private Line leftLine, rightLine;
    [SerializeField] private Disc arc;
    [SerializeField] private Color playerColor, enemyColor;
    float range;
    Coroutine fadeArrowRoutine;
    ShapesBloom leftLineBloom, rightLineBloom, polylineBloom, arcBloom;
    Entity cachedEntity;
    bool isSetUp = false, cachedOn;
    bool isOn = false;
    bool _toggledOn = false;
    int squadId;
    bool inMeleeMode = false;
    private void Awake()
    {
        leftLineBloom = leftLine.GetComponent<ShapesBloom>();
        rightLineBloom = rightLine.GetComponent<ShapesBloom>();
        arcBloom = arc.GetComponent<ShapesBloom>();
    }
    private void Start()
    {
        InputHandler.Instance.OnShowUnitMovement += ToggleRange;
    }
    private void ToggleRange()
    {
        _toggledOn = !_toggledOn;
        if (_toggledOn) TurnOn();
        else TurnOff();
    }
    private void OnDestroy()
    {
        if (InputHandler.HasInstance)
            InputHandler.Instance.OnShowUnitMovement -= ToggleRange;
    }
    public void Update()
    {
        if(!isSetUp) return;

        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        if(!entityManager.Exists(cachedEntity)) {
            BattleManager.Instance.SquadManager.RemoveArcherRangeDrawer(squadId);
            return;
        }
        SquadMovementComponent _squadEntity = entityManager.GetComponentData<SquadMovementComponent>(cachedEntity);
        transform.SetPositionAndRotation(_squadEntity.SquadCenter, _squadEntity.SquadRotation);
    }
    public void SetUp(SquadEntity _squadEntity)
    {
        cachedEntity = _squadEntity.SelfEntity;
        squadId = _squadEntity.SquadId;
        leftLineBloom.SetColor(squadId > 0 ? playerColor : enemyColor);
        rightLineBloom.SetColor(squadId > 0 ? playerColor : enemyColor);
        arcBloom.SetColor(squadId > 0 ? playerColor : enemyColor);
        Recalculate();
        isSetUp = true;
    }
    public void Recalculate()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        if(!entityManager.Exists(cachedEntity)) {
            Debug.LogError($"Entity {cachedEntity} does not exist anymore. Should remove ArcherRangeDrawer.");
            return;
        }
        if (!entityManager.HasComponent<SquadMovementComponent>(cachedEntity))
        {
            Debug.LogError($"Entity {cachedEntity} does not have SquadMovementComponent.");
            return;
        }
        SquadMovementComponent squadMovementComponent = entityManager.GetComponentData<SquadMovementComponent>(cachedEntity);
        int2 widthAndDepth = squadMovementComponent.SquadWidthAndDepth;
        float width = widthAndDepth.x * 0.75f;
        float height = widthAndDepth.y * 0.75f;

        if (entityManager.HasComponent<RangedSquad>(cachedEntity))
            range = entityManager.GetComponentData<RangedSquad>(cachedEntity).AttackRange;

        arc.Radius = range;
        // Debug.Log($"Recalculating archer range drawer for squad {_squadEntity.SquadId} with range {range}");
        Vector3 center = Vector3.zero; // Center of the arc

        static Vector3 CalculateArcPoint(Vector3 center, float range, float angleInDegrees)
        {
            float angleInRadians = Mathf.Deg2Rad * angleInDegrees;
            float x = center.x + range * Mathf.Cos(angleInRadians);
            float z = center.z + range * Mathf.Sin(angleInRadians);
            return new Vector3(x, center.y, z);
        }

        Vector3 startPoint = CalculateArcPoint(center, range, 45f);  // Start of the arc
        Vector3 endPoint = CalculateArcPoint(center, range, 135f);  // End of the arc

        leftLine.Start = center - (width * Vector3.right) + (height * Vector3.forward);
        leftLine.End = endPoint;

        rightLine.Start = center + (width * Vector3.right) + (height * Vector3.forward);
        rightLine.End = startPoint;

        leftLineBloom.Bloom();
        rightLineBloom.Bloom();
        arcBloom.Bloom();
    }
    public void TurnOn()
    {
        if (!Cursor.visible) return;
        if (inMeleeMode) return;
        
        leftLine.gameObject.SetActive(true);
        rightLine.gameObject.SetActive(true);
        arc.gameObject.SetActive(true);
        isOn = true;
    }
    public void TurnOff()
    {
        if (_toggledOn) return;
        leftLine.gameObject.SetActive(false);
        rightLine.gameObject.SetActive(false);
        arc.gameObject.SetActive(false);
        isOn = false;
    }
    public void ShowAllRanges(bool _start)
    {
        if (_start)
        {
            cachedOn = isOn;
            TurnOn();
        }
        else
        {
            if (!cachedOn && !_toggledOn) TurnOff();
        }
    }
    public void SwitchToMelee(bool _toMelee)
    {
        inMeleeMode = _toMelee;
        if (inMeleeMode)
        {
            TurnOff();
        }
        else
        {
            TurnOn();
        }
    }
}
