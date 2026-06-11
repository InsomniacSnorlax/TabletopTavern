#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class ReduceAnimationFrames : MonoBehaviour
{
    public AnimationClip animationClip;

 
    [ContextMenu("Reduce Keyframes")]
    void ReduceKeyframes()
    {
        AnimationClip clip = this.animationClip;
        foreach (var binding in AnimationUtility.GetCurveBindings(clip))
        {
            var curve = AnimationUtility.GetEditorCurve(clip, binding);
            var keyframes = curve.keys;

            if (keyframes.Length <= 1) continue; // Skip if not enough keyframes to process

            // Create a new array that will hold half the keyframes
            var newKeyframes = new Keyframe[(keyframes.Length + 1) / 2];

            // Copy every other keyframe and adjust the time so it "smushes" to the left
            for (int i = 0, j = 0; i < keyframes.Length; i += 2, j++)
            {
                // Adjust the keyframe time to fill in the gaps, so that it moves to where the next keyframe should be
                float newTime = keyframes[i].time * 0.5f; // Halve the time to smush the keyframes closer
                newKeyframes[j] = new Keyframe(newTime, keyframes[i].value, keyframes[i].inTangent, keyframes[i].outTangent);
            }

            // Set the new keyframes into the curve
            curve.keys = newKeyframes;
            AnimationUtility.SetEditorCurve(clip, binding, curve);
        }
    }
}
#endif