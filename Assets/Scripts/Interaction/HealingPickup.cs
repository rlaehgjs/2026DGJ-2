using UnityEngine;

public class HealingPickup : MonoBehaviour
{
    [Min(0.1f)][SerializeField] private float healAmount = 25f;
    [SerializeField] private string saveId;
    [SerializeField] private SaveManager saveManager;

    private bool isCollected;

    private void Start()
    {
        if (saveManager != null
            && !string.IsNullOrWhiteSpace(saveId)
            && saveManager.IsItemCollected(saveId))
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerMeltSystem meltSystem = other.GetComponentInParent<PlayerMeltSystem>();
        TryCollect(meltSystem);
    }

    private void TryCollect(PlayerMeltSystem meltSystem)
    {
        if (isCollected
            || meltSystem == null
            || string.IsNullOrWhiteSpace(saveId)
            || saveManager == null)
        {
            return;
        }

        if (meltSystem.TryHeal(healAmount))
        {
            isCollected = true;
            saveManager.RegisterCollectedItem(saveId);
            gameObject.SetActive(false);
        }
    }
}
