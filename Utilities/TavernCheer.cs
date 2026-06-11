using UnityEngine;

[RequireComponent(typeof(Animator))]
public class TavernCheer : MonoBehaviour
{
    private static readonly int CheerHash = Animator.StringToHash("Cheer");

    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void Cheer()
    {
        _animator.CrossFadeInFixedTime(CheerHash, 0.2f);
    }
}
