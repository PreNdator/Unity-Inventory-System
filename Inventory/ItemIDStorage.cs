using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class ItemIDStorage : MonoBehaviour
{
    

    [SerializeField]
    private List<ItemInfo> _itemInfoList = new List<ItemInfo>();

    private List<ItemType> _itemTypeList = new List<ItemType>();

    private List<ItemInfoMoney> _coins = new List<ItemInfoMoney>();

    private List<ItemInfo> _spawnableItems = new List<ItemInfo>();

    private Dictionary<Spell, ItemInfoSpellOnly> _spellOnlyList = new Dictionary<Spell, ItemInfoSpellOnly>();

    public const int NO_ITEM_ID = -1;

    public void PopulateItemInfoList()
    {
        //_itemInfoList.Clear();
#if UNITY_EDITOR
        string[] guids = AssetDatabase.FindAssets("t:ItemInfo");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ItemInfo itemInfo = AssetDatabase.LoadAssetAtPath<ItemInfo>(path);
            if (itemInfo != null && !_itemInfoList.Contains(itemInfo))
            {
                _itemInfoList.Add(itemInfo);
            }
        }

        Debug.Log($"{_itemInfoList.Count} ItemInfo objects loaded from project assets.");
#else
    Debug.LogWarning("PopulateItemInfoList can only be used in the Unity Editor.");
#endif
    }


    public void Awake()
    {
        foreach (ItemInfo itemInfo in _itemInfoList)
        {
            TypeCheck(itemInfo);
            AddSpells(itemInfo);
        }
        FillItemTypeList();
    }

    private void FillItemTypeList()
    {
        foreach (var itemInfo in _itemInfoList)
        {
            if (itemInfo is ItemInfoMoney)
            {
                _itemTypeList.Add(ItemType.Money);
            }
            else if (itemInfo is ItemInfoSellable)
            {
                _itemTypeList.Add(ItemType.Item);
            }
            else if (itemInfo is ItemInfoSpellOnly)
            {
                _itemTypeList.Add(ItemType.Spell);
            }
            else
            {
                _itemTypeList.Add(ItemType.None);
            }

        }

    }

    private void AddSpells(ItemInfo itemInfo)
    {
        if (itemInfo is ItemInfoSpellOnly spell)
        {
            if (_spellOnlyList.ContainsKey(spell.Spell))
            {
                Debug.LogError("Same spell already added.");
            }
            else
            {
                _spellOnlyList.Add(spell.Spell, spell);
            }
        }
    }

    private void TypeCheck(ItemInfo itemInfo)
    {
        if (itemInfo is ItemInfoMoney)
        {
            _itemTypeList.Add(ItemType.Money);
            _coins.Add(itemInfo as ItemInfoMoney);
        }
        else if (itemInfo is ItemInfoSellable)
        {
            _spawnableItems.Add(itemInfo);
            _itemTypeList.Add(ItemType.Item);
        }
        else if (itemInfo is ItemInfoSpellOnly)
        {
            _itemTypeList.Add(ItemType.Spell);
        }
        else
        {
            _itemTypeList.Add(ItemType.None);
        }
    }

    public int GetSpellID(Spell spell)
    {
        if (_spellOnlyList.TryGetValue(spell, out ItemInfoSpellOnly spellInfo))
        {
            return IDByInfo(spellInfo);
        }

        return -1;
    }
    public bool IsValidID(int id)
    {
        return (id >= 0 && id < _itemInfoList.Count);
    }

    public int GetRandomSpawnableID()
    {
        if (_spawnableItems.Count == 0)
        {
            Debug.LogWarning("No spawnable items available.");
            return NO_ITEM_ID;
        }

        ItemInfo randomItem = _spawnableItems[Random.Range(0, _spawnableItems.Count)];

        return IDByInfo(randomItem);
    }

    public ItemInfo GetCoin(int value)
    {
        foreach (ItemInfo itemInfo in _itemInfoList) {
            if (itemInfo is ItemInfoMoney money)
            {
                if (money.Count == value)
                {
                    return money;
                }
            }
        }
        Debug.LogWarning($"Coin with value {value} not found.");
        return null;
    }

    public ItemInfo InfoByID(int id)
    {
        if (IsValidID(id))
        {
            return _itemInfoList[id];
        }
        Debug.LogWarning($"Item with ID {id} not found.");
        return null;
    }

    public ItemType GetItemType(int id)
    {
        if (IsValidID(id))
        {
            return _itemTypeList[id];
        }
        else return ItemType.None;
    }



    public int IDByInfo(ItemInfo item)
    {
        int id = _itemInfoList.IndexOf(item);
        if (id == NO_ITEM_ID)
        {
            Debug.LogWarning("Item not found in the list.");
        }
        return id;
    }


}

public enum ItemType{
    None, 
    Item,
    Spell,
    Money
}
