using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shapes;
using TJ.Spells;

namespace TJ.Shapes
{
public class ShapesDrawingManager : ImmediateModeShapeDrawer
{
    [SerializeField] private SpellManager _spellManager;

    [SerializeField] private Color _spellCursorValidColor = new Vector4(){x=0, y=1, z=0, w=1f}, _spellCursorInvalidColor = new Vector4(){x=1, y=0, z=0, w=1f};
    
    [Header("Inner Ring")]
    [SerializeField] private float _spellCursorRadius = 1.5f;
    [SerializeField] private float _spellCursorHitPointSphereRadius = 0.25f;
    [SerializeField] private float _spellCursorThickness = 0.025f;
    [SerializeField] private float _cursorBloomIntensity = 100f;

    [Header("Outer Ring")]
    [SerializeField] private float _outerRingBloomIntensity = 100f;
    [SerializeField] private float _dashSize = 1.5f;
    [SerializeField] private float _dashSpace = 1.5f;
    [SerializeField] private float _spellRingRotationSpeed = 5f;
    [SerializeField] private float _spellRingThickness = 0.01f;

    public override void DrawShapes( Camera cam )
    {
        using( Draw.Command( cam ) ){

            if(BattleManager.Instance.CursorMode == CursorMode.CastSpell && _spellManager.MouseReleased)
            {
                Draw.ZTest = UnityEngine.Rendering.CompareFunction.Always;
                Color bloomColor = _spellManager.ValidSpellCastPoint ? _spellCursorValidColor : _spellCursorInvalidColor;
                bloomColor.a = _cursorBloomIntensity;

                // Hit point sphere
                Draw.Sphere(
                    _spellManager.SpellCursorOrigin,
                    _spellCursorHitPointSphereRadius,
                    bloomColor
                );

                bloomColor.a = _outerRingBloomIntensity;

                //Inner ring
                // Draw.Ring(
                //     _spellManager.SpellCursorOrigin,
                //     Quaternion.Euler(90, 0, 0),
                //     _spellCursorRadius,
                //     _spellCursorThickness,
                //     DiscColors.Radial(Color.clear, bloomColor)
                // );

                Draw.DashedScope(new DashStyle(){ size = _dashSize, spacing = _dashSpace, offset = 0, });

                // Outer ring
                Draw.Ring(
                    _spellManager.SpellCursorOrigin,
                    Quaternion.Euler(90, _spellRingRotationSpeed * Time.unscaledTime, 0),
                    _spellManager.SelectedSpellRadius,
                    _spellRingThickness,
                    bloomColor
                );
            }
        }
    }
}
}
