using UnityEngine;

namespace TJ
{
    [RequireComponent(typeof(MeshRenderer))]
    public class SquadFlagGameObjectTag : MonoBehaviour
    {
        public MeshRenderer FlagMeshRenderer { get; private set; }
        private void Awake() 
        {
            FlagMeshRenderer = GetComponent<MeshRenderer>();
        }
    }
}
