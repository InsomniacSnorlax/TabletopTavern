using System.Threading.Tasks;
using Memori.Audio;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace TJ.Map
{
public class PlayerToken : MonoBehaviour
{
    [SerializeField] private GameObject[] heroPrefabs;
    [SerializeField] private MMF_Player hopFeedbacks;
    [SerializeField] private MMF_Player landFeedbacks;
    bool isHopping = false;
    MapSceneManager mapSceneManager;
    public void LoadHero()
    {
        int heroID = HeroBonusManager.Instance.ActiveHeroID;
        // Debug.Log($"Loading hero {heroID}");
        if(heroID == -1) heroID = 1; // Default to the first hero if no active hero is set

        heroPrefabs[heroID-1].SetActive(true);
    }
    public void StartHopping()
    {
        isHopping = true;
        hopFeedbacks.PlayFeedbacks();
    }
    public void FinishHop()
    {
        // Guards against a stray callback from a stale hop cycle after ReachedDestination
        // has already handed things off to landFeedbacks below.
        if (!isHopping) return;
        IAudioRequester.Instance.PlaySFX(SFXData.PlayerToken);
        hopFeedbacks.StopFeedbacks();
        hopFeedbacks.PlayFeedbacks();
    }
    public void ReachedDestination(MapSceneManager _mapSceneManager)
    {
        mapSceneManager = _mapSceneManager;
        if (!isHopping) return;
        isHopping = false;

        // Drive the hop -> land handoff from here (the position-lerp coroutine) rather than
        // waiting for the hop feedback's own repeat cycle to notice isHopping went false.
        // That old path called StopFeedbacks/PlayFeedbacks on hopFeedbacks from inside a
        // callback hopFeedbacks itself was invoking, which could desync under irregular
        // frame timing (e.g. a player alt-tabbing mid-hop) and leave the token stuck forever.
        IAudioRequester.Instance.PlaySFX(SFXData.PlayerToken);
        hopFeedbacks.StopFeedbacks();
        landFeedbacks.PlayFeedbacks();
    }
    public void OnLand()
    {
        mapSceneManager.FinishHopping();
    }

}
}