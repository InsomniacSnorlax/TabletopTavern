using UnityEngine;

/// <summary>
/// Replaces the material on every SkinnedMeshRenderer in this GameObject's children
/// with the provided shared material. Right-click the component and choose
/// "Apply Material To All Skinned Meshes" to run it in the editor.
/// </summary>
public class CombinedMaterialApplier : MonoBehaviour
{
    public Material sharedMaterial;

    [ContextMenu("Apply Material To All Skinned Meshes")]
    private void ApplyMaterial()
    {
        if (sharedMaterial == null)
        {
            Debug.LogError("No material assigned.", this);
            return;
        }

        SkinnedMeshRenderer[] renderers = GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);

        if (renderers.Length == 0)
        {
            Debug.LogWarning("No SkinnedMeshRenderers found in children.", this);
            return;
        }

        foreach (var smr in renderers)
        {
            // Replace all material slots with the shared material
            Material[] mats = new Material[smr.sharedMaterials.Length];
            for (int i = 0; i < mats.Length; i++)
                mats[i] = sharedMaterial;

#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(smr, "Apply Combined Material");
#endif
            smr.sharedMaterials = mats;
        }

        Debug.Log($"Applied '{sharedMaterial.name}' to {renderers.Length} SkinnedMeshRenderer(s) on '{gameObject.name}'.", this);
    }
}
