using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(Interactable), typeof(NoteText))]
public class Item : BillboardEntity
{
    private ItemIDStorage _itemIDStorage;
    [SerializeField]
    private int _debugItemId = -1;

    [SyncVar(hook = nameof(ChangeID))]
    private int _itemID = -1;
    private ItemInfo _info;
    protected Interactable _interactable;

    public int ID => _itemID;
    public ItemInfo Info => _info;

    private NoteText _note;

    [Inject]
    private void Construct(ItemIDStorage itemIDStorage)
    {
        _itemIDStorage = itemIDStorage;
    }

    [Server]
    public void SetItemID(int id)
    {
        _itemID = id;
    }


    private void ChangeID(int oldID, int newID)
    {
        ProjectContext.Instance.InjectWithMainSceneContext(this);
        if (_itemIDStorage.IsValidID(newID))
        {
            _info = _itemIDStorage.InfoByID(newID);
            _note.TextKey = _info.Description;
            SetTexture(_info.Texture);
        }
    }

    protected override void Awake()
    {
        base.Awake();
        _note = GetComponent<NoteText>();
    }

    protected virtual void TryPickup(PlayerController player)
    {
        player.Inventory.CmdTryPickup(this, 0);
    }

    protected override void Start()
    {
        base.Start();


        
        if (isServer && _debugItemId != -1)
        {
            _itemID =  _debugItemId;
        }
        
        if (_itemID != -1)
        {
            ChangeID(_itemID, _itemID);
        }
        
        _interactable = GetComponent<Interactable>();
        _interactable.OnUse += TryPickup;
    }
}
