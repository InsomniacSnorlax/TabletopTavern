using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace TJ
{
    public class CoinDispenser : MonoBehaviour
    {
        [SerializeField] private CoinRigidBody coinPrefab;
        [SerializeField] private Transform dispensePoint;
        [SerializeField] private float dispenseInterval = 0.1f;

        private List<CoinRigidBody> activeCoins = new();

        [ContextMenu("Dispense Coin")]
        public async void DispenseCoin(int count)
        {
            //cap the max coins to prevent performance issues
            int maxCoins = 20;
            count = Mathf.Min(count, maxCoins);

            for (int i = 0; i < count; i++)
            {
                CoinRigidBody coin = Instantiate(coinPrefab, dispensePoint.position, dispensePoint.rotation);
                Rigidbody rb = coin.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = new Vector3(Random.Range(-0.5f, 0.5f), -2f + Random.Range(-1f, 0f), Random.Range(-0.5f, 0.5f));
                    rb.angularVelocity = Random.insideUnitSphere * 5f;  // Natural tumble
                }
                activeCoins.Add(coin);
                await Task.Delay(System.TimeSpan.FromSeconds(dispenseInterval));
            }
            await Task.Delay(System.TimeSpan.FromSeconds(1f));
        }

        [ContextMenu("Clear Coins")]
        public void ClearCoins()
        {
            StopAllCoroutines();
            foreach (CoinRigidBody coin in activeCoins)
            {
                if (coin != null)
                    DestroyImmediate(coin.gameObject);
            }
            activeCoins.Clear();
            CoinRigidBody[] coins = FindObjectsByType<CoinRigidBody>(FindObjectsSortMode.None);
            foreach (CoinRigidBody coin in coins)
            {
                DestroyImmediate(coin.gameObject);
            }
        }
        private void OnDestroy() {
            ClearCoins();
        }

        public void ClearCoinsOnShopLeave()
        {
            StopAllCoroutines();
            foreach (CoinRigidBody coin in activeCoins)
            {
                if (coin != null)
                    DestroyImmediate(coin.gameObject);
            }
            activeCoins.Clear();
        }
    }
}
