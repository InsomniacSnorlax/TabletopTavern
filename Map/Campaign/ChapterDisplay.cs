using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.UI;

namespace TJ.Map
{
public class ChapterDisplay : MonoBehaviour
{
    [SerializeField] private CanvasGroup chapterCanvasGroup;
    [SerializeField] private GameObject enemyIcon, enemyCompletedIcon, bossIcon, bossCompletedIcon;
    [SerializeField] private Color mainColor, activeColor;
    // [SerializeField] private Animator activeAnimator;
    [SerializeField] private bool isBoss;
    public bool IsBoss => isBoss;
    [SerializeField] private Transform iconToScale;
    MMF_Player mMF_Player;

    private void Awake()
    {
        mMF_Player = GetComponent<MMF_Player>();
    }

    public void ShowChapterCompleted(bool _isCompleted)
    {
        if(_isCompleted) {
            mMF_Player.StopFeedbacks();
            iconToScale.localScale = Vector3.one;
        }
        // activeAnimator.SetBool("Active", false);
        chapterCanvasGroup.alpha = _isCompleted ? 1 : 0.25f;
        if (isBoss) {
            bossIcon.SetActive(true);
            bossCompletedIcon.SetActive(_isCompleted);

            enemyIcon.SetActive(false);
            enemyCompletedIcon.SetActive(false);
            bossIcon.GetComponent<Image>().color = mainColor;
        } else {
            enemyIcon.SetActive(true);
            enemyCompletedIcon.SetActive(_isCompleted);
            enemyIcon.GetComponent<Image>().color = mainColor;
            
            bossIcon.SetActive(false);
            bossCompletedIcon.SetActive(false);
        }
    }
    public void ShowBattleCompleted()
    {
        if (isBoss) {
            bossIcon.SetActive(true);
            bossIcon.GetComponent<Image>().color = activeColor;
            bossCompletedIcon.SetActive(true);
        } else {
            enemyIcon.SetActive(true);
            enemyIcon.GetComponent<Image>().color = activeColor;
            enemyCompletedIcon.SetActive(true);
        }
        // activeAnimator.SetBool("Active", false);
        mMF_Player.StopFeedbacks();
        iconToScale.localScale = Vector3.one;
    }
    public void SetChapterActive()
    {
        mMF_Player.PlayFeedbacks();
        bossCompletedIcon.SetActive(false);
        enemyCompletedIcon.SetActive(false);
        enemyIcon.GetComponent<Image>().color = activeColor;
        bossIcon.GetComponent<Image>().color = activeColor;
        // activeAnimator.SetBool("Active", true);
        chapterCanvasGroup.alpha = 1;
    }

}
}

