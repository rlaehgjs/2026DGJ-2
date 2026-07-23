using UnityEngine;

public class FrontDoorLock : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemData requiredKey;
    [Min(1)][SerializeField] private int requiredAmount = 1;
    [SerializeField] private GameProgressManager gameProgressManager;
    [SerializeField] private Collider blockingCollider;

    public bool IsUnlocked => gameProgressManager != null
        && gameProgressManager.CurrentState >= GameProgressState.FindGenerator;

    public bool IsLocked => !IsUnlocked;

    private void Awake()
    {
        ResolveBlockingCollider();
    }

    private void OnEnable()
    {
        if (gameProgressManager != null)
        {
            gameProgressManager.ProgressChanged += HandleProgressChanged;
        }
    }

    private void OnDisable()
    {
        if (gameProgressManager != null)
        {
            gameProgressManager.ProgressChanged -= HandleProgressChanged;
        }
    }

    private void Start()
    {
        if (IsUnlocked)
        {
            DisableBlockingCollider();
        }
    }

    public bool CanInteract(PlayerInventory inventory)
    {
        return IsLocked
            && inventory != null
            && requiredKey != null
            && gameProgressManager != null
            && gameProgressManager.CurrentState == GameProgressState.FindFrontDoorKey
            && inventory.HasItem(requiredKey.ItemId, requiredAmount);
    }

    public void Interact(PlayerInventory inventory)
    {
        if (!CanInteract(inventory))
        {
            return;
        }

        if (gameProgressManager.TryCompleteFrontDoorKeyCollection())
        {
            DisableBlockingCollider();
        }
    }

    private void HandleProgressChanged(GameProgressState state)
    {
        if (state >= GameProgressState.FindGenerator)
        {
            DisableBlockingCollider();
        }
    }

    private void DisableBlockingCollider()
    {
        ResolveBlockingCollider();

        if (blockingCollider != null)
        {
            blockingCollider.enabled = false;
        }
    }

    private void ResolveBlockingCollider()
    {
        if (blockingCollider != null)
        {
            return;
        }

        foreach (Collider candidate in GetComponentsInChildren<Collider>(true))
        {
            if (candidate.GetComponentInParent<DoorInteractable>() == null)
            {
                blockingCollider = candidate;
                return;
            }
        }
    }
}
