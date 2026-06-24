using UnityEngine;
using Zenject;

public class ItemsInstaller : MonoInstaller
{
    [SerializeField]
    private ItemIDStorage _itemIDStoragePrefab;
    [SerializeField]
    private ItemsToSpellsTransformation _spellTransformation;

    public override void InstallBindings()
    {
        Container.Bind<ItemIDStorage>().FromComponentInNewPrefab(_itemIDStoragePrefab).AsSingle();
        Container.Bind<ItemsToSpellsTransformation>().FromInstance(_spellTransformation).AsSingle();
    }
}