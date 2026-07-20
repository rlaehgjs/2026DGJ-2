using UnityEngine;

public class RefrigeratorInspectInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private GameProgressManager gameProgressManager;

    public bool CanInteract(PlayerInventory inventory)
    {
        return gameProgressManager != null
            && gameProgressManager.CurrentState == GameProgressState.InspectRefrigerator;
    }

    public void Interact(PlayerInventory inventory)
    {
        if (!CanInteract(inventory))
        {
            return;
        }

        gameProgressManager.TryCompleteRefrigeratorInspection();
    }
}
