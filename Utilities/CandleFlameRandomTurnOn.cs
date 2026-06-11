using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TJ.Tavern
{
[RequireComponent(typeof(ParticleSystem))]
public class CandleFlameRandomTurnOn : MonoBehaviour
{
	public IEnumerator Start()
    {
        yield return new WaitForSeconds(Random.Range(0f,2f));
        GetComponent<ParticleSystem>().Play();
    }
}
}
