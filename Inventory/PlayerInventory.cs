using DG.Tweening;
using Mirror;
using System;
using System.Reflection;
using UnityEngine;
using Zenject;

public class PlayerInventory : NetworkBehaviour
{
    public event Action<int, ItemInfoSpellOnly> OnSpellChanged;
    public event Action<int, ItemInfoSellable> OnItemChanged;
    public event Action<int> OnMoneyChange;
    public event Action<int> OnCurrSlotChanged;

    [SerializeField]
    private RandomClip _pickUpSound;
    [SerializeField]
    private RandomClip _throwSound;
    [SerializeField]
    private RandomClip _sellSound;
    [SerializeField]
    private RandomClip _eraseSound;

    private ServerSpawnerItems _spawner;
    private ItemIDStorage _itemIDStorage;
    private LocalizationManager _localization;
    private ISfxPlayer _sfxPlayer;

    private const int NO_ITEM = -1;
    private const int NO_SLOT = -1;

    private int[] _spellsID;
    private int[] _itemsID;

    [SerializeField]
    private int _spells = 4;
    [SerializeField]
    private int _items = 4;
    [SyncVar(hook = nameof(MoneyChange))]
    private int _coins = 0;

    private int _currSlot = 0;
    private int _maxSlot;
    

    public int CurrSlot => _currSlot;

    public int Coins => _coins;
    public int Spells => _spells;
    public int Items => _items;



    protected void Start()
    {
        ProjectContext.Instance.InjectWithMainSceneContext(this);
        _spellsID = new int[_spells];
        _itemsID = new int[_items];
        for (int i = 0; i < _spells; i++)
        {
            _spellsID[i] = NO_ITEM;
        }
        for (int i = 0; i < _items; i++)
        {
            _itemsID[i] = NO_ITEM;
        }
        _maxSlot = _spells + _items + 1;
    }

    [Inject]
    private void Construct(ServerSpawnerItems spawner, ItemIDStorage itemIDStorage, LocalizationManager localization, ISfxPlayer sfxPlayer)
    {
        _sfxPlayer = sfxPlayer;
        _spawner = spawner;
        _itemIDStorage = itemIDStorage;
        _localization = localization;
    }

    public string GetText(int index)
    {
        ItemInfo info = GetInfo(index);
        if (info != null)
        {
            if (info is ItemInfoMoney)
            {
                return $"{_localization.GetText(info.ItemName)}: {Coins}";
            }
            else
            {
                return _localization.GetText(info.ItemName);
            }
        }
        return "";
    }

    [Server]
    public bool ServerDestroyAll()
    {
        bool destroyed = false;

        if (_coins > 0)
        {
            _coins = 0;
            destroyed = true;
        }

        if (_itemsID != null)
        {
            for (int i = 0; i < _itemsID.Length; i++)
            {
                if (_itemsID[i] != NO_ITEM)
                {
                    ChangeItemID(i, NO_ITEM);
                    destroyed = true;
                }
            }
        }

        if (_spellsID != null)
        {
            for (int i = 0; i < _spellsID.Length; i++)
            {
                if (_spellsID[i] != NO_ITEM)
                {
                    ChangeSpellID(i, NO_ITEM);
                    destroyed = true;
                }
            }
        }

        if (destroyed) RpcEraseSound(transform.position);
        return destroyed;
    }

    public ItemInfo GetInfo(int index)
    {
        if (_itemIDStorage == null) return null;
        if (index == 0)
        {
            return _itemIDStorage.GetCoin(1);
        }
        else
        {
            ItemInfoWithSpell info = null;
            int ind = NO_SLOT;
            if (index > 0 && index <= _items)
            {
                ind = GetIndexInItems(index);
                if (ind != NO_SLOT && _itemsID[ind] != NO_ITEM)
                {
                    info = _itemIDStorage.InfoByID(_itemsID[ind]) as ItemInfoWithSpell;
                }
            }
            else if (index > _items && index < _maxSlot)
            {
                ind = GetIndexInSpells(index);
                if (ind != NO_SLOT && _spellsID[ind] != NO_ITEM)
                {
                    info = _itemIDStorage.InfoByID(_spellsID[ind]) as ItemInfoWithSpell;
                }
            }

            return info;
        }
    }

    public Spell GetSpell(int index)
    {
        ItemInfo info = GetInfo(index);
        if (info != null)
        {
            if (info is ItemInfoWithSpell spell)
            {
                return spell.Spell;
            }
        }
        return null;
    }

    [Command]
    public void CmdTrySell(int index)
    {
        if (index > 0 && index <= _items)
        {
            int inArrIndex = GetIndexInItems(index);
            int id = _itemsID[inArrIndex];
            if (id != NO_ITEM && _itemIDStorage.IsValidID(id))
            {
                ItemInfo info = _itemIDStorage.InfoByID(id);
                if (info is ItemInfoSellable sellable)
                {
                    RpcSellSound(transform.position);
                    _coins += sellable.SellPrice;
                    ChangeItemID(inArrIndex, NO_ITEM);
                }
            }
        }
    }

