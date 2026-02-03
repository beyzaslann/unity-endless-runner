using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    public int value = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        GameManagerRunner.Instance?.AddScore(value);

        // Destroy yerine pool'a dön
        SimplePool.Instance.Despawn("Coin", gameObject);
    }
}
