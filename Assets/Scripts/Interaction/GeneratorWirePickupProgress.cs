using UnityEngine;

public class GeneratorWirePickupProgress : MonoBehaviour
{
    [SerializeField] private PickupInteractable pickupInteractable;
    [SerializeField] private GameProgressManager gameProgressManager;

    private void OnEnable()
    {
        if (pickupInteractable != null)
        {
            pickupInteractable.ItemCollected += HandleItemCollected;
        }

        if (gameProgressManager != null)
        {
            gameProgressManager.ProgressChanged += HandleProgressChanged;
            gameProgressManager.ProgressRestored += HandleProgressRestored;
        }

        RefreshPickupAvailability();
    }

    private void OnDisable()
    {
        if (pickupInteractable != null)
        {
            pickupInteractable.ItemCollected -= HandleItemCollected;
        }

        if (gameProgressManager != null)
        {
            gameProgressManager.ProgressChanged -= HandleProgressChanged;
            gameProgressManager.ProgressRestored -= HandleProgressRestored;
        }
    }

    private void HandleItemCollected(ItemData _, int __)
    {
        if (gameProgressManager != null)
        {
            gameProgressManager.TryCompleteGeneratorWire();
        }
    }

    private void HandleProgressChanged(GameProgressState _)
    {
        RefreshPickupAvailability();
    }

    private void HandleProgressRestored(GameProgressState _)
    {
        RefreshPickupAvailability();
    }

    private void RefreshPickupAvailability()
    {
        if (pickupInteractable == null)
        {
            return;
        }

        pickupInteractable.enabled = gameProgressManager != null
            && gameProgressManager.CurrentState == GameProgressState.FindGeneratorWire;
    }
}
