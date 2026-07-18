using System;
using System.Collections.Generic;

[Serializable]
public class GameSaveData
{
    public const int CurrentVersion = 1;

    public int Version = CurrentVersion;
    public GameProgressState ProgressState = GameProgressState.FindKitchenKey;
    public List<ItemSaveEntry> Inventory = new List<ItemSaveEntry>();
    public List<string> CollectedItemIds = new List<string>(); //획득한 아이템 리스트
    public List<string> ChangedWorldObjectIds = new List<string>();

    public void EnsureCollections()
    {
        Inventory ??= new List<ItemSaveEntry>();
        CollectedItemIds ??= new List<string>();
        ChangedWorldObjectIds ??= new List<string>();
    }
}

[Serializable]
public class ItemSaveEntry
{
    public string ItemId;
    public int Amount;
}
