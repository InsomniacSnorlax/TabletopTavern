using UnityEngine;

namespace TJ
{
    public class WinterUpdateGO : MonoBehaviour
    {
        public bool enableInWinterUpdate = true;
        void Start()
        {
# if WINTER_UPDATE
            gameObject.SetActive(enableInWinterUpdate);
#else
            gameObject.SetActive(!enableInWinterUpdate);
#endif
        }
    }
}
