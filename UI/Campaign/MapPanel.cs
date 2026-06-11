using System.Collections.Generic;
using System.Threading.Tasks;
using Memori.Utilities;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;

namespace TJ
{
    [RequireComponent(typeof(MemoriCanvasGroup))]
    public abstract class MapPanel : MonoBehaviour
    {
        public abstract void ClosePanel();
        [SerializeField] protected MMF_Player OpenFeedback;
        protected void CloseFeedback()
        {
            MMF_Position posFeedback = OpenFeedback.GetFeedbackOfType<MMF_Position>();
            if(posFeedback != null) {
                // Debug.Log("Yo");
                posFeedback.AnimatePositionTarget.transform.localPosition = posFeedback.InitialPosition;
            }
        }
    }
}