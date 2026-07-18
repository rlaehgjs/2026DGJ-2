using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Data/Item Data")]
public class ItemData : ScriptableObject
{
    [SerializeField] private string itemId = string.Empty;
    [SerializeField] private string displayNameKey = string.Empty;
    [SerializeField] private string descriptionKey = string.Empty;
    [SerializeField] private Sprite icon;
    [Min(1)][SerializeField] private int maxStack = 1;

    public string ItemId => itemId;
    public string DisplayNameKey => displayNameKey;
    public string DescriptionKey => descriptionKey;
    public Sprite Icon => icon;
    public int MaxStack => maxStack;
}
