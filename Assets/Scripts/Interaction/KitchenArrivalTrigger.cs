using UnityEngine;

public class KitchenArrivalTrigger : MonoBehaviour
{
    [SerializeField] private GameProgressManager gameProgressManager;

    private void OnTriggerEnter(Collider other)
    {
        if (gameProgressManager == null)
        {
            return;
        }

        PlayerInventory inventory = other.GetComponentInParent<PlayerInventory>();

        if (inventory == null)
        {
            return;
        }

        gameProgressManager.TryCompleteKitchenArrival();
    }
}