    [Command]
    public void CmdThrowAll(Vector3 pos, float force) {
        ServerThrowAll(pos, force);
    }

    [Server]
    public void ServerThrowAll(Vector3 pos, float force)
    {
        while (_coins > 0)
        {
            TryThrowItem(0, pos, GetRandVelocity(force));
        }

        for (int i = 1; i < _maxSlot; ++i)
        {
            TryThrowItem(i, pos, GetRandVelocity(force));
        }
    }

    [Server]
    public void ServerThrowItems(Vector3 pos, float force)
    {
        while (_coins > 0)
        {
            TryThrowItem(0, pos, GetRandVelocity(force));
        }

        for (int i = 1; i < _items+1; ++i)
        {
            TryThrowItem(i, pos, GetRandVelocity(force));
        }
    }

    private Vector3 GetRandVelocity(float force)
    {
        return new Vector3(
            UnityEngine.Random.Range(-force, force),
            force,
            UnityEngine.Random.Range(-force, force)
            );
    }

    [Command]
    public void CmdTryPickup(Item item, int cost)
    {
        if (item == null)
        {
            Debug.LogWarning("Item is null. Possibly already picked up.");
            return;
        }
        if (cost > 0)
        {
            if(_coins < cost)
            {
                return;
            }
            else
            {
                _coins -= cost;
            }
        }
        ItemInfo info = item.Info;
        if (info is ItemInfoMoney)
        {
            _coins += (info as ItemInfoMoney).Count;
            RpcPickUpSound(item.transform.position);
            _spawner.RemoveItem(item);
        }
        else if (info is ItemInfoSellable)
        {
            int slot = TryFindItemSlot(GetIndexInItems(_currSlot));
            if (slot != NO_SLOT)
            {
                ChangeItemID(slot, item.ID);
                RpcPickUpSound(item.transform.position);
                _spawner.RemoveItem(item);
            }
        }
        else if(info is ItemInfoSpellOnly)
        {
            int slot = TryFindSpellSlot(GetIndexInSpells(_currSlot));
            if (slot != NO_SLOT)
            {
                ChangeSpellID(slot, item.ID);
                RpcPickUpSound(item.transform.position);
                _spawner.RemoveItem(item);
            }
        }
        else
        {
            Debug.LogWarning("Wrong ItemType");
        }
    }

    [ClientRpc]
    private void RpcThrowSound(Vector3 pos)
    {
        if (_sfxPlayer != null)
        {
            _sfxPlayer.PlayAtPoint(_throwSound, pos);
        }
    }

    [ClientRpc]
    private void RpcPickUpSound(Vector3 pos)
    {
        if(_sfxPlayer != null)
        {
            _sfxPlayer.PlayAtPoint(_pickUpSound, pos);
        }
    }
    [ClientRpc]
    private void RpcSellSound(Vector3 pos)
    {
        if (_sfxPlayer != null)
        {
            _sfxPlayer.PlayAtPoint(_sellSound, pos);
        }
    }
    public bool IsSpellSlot(int index)
    {
        return index > _items;
    }

    [Command]
    public void CmdTryThrow(int index, Vector3 pos)
    {
        TryThrowItem(index, pos);
    }

    [Command]
    public void CmdTryThrow(int index, Vector3 pos, Vector3 velocity)
    {
        TryThrowItem(index, pos, velocity);
    }


    [ClientRpc]
    private void ThrowRpc(Item item, Vector3 velocity)
    {
        if (item != null)
        {
            Rigidbody rb = item.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = velocity;
            }
        }
    }

    [Server]
    private void TryThrowItem(int index, Vector3 pos, Vector3 velocity)
    {
        Item item = TryThrowItem(index, pos);

        if (item != null)
        {
            ThrowRpc(item, velocity);
        }
    }

    [Server]
    public bool ServerDestroyItem(int index)
    {
        if (index == 0 && _coins > 0)
        {
            _coins -= 1;
            return true;
        }
        else
        if (index > 0 && index <= _items)
        {
            int inArrIndex = GetIndexInItems(index);
            if (_itemsID[inArrIndex] != NO_ITEM)
            {
                ChangeItemID(inArrIndex, NO_ITEM);
                return true;
            }
        }
        else if (index > _items && index < _maxSlot)
        {
            int inArrIndex = GetIndexInSpells(index);
            if (_spellsID[inArrIndex] != NO_ITEM)
            {
                ChangeSpellID(inArrIndex, NO_ITEM);
                return true;
            }
        }
        return false;
    }

    [Server]
    private Item TryThrowItem(int index, Vector3 pos)
    {
        if (index == 0 && _coins > 0)
        {
            _coins -= 1;
            RpcThrowSound(pos);
            return _spawner.SpawnCoin(pos);
        }
        else
        if (index > 0 && index <= _items)
        {
            int inArrIndex = GetIndexInItems(index);
            if (_itemsID[inArrIndex] != NO_ITEM)
            {
                int id = _itemsID[inArrIndex];
                ChangeItemID(inArrIndex, NO_ITEM);
                RpcThrowSound(pos);
                return _spawner.SpawnItem(id, pos);
            }
        }
        else if (index > _items && index < _maxSlot)
        {
            int inArrIndex = GetIndexInSpells(index);
            if (_spellsID[inArrIndex] != NO_ITEM)
            {
                int id = _spellsID[inArrIndex];
                ChangeSpellID(inArrIndex, NO_ITEM);
                RpcThrowSound(pos);
                return _spawner.SpawnItem(id, pos);
            }
        }
        return null;
    }

    public void AddToCurrSlot(int value)
    {
        ChooseCurrSlot(_currSlot+value);
    }


    public void ChooseCurrSlot(int slot)
    {
        if (slot < 0)
        {
            slot = _maxSlot + (slot % _maxSlot);
        }
        _currSlot = slot%_maxSlot;
        OnCurrSlotChanged?.Invoke(_currSlot);
    }

    private int TryFindItemSlot(int index)
    {
        for (int i = 0; i < _items; i++)
        {
           if(_itemsID[index] == NO_ITEM)
           {
                return index;
           }
           index = (index + 1)%_items;
        }
        return NO_SLOT;
    }

    private int TryFindSpellSlot(int index)
    {
        for (int i = 0; i < _spells; i++)
        {
            if (_spellsID[index] == NO_ITEM)
            {
                return index;
            }
            index = (index + 1) % _spells;
        }
        return NO_SLOT;
    }


    private int GetIndexInSpells(int index)
    {
        index -= 1 + _items;
        if(index < 0)
        {
            index = 0;
        }
        return index;
    }

    private int GetIndexInItems(int index)
    {
        index -= 1 ;  

        if (index < 0)
        {
            index = 0;
        }
        else if (index >= _items)
        {
            index = _items - 1;
        }
        return index;
    }


    [Server]
    private void ChangeSpellID(int slot, int id)
    {
        
        if (_itemIDStorage.GetItemType(id) == ItemType.Spell || id == NO_ITEM)
        {
            _spellsID[slot] = id;
            RpcUpdateSpellsID(slot, id);
        }
        else
        {
            Debug.LogError($"Wrong ItemType: {_itemIDStorage.GetItemType(id)}");
        }
    }

    [Server]
    private void ChangeItemID(int slot, int id)
    {
        if (_itemIDStorage.GetItemType(id) == ItemType.Item || id == NO_ITEM)
        {
            _itemsID[slot] = id;    
            RpcUpdateItemID(slot, id);
        }
        else
        {
            Debug.LogError($"Wrong ItemType: {_itemIDStorage.GetItemType(id)}");
        }
        
    }

    [ClientRpc]
    private void RpcUpdateSpellsID(int slot, int id)
    {
        if (slot >= 0 && slot < _spellsID.Length)
        {
            _spellsID[slot] = id;
            if (id != -1)
            {
                OnSpellChanged?.Invoke(slot, _itemIDStorage.InfoByID(id) as ItemInfoSpellOnly);
            }
            else
            {
                OnSpellChanged?.Invoke(slot, null);
            }
        }
    }

    [Server]
    public bool ServerEraseItemsAndMoney()
    {
        bool erased = false;

        if (_coins > 0)
        {
            _coins = 0;
            erased = true;
        }

        if (_itemsID != null)
        {
            for (int i = 0; i < _itemsID.Length; i++)
            {
                if (_itemsID[i] != NO_ITEM)
                {
                    ChangeItemID(i, NO_ITEM);
                    erased = true;
                }
            }
        }

        if (erased) RpcEraseSound(transform.position);
        return erased;
    }

    [ClientRpc]
    private void RpcEraseSound(Vector3 pos)
    {
        if (_sfxPlayer != null)
            _sfxPlayer.PlayAtPoint(_eraseSound, pos);
    }

    [ClientRpc]
    private void RpcUpdateItemID(int slot, int id)
    {
        if (slot >= 0 && slot < _itemsID.Length)
        {
            _itemsID[slot] = id;
            if (id != -1)
            {
                OnItemChanged?.Invoke(slot, _itemIDStorage.InfoByID(id) as ItemInfoSellable);
            }
            else
            {
                OnItemChanged?.Invoke(slot, null);
            }
        }
    }

    private void MoneyChange(int oldVal, int newVal){
        OnMoneyChange?.Invoke(newVal);
    }
    
   
}
