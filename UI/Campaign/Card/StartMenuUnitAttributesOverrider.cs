using UnityEngine;

namespace TJ
{
    [RequireComponent(typeof(UnitAttributesUIContainer))]
    public class StartMenuUnitAttributesOverrider : MonoBehaviour
    {
        void Start()
        {
            GetComponent<UnitAttributesUIContainer>().OverrideStatsDisplayOnStart();
        }
    }
}
