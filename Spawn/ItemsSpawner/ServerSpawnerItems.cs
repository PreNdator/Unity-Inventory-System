using Mirror;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class ServerSpawnerItems : NetworkBehaviour
{
    private ItemIDStorage _itemIDStorage;
    [SerializeField]
    private Item _itemPrefab;
    [SerializeField]
    private ShopItem _shopItemPrefab;
    private List<Item> _items = new List<Item>();
    public List<Item> Items => _items;

    [Inject]
    private void Construct(ItemIDStorage itemIDStorage)
    {
        _itemIDStorage = itemIDStorage;
    }

    protected void Start()
    {
        ProjectContext.Instance.InjectWithMainSceneContext(this);
    }

    [Server]
    public Item SpawnCoin(Vector3 pos)
    {
        return SpawnItem(_itemIDStorage.IDByInfo(_itemIDStorage.GetCoin(1)), pos);
    }

    [Server]
    public Item SpawnShopItem(int id, Vector3 pos)
    {
        if (_itemIDStorage.IsValidID(id))
        {
            ShopItem item = Instantiate(_shopItemPrefab, pos, Quaternion.identity);
            NetworkServer.Spawn(item.gameObject);
            item.SetItemID(id);
            RpcAddItemToList(item);
            return item;
        }
        return null;
    }

    [Server]
    public Item SpawnItem(int id, Vector3 pos)
    {
        if (_itemIDStorage.IsValidID(id))
        {
            Item item = Instantiate(_itemPrefab, pos, Quaternion.identity);
            NetworkServer.Spawn(item.gameObject);
            item.SetItemID(id);
            RpcAddItemToList(item);
            return item;
        }
        return null;
    }

    [ClientRpc]
    private void RpcAddItemToList(Item item)
    {
        if (item != null && !_items.Contains(item))
        {
            _items.Add(item);
        }
    }
    private void RpcRemove(Item item)
    {
        if (item == null) return;

        _items.Remove(item);
    }

    [Server]
    public void RemoveItem(Item item)
    {
        if (item == null) return;
        RpcRemove(item);

        if (item.gameObject != null)
        {
            NetworkServer.Destroy(item.gameObject);
        }
    }

}