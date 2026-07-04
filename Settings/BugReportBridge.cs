using Memori.Localization;
using Memori.Utilities;
using Memori.Notifications;
using UnityEngine;

namespace TJ
{
    public class BugReportBridge : MonoBehaviour
    {
        [SerializeField] private ReportABugScreen reportABugScreen;

        private void OnEnable()
        {
            reportABugScreen.OnBlankSubmit.AddListener(OnBlankSubmit);
        }

        private void OnDisable()
        {
            reportABugScreen.OnBlankSubmit.RemoveListener(OnBlankSubmit);
        }

        private void OnBlankSubmit()
        {
            string localizedMessage = LocalizationManager.Instance.GetText("bugReportBlankSubmit");
            NotificationManager.Instance.ErrorNotification(localizedMessage);
        }
    }
}
