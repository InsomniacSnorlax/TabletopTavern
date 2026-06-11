using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif
using System.Collections.Generic;

namespace TJ
{
    [ExecuteInEditMode]
    public class PoseSkinnedMeshEditor : MonoBehaviour
    {
        public SkinnedMeshRenderer skinnedMeshRenderer;
        public Animator animator;       // The Animator component
        [HideInInspector] public string animationState;  // The selected animation state name
        public int frame;              // The target frame
        private int frameRate = 30;     // Fallback frame rate if clip doesn't provide one
        [HideInInspector] public int selectedStateIndex = 0; // Index for state dropdown
        private string[] stateNames = new string[0]; // Cached animation state names

        // Called when the script is added to a GameObject or reset in the Inspector
        private void Reset()
        {
            if (animator != null) return;

            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogWarning("No Animator component found on this GameObject.");
            }
            UpdateStateNames();
        }

        // Called when the script is loaded or a value changes in the Inspector
        private void OnValidate()
        {
            UpdateStateNames();
            if (stateNames.Length > 0 && selectedStateIndex >= 0 && selectedStateIndex < stateNames.Length)
            {
                animationState = stateNames[selectedStateIndex];
            }
        }

        // Fetch animation state names from the Animator Controller
        private void UpdateStateNames()
        {
            List<string> names = new List<string>();
            if (animator != null && animator.runtimeAnimatorController != null)
            {
#if UNITY_EDITOR
                var controller = animator.runtimeAnimatorController as AnimatorController;
                if (controller != null)
                {
                    foreach (var layer in controller.layers)
                    {
                        foreach (var state in layer.stateMachine.states)
                        {
                            names.Add(state.state.name);
                        }
                    }
                }
#endif
            }

            stateNames = names.ToArray();
            // Ensure the selected index is valid
            if (selectedStateIndex >= stateNames.Length)
            {
                selectedStateIndex = 0;
            }
        }

        private AnimationClip GetClipForState(string stateName)
        {
#if UNITY_EDITOR
            var controller = animator.runtimeAnimatorController as AnimatorController;
            if (controller != null)
            {
                foreach (var layer in controller.layers)
                {
                    foreach (var state in layer.stateMachine.states)
                    {
                        if (state.state.name == stateName)
                        {
                            if (state.state.motion is AnimationClip clip)
                            {
                                return clip;
                            }
                        }
                    }
                }
            }
#endif
            return null;
        }

        public void SetAnimationToFrame()
        {
            if (!animator)
            {
                animator = GetComponent<Animator>();
                if (!animator)
                {
                    Debug.LogError("No Animator component found.");
                    return;
                }
            }
            if (string.IsNullOrEmpty(animationState))
            {
                Debug.LogError("No animation state specified.");
                return;
            }

            var clip = GetClipForState(animationState);
            if (clip == null)
            {
                Debug.LogError("Could not find AnimationClip for state " + animationState);
                return;
            }

            float clipFrameRate = clip.frameRate > 0 ? clip.frameRate : frameRate;
            float length = clip.length;
            int maxFrame = Mathf.RoundToInt(length * clipFrameRate);

            if (frame < 0) frame = 0;
            if (frame > maxFrame) frame = maxFrame;

            float normalizedTime = maxFrame > 0 ? (float)frame / maxFrame : 0f;

            // Play the animation state and set the normalized time
            animator.Play(animationState, 0, normalizedTime);
            animator.Update(0); // Force the Animator to update immediately

            Debug.Log($"Set animation state '{animationState}' to frame {frame}.");
        }
        public void ResetToBindPose()
        {
            if (skinnedMeshRenderer == null)
                skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();

            if (skinnedMeshRenderer == null)
            {
                Debug.LogError("No SkinnedMeshRenderer found!");
                return;
            }

            var bones = skinnedMeshRenderer.bones;
            var bindPoses = skinnedMeshRenderer.sharedMesh.bindposes;

            if (bones.Length != bindPoses.Length)
            {
                Debug.LogError($"Bone count mismatch: {bones.Length} bones vs {bindPoses.Length} bindposes!");
                return;
            }

            // Map bone Transforms to their index in the bones array
            var boneIndex = new System.Collections.Generic.Dictionary<Transform, int>();
            for (int i = 0; i < bones.Length; i++)
                boneIndex[bones[i]] = i;

            // Start recursion from root bone
            ResetBoneRecurse(skinnedMeshRenderer.rootBone, boneIndex, bindPoses, null);

            // Force editor refresh (better than non-existent Update())
#if UNITY_EDITOR
            EditorUtility.SetDirty(skinnedMeshRenderer);
            SceneView.RepaintAll();  // Refresh scene view immediately
#endif
        }
        private void ResetBoneRecurse(Transform bone, System.Collections.Generic.Dictionary<Transform, int> boneIndex, Matrix4x4[] bindPoses, Matrix4x4? parentWorldMatrix)
        {
            if (bone == null || !boneIndex.TryGetValue(bone, out int boneIdx))
                return;

            // Desired world matrix at bind pose = inverse(bindPose)
            Matrix4x4 desiredWorldMatrix = Matrix4x4.Inverse(bindPoses[boneIdx]);

            // Compute local matrix relative to parent
            Matrix4x4 localMatrix;
            if (parentWorldMatrix.HasValue)
            {
                localMatrix = parentWorldMatrix.Value.inverse * desiredWorldMatrix;
            }
            else
            {
                localMatrix = desiredWorldMatrix;
            }
#if UNITY_EDITOR
            Undo.RecordObject(bone, "Reset Bone to Bind Pose");
#endif
            // Translation: last column (row-major access: m03, m13, m23)
            Vector3 localPosition = new Vector3(
                localMatrix.m03,
                localMatrix.m13,
                localMatrix.m23
            );

            // Rotation: built-in property (safe for uniform/positive scale)
            Quaternion localRotation = localMatrix.rotation;

            // Scale: built-in (lossy but usually accurate enough for bind pose)
            Vector3 localScale = localMatrix.lossyScale;

            bone.localPosition = localPosition;
            bone.localRotation = localRotation;
            bone.localScale   = localScale;

            // Recurse to children
            foreach (Transform child in bone)
            {
                ResetBoneRecurse(child, boneIndex, bindPoses, desiredWorldMatrix);
            }
        }
        public void SetIdle()
        {
            animator.Play("walk", 0, 0f);
            animator.SetFloat("velocity", 0f);     // or whatever value picks your desired motion
            animator.Update(0f);
        }
    }

#if UNITY_EDITOR

    // Custom Editor to display the dropdown for animation states
    [CustomEditor(typeof(PoseSkinnedMeshEditor))]
    public class PoseSkinnedMeshEditorInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            PoseSkinnedMeshEditor script = (PoseSkinnedMeshEditor)target;

            // Draw default inspector fields
            DrawDefaultInspector();

            // Get the state names
            string[] stateNames = (string[])script.GetType().GetField("stateNames", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(script);

            if (stateNames != null && stateNames.Length > 0)
            {
                // Draw the dropdown for animation states
                int selectedIndex = EditorGUILayout.Popup("Animation State", script.selectedStateIndex, stateNames);
                if (selectedIndex != script.selectedStateIndex)
                {
                    script.selectedStateIndex = selectedIndex;
                    script.animationState = stateNames[selectedIndex];
                    EditorUtility.SetDirty(script); // Mark the object as dirty to save changes
                }
            }
            else
            {
                EditorGUILayout.LabelField("No animation states found. Ensure an Animator with a valid Controller is assigned.");
            }

            // Button to trigger SetAnimationToFrame
            EditorGUILayout.Space(10);
            if (GUILayout.Button("Set Animation Frame"))
            {
                script.SetAnimationToFrame();
            }

            // Buttons to step forward and backward
            if (GUILayout.Button("Next Frame"))
            {
                script.frame += 1;
                script.SetAnimationToFrame();
            }

            if (GUILayout.Button("Previous Frame"))
            {
                script.frame -= 1;
                script.SetAnimationToFrame();
            }

            if (GUILayout.Button("RESET TO T-POSE"))
            {
                script.ResetToBindPose();
            }

            if (GUILayout.Button("Set Idle Pose"))
            {
                script.SetIdle();
            }
        }
    }
#endif
}