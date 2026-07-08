using UnityEngine;

namespace TJ
{
    public class UpdateSpecificCanvasGroup : MemoriCanvasGroup
    {
        public bool spellsUpdate;

        private void Start()
        {
            if(spellsUpdate)
            {
#if UNITY_EDITOR || SPELLS
            CGEnable();
#else
            CGDisable();
#endif
            }
        }
    }
}
