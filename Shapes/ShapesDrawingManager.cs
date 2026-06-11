using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shapes;
using TJ.Spells;

namespace TJ.Shapes
{
public class ShapesDrawingManager : ImmediateModeShapeDrawer
{
    [Header("Cursor Drawer")]
    [SerializeField] private SpellManager spellManager;
    [SerializeField] private SpellCursorDrawer spellCursorDrawer;

    [SerializeField] private float spellCursorThickness = 0.025f;
    [SerializeField] private float dashSize = 0.025f, dashSpace = 0.025f;
    private Color spellCursorValidColor = new Vector4(){x=0, y=1, z=0, w=250f}, spellCursorInvalidColor = new Vector4(){x=1, y=0, z=0, w=250f};
    
    public override void DrawShapes( Camera cam )
    {
        using( Draw.Command( cam ) ){

            if(BattleManager.Instance.CursorMode == CursorMode.CastSpell && spellManager.MouseReleased) {
                Draw.ZTest = UnityEngine.Rendering.CompareFunction.Always;
                Draw.Ring(
                    spellManager.SpellCursorOrigin, 
                    Quaternion.Euler(90, 0, 0), 
                    spellCursorDrawer.SpellCursorRadius, 
                    spellCursorThickness, 
                    DiscColors.Radial(Color.clear, spellManager.ValidSpellCastPoint ? spellCursorValidColor : spellCursorInvalidColor)
                );

                Draw.Sphere(
                    spellManager.SpellCursorOrigin, 
                    spellCursorDrawer.SpellCursorHitPointSphereRadius, 
                    spellManager.ValidSpellCastPoint ? spellCursorValidColor : spellCursorInvalidColor
                );

                Draw.DashedScope(new DashStyle(){ size = dashSize, spacing = dashSpace, offset = 0, });
                Draw.Ring(
                    spellManager.SpellCursorOrigin, 
                    Quaternion.Euler(90, spellCursorDrawer.SpellCursorRotationSpeed * Time.unscaledTime, 0), 
                    spellManager.SelectedSpellRadius, 
                    spellManager.ValidSpellCastPoint ? spellCursorValidColor : spellCursorInvalidColor
                );
            }
        }
    } 
}
}