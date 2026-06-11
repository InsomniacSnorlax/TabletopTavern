using UnityEngine;

namespace TJ.Map
{
public class TutorialTurnOff : MonoBehaviour
{
    void Start()
    {
        TutorialManager.Instance.TurnOff();
    }

}
}