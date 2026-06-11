using UnityEngine;

public class ParticleClearOnDisable : MonoBehaviour
{
    private ParticleSystem _ps;

    void Awake()
    {
        _ps = GetComponent<ParticleSystem>();
    }

    void OnDisable()
    {
        if (_ps != null)
            _ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}
