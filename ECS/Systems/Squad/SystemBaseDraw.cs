using Unity.Entities;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Shapes;
using Unity.Transforms;
using Unity.Mathematics;
// Classes that inherit from SystemBaseDraw can  use the Draw class from the Shapes library
public abstract partial class SystemBaseDraw : SystemBase
{
    protected override void OnStartRunning() => RenderPipelineManager.beginCameraRendering += DrawShapesSRP;
    protected override void OnStopRunning()  => RenderPipelineManager.beginCameraRendering -= DrawShapesSRP;
    private void DrawShapesSRP( ScriptableRenderContext ctx, Camera cam ) => OnCameraPreRender( cam );
    
    protected abstract void OnDraw(Camera cam);
    
    void OnCameraPreRender( Camera cam ) {
        switch( cam.cameraType ) {
            case CameraType.Reflection:
                return; // Don't render in reflection probes in case we run this script in the editor
        }

        // Don't draw Immediate Mode Shapes to cameras that cull the Shapes layer
        var myLayer = LayerMask.NameToLayer("Shapes");
        if ((cam.cullingMask & 1 << myLayer) != 0)
        {
            OnDraw(cam);
        }
    }
}

[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class LineDrawerSystem : SystemBaseDraw
{
    protected override void OnUpdate()
    {
    }

    protected override void OnDraw(Camera cam)
    {
        Draw.BlendMode = ShapesBlendMode.Lighten;
        Draw.ZTest = CompareFunction.LessEqual;

        using (Draw.Command(cam, RenderPassEvent.AfterRenderingOpaques))
        {
            foreach ((var line, Entity entity) in SystemAPI.Query<RefRO<TriangleEntity>>().WithEntityAccess())
            {
                Vector3 nodePos = SystemAPI.GetComponent<LocalTransform>(entity).Position;
                quaternion nodeRot = SystemAPI.GetComponent<LocalTransform>(entity).Rotation;

                float3 moulta = math.mul(nodeRot, new float3(0.5f, 0.2f, -0.5f)); //only the x and z axis are rotated
                float3 moultb = math.mul(nodeRot, new float3(-0.5f, 0.2f, -0.5f)); //only the x and z axis are rotated
                float3 moultc = math.mul(nodeRot, new float3(0, 0.2f, 0.5f));

                Draw.TriangleBorder(nodePos + new Vector3(moulta.x, moulta.y, moulta.z), nodePos + new Vector3(moultb.x, moultb.y, moultb.z), nodePos + new Vector3(moultc.x, moultc.y, moultc.z), 0.125f, 0.125f, line.ValueRO.activeColor);


            }
        }
    }
}