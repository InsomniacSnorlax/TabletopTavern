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
        IAudioRequester.Instance.PlaySFX(SFXData.PlayerToken);
        if(isHopping) {
            hopFeedbacks.StopFeedbacks();
            hopFeedbacks.PlayFeedbacks();
        } else {
            landFeedbacks.PlayFeedbacks();
        }
    }
    public void ReachedDestination(MapSceneManager _mapSceneManager)
    {
        mapSceneManager = _mapSceneManager;
        isHopping = false;
    }
    public void OnLand()
    {
        mapSceneManager.FinishHopping();
    }

}
}