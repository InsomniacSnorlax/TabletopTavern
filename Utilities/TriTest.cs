using UnityEngine;
using Shapes;

public class TriTest : ImmediateModeShapeDrawer
{
    [SerializeField] private Transform targetA, targetB;
    public override void DrawShapes( Camera cam )
    {
        using (Draw.Command(cam))
        {
            // Draw.ZTest = UnityEngine.Rendering.CompareFunction.Always;
            // Draw.BlendMode = ShapesBlendMode.Transparent;
            Draw.LineGeometry = LineGeometry.Volumetric3D;
            Draw.ThicknessSpace = ThicknessSpace.Pixels;
            Draw.Thickness = 5;

            Draw.Matrix = transform.localToWorldMatrix;

			// draw lines
			Draw.Line( Vector3.zero, Vector3.right,   Color.red   );
			Draw.Line( Vector3.zero, Vector3.up,      Color.green );
			Draw.Line( Vector3.zero, Vector3.forward, Color.blue  );

                Vector3 nodeVisualPos = this.transform.position;
                Vector3 pointApos = nodeVisualPos + new Vector3(1, 0, -1);
                Vector3 pointBpos = nodeVisualPos + new Vector3(-1, 0, -1);
                Vector3 pointCpos = nodeVisualPos + new Vector3(0, 0, 1);
                // Debug.Log($"Drawing triangle at {nodeVisualPos}");
                Draw.Triangle(pointApos, pointBpos, pointCpos, 0.25f, Color.red);

                Draw.TriangleBorder(pointApos, pointBpos, pointCpos, 0.25f, Color.green);

                Draw.Line(pointApos, pointBpos, 0.25f, Color.blue);
                Draw.Line(targetA.position, targetB.position, 0.25f, Color.blue);

                Draw.Cube(nodeVisualPos, 0.1f);
        }
    }
}
