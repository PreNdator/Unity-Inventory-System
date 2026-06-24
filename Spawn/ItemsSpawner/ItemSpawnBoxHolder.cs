using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class ItemSpawnBoxHolder
{
    private List<ItemSpawnBox> _spawnBoxes = new List<ItemSpawnBox>();
    private ItemIDStorage _itemIDStorage;

    private List<ItemSpawnBox> _bag = new List<ItemSpawnBox>();
    private int _bagIndex = 0;

    private int _shuffleIndex = 10; //change to test dencity

    [Inject]
    private void Construct(ItemIDStorage itemIDStorage)
    {
        _itemIDStorage = itemIDStorage;
    }

    public void Clear()
    {
        _spawnBoxes.Clear();

        _bag.Clear();
        _bagIndex = 0;
    }

    public void AddBox(ItemSpawnBox box)
    {
        if (box != null)
        {
            _spawnBoxes.Add(box);
        }
    }

    [Server]
    private ItemSpawnBox GetRandomSpawnBox()
    {
        if (_spawnBoxes.Count == 0)
        {
            Debug.LogWarning("No available spawn boxes");
            return null;
        }

        if (_bag.Count != _spawnBoxes.Count || _bagIndex >= _bag.Count || _bagIndex >= _shuffleIndex)
        {
            _bag = new List<ItemSpawnBox>(_spawnBoxes);
            Shuffle(_bag);
            _bagIndex = 0;
        }

        return _bag[_bagIndex++];
    }

    [Server]
    private int GetRandomItemID()
    {
        int itemID = _itemIDStorage.GetRandomSpawnableID();
        if (itemID == -1)
        {
            Debug.LogWarning("Failed to spawn item, no valid item IDs.");
            return -1;
        }
        return itemID;
    }

    private static void Shuffle(List<ItemSpawnBox> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    [Server]
    public void FillAllSpawnBoxesWithShopItems()
    {
        if (_spawnBoxes.Count == 0)
        {
            Debug.LogWarning("No available spawn boxes");
            return;
        }

        foreach (var selectedBox in _spawnBoxes)
        {
            int itemID = GetRandomItemID();
            if (itemID == -1)
            {
                Debug.LogWarning("Failed to spawn item, no valid item IDs.");
                continue;
            }

            selectedBox.SpawnShopItem(itemID);

            ItemInfo itemInfo = _itemIDStorage.InfoByID(itemID);
        }
    }

    [Server]
    public int RandomShopSpawn()
    {
        ItemSpawnBox selectedBox = GetRandomSpawnBox();
        if (selectedBox == null)
            return 1;

        int itemID = GetRandomItemID();
        if (itemID == -1)
            return 1;

        selectedBox.SpawnShopItem(itemID);

        ItemInfo itemInfo = _itemIDStorage.InfoByID(itemID);
        if (itemInfo is ItemInfoSellable sellableItem)
        {
            return sellableItem.Price;
        }
        else
        {
            Debug.LogWarning("Spawned item does not have a valid price.");
            return 1;
        }
    }

    [Server]
    public int RandomSpawn(float coinSpawnChance = 0.5f)
    {
        ItemSpawnBox selectedBox = GetRandomSpawnBox();
        if (selectedBox == null)
            return 1;

        if (Random.value < coinSpawnChance)
        {
            selectedBox.SpawnCoin();
            return 1;
        }
        else
        {
            int itemID = GetRandomItemID();
            if (itemID == -1)
                return 1;

            selectedBox.SpawnItem(itemID);

            ItemInfo itemInfo = _itemIDStorage.InfoByID(itemID);
            if (itemInfo is ItemInfoSellable sellableItem)
            {
                return sellableItem.Price;
            }
            else
            {
                Debug.LogWarning("Spawned item does not have a valid price.");
                return 1;
            }
        }
    }
}