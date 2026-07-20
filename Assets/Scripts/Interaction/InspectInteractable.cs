using UnityEngine;

public class InspectInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private GameProgressManager gameProgressManager;

    public bool CanInteract(PlayerInventory inventory)
    {
        return gameProgressManager != null
            && gameProgressManager.CurrentState == GameProgressState.InspectRefrigerator;
    }

    public void Interact(PlayerInventory inventory)
    {
        if (gameProgressManager == null)
        {
            return;
        }

        gameProgressManager.TryCompleteRefrigeratorInspection();
    }
}
