using UnityEngine;

public class HeroAnimatorSetup : MonoBehaviour
{
    [SerializeField] private bool isMasculine;

    private static readonly int IsMasculineHash = Animator.StringToHash("isMasculine");

    private void Awake()
    {
        GetComponent<Animator>().SetBool(IsMasculineHash, isMasculine);
    }
}
