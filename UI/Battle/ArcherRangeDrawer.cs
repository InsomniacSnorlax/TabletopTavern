using UnityEngine;
using Shapes;
using System.Collections;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Transforms;
using TJ.Shapes;
using Memori.Input;

public class ArcherRangeDrawer : MonoBehaviour
{
    [SerializeField] private Line leftLine, rightLine;
    [SerializeField] private Disc arc, arc2;
    [SerializeField] private Color playerColor, enemyColor;
    [SerializeField] private float _fadeDuration = 0.11f;

    float range;
    ShapesBloom leftLineBloom, rightLineBloom, arcBloom, arc2Bloom;
    Entity cachedEntity;
    bool isSetUp = false, cachedOn;
    bool isOn = false;
    bool _toggledOn = false;
    int squadId;
    bool inMeleeMode = false;

    Coroutine _fadeRoutine;
    Color _arcTargetColor, _arc2TargetOuter, _lineTargetColor;

    private void Awake()
    {
        leftLineBloom = leftLine.GetComponent<ShapesBloom>();
        rightLineBloom = rightLine.GetComponent<ShapesBloom>();
        arcBloom = arc.GetComponent<ShapesBloom>();
        arc2Bloom = arc2.GetComponent<ShapesBloom>();
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
        if (!isSetUp) return;

        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        if (!entityManager.Exists(cachedEntity))
        {
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
        Color teamColor = squadId > 0 ? playerColor : enemyColor;
        leftLineBloom.SetColor(teamColor);
        rightLineBloom.SetColor(teamColor);
        arcBloom.SetColor(teamColor);
        // arc2Bloom.SetColor(teamColor);

        Recalculate();

        leftLineBloom.Bloom();
        rightLineBloom.Bloom();
        arcBloom.Bloom();
        arc2Bloom.Bloom();

        // Cache target colors after bloom sets them, then start invisible
        _lineTargetColor = leftLine.Color;
        _arcTargetColor  = arc.Color;
        _arc2TargetOuter = arc2.ColorOuter;

        leftLine.Color  = new Color(_lineTargetColor.r, _lineTargetColor.g, _lineTargetColor.b, 0f);
        rightLine.Color = new Color(_lineTargetColor.r, _lineTargetColor.g, _lineTargetColor.b, 0f);
        arc.Color       = new Color(_arcTargetColor.r,  _arcTargetColor.g,  _arcTargetColor.b,  0f);
        arc2.ColorInner = new Color(_arc2TargetOuter.r, _arc2TargetOuter.g, _arc2TargetOuter.b, 0f);
        arc2.ColorOuter = new Color(_arc2TargetOuter.r, _arc2TargetOuter.g, _arc2TargetOuter.b, 0f);

        isSetUp = true;
    }

    public void Recalculate()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        if (!entityManager.Exists(cachedEntity))
        {
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
        float width  = widthAndDepth.x * 0.75f;
        float height = widthAndDepth.y * 0.75f;

        if (entityManager.HasComponent<RangedSquad>(cachedEntity))
            range = entityManager.GetComponentData<RangedSquad>(cachedEntity).AttackRange;

        arc.Radius     = range;
        arc2.Radius    = range - 3.75f;
        arc2.Thickness = 7.5f;

        Vector3 center = Vector3.zero;

        static Vector3 CalculateArcPoint(Vector3 center, float range, float angleInDegrees)
        {
            float angleInRadians = Mathf.Deg2Rad * angleInDegrees;
            float x = center.x + range * Mathf.Cos(angleInRadians);
            float z = center.z + range * Mathf.Sin(angleInRadians);
            return new Vector3(x, center.y, z);
        }

        Vector3 startPoint = CalculateArcPoint(center, range, 45f);
        Vector3 endPoint   = CalculateArcPoint(center, range, 135f);

        leftLine.Start  = center - (width * Vector3.right) + (height * Vector3.forward);
        leftLine.End    = endPoint;
        rightLine.Start = center + (width * Vector3.right) + (height * Vector3.forward);
        rightLine.End   = startPoint;
    }

    public void TurnOn()
    {
        if (!Cursor.visible) return;
        if (inMeleeMode) return;
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(Fade(1f));
        isOn = true;
    }

    public void TurnOff()
    {
        if (_toggledOn) return;
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(Fade(0f));
        isOn = false;
    }

    private IEnumerator Fade(float targetAlpha)
    {
        float startAlphaLine    = leftLine.Color.a;
        float startAlphaArc     = arc.Color.a;
        float startAlphaArc2Out = arc2.ColorOuter.a;
        float elapsed = 0f;

        while (elapsed < _fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _fadeDuration);

            float lineA = Mathf.Lerp(startAlphaLine, _lineTargetColor.a * targetAlpha, t);
            leftLine.Color  = new Color(_lineTargetColor.r, _lineTargetColor.g, _lineTargetColor.b, lineA);
            rightLine.Color = new Color(_lineTargetColor.r, _lineTargetColor.g, _lineTargetColor.b, lineA);

            arc.Color = new Color(_arcTargetColor.r, _arcTargetColor.g, _arcTargetColor.b,
                Mathf.Lerp(startAlphaArc, _arcTargetColor.a * targetAlpha, t));

            float outerA = Mathf.Lerp(startAlphaArc2Out, _arc2TargetOuter.a * targetAlpha / 2f, t);
            arc2.ColorInner = new Color(_arc2TargetOuter.r, _arc2TargetOuter.g, _arc2TargetOuter.b, 0f);
            arc2.ColorOuter = new Color(_arc2TargetOuter.r, _arc2TargetOuter.g, _arc2TargetOuter.b, outerA);

            yield return null;
        }
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
        if (inMeleeMode) TurnOff();
        else TurnOn();
    }
}
