using UnityEngine;

namespace TJ.Map
{
[System.Serializable] public struct LayerNodeTypeWeight { public NodeType type; public float weight; }

[CreateAssetMenu(fileName = "LayerNodeProbability", menuName = "Map/LayerNodeProbability")]
public class LayerNodeProbability : ScriptableObject
{
    public LayerNodeTypeWeight[] nodeTypeWeights;
    public bool preventHidden;
}
}