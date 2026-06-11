using UnityEngine;
using Memori.Audio;

namespace TJ
{
    public class CoinRigidBody : MonoBehaviour
    {
        bool soundPlayed = false;
        private void Start()
        {
            Physics.simulationMode = SimulationMode.FixedUpdate;
        }
        private void OnCollisionEnter(Collision collision)
        {
            if (soundPlayed) return;

            string tag = collision.gameObject.tag;
            // Debug.Log($"Collided with object tagged: {tag}");

            if (tag == "Coin")
            {
                IAudioRequester.Instance.PlaySFX(SFXData.CoinClink);
            }
            else
            {
                IAudioRequester.Instance.PlaySFX(SFXData.CoinDrop);
            }

            soundPlayed = true;
        }
    }
}
