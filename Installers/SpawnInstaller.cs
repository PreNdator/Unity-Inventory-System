using UnityEngine;
using Zenject;

public class SpawnInstaller : MonoInstaller
{
    [SerializeField]
    private ServerSpawnerItems _spawnerItemsInstance;
    [SerializeField]
    private ServerSpawnerMonsters _spawnerMonstersInstance;
    public override void InstallBindings()
    {
        Container.Bind<ServerSpawnerItems>().FromInstance(_spawnerItemsInstance).AsSingle();
        Container.Bind<MonstersSpawnPointHolder>().FromNew().AsSingle();
        Container.Bind<ServerSpawnerMonsters>().FromInstance(_spawnerMonstersInstance).AsSingle();
        Container.Bind<ItemSpawnBoxHolder>().FromNew().AsSingle();
    }
}