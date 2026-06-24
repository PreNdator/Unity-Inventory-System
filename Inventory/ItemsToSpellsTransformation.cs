using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class ItemsToSpellsTransformation : MonoBehaviour
{
    private ServerSpawnerItems _spawner;
    private ItemIDStorage _itemIDStorage;

    [SerializeField]
    private Transform _spawnPos;

    [SerializeField]
    private float _spawnPosionRandom = 0.25f;

    [Inject]
    private void Construct(ServerSpawnerItems spawner, ItemIDStorage itemIDStorage)
    {
        _spawner = spawner;
        _itemIDStorage = itemIDStorage;
    }

    [Server]
    public void TrySpawnSpellFromItem(ItemInfo item)
    {
        if (item is ItemInfoSellable sellable) {
            int id = _itemIDStorage.GetSpellID(sellable.Spell);
            if (id != ItemIDStorage.NO_ITEM_ID)
            {
                _spawner.SpawnItem(id, _spawnPos.position + 
                    new Vector3
                    (Random.Range(-_spawnPosionRandom, _spawnPosionRandom),
                    0,
                    Random.Range(-_spawnPosionRandom, _spawnPosionRandom)
                    )
                    
                    );
            }
        }

    }
}
