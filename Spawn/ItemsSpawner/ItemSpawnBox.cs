using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class ItemSpawnBox : MonoBehaviour
{
    [SerializeField]
    private Vector3 _firstPoint;
    [SerializeField]
    private Vector3 _lastPoint;

    public Vector3 FirstPoint => _firstPoint;
    public Vector3 LastPoint => _lastPoint;


    private ServerSpawnerItems _spawner;

    [Inject]
    private void Construct(ServerSpawnerItems spawner, ItemSpawnBoxHolder itemSpawnBoxHolder)
    {
        _spawner = spawner;
        itemSpawnBoxHolder.AddBox(this);
    }

    private void Awake()
    {
        ProjectContext.Instance.InjectWithMainSceneContext(this);
    }

    [Server]
    public virtual void SpawnItem(int id)
    {
        Vector3 spawnPosition = GetRandomPosition();
        _spawner.SpawnItem(id, spawnPosition);
    }

    [Server]
    public virtual void SpawnShopItem(int id)
    {
        Vector3 spawnPosition = GetRandomPosition();
        _spawner.SpawnShopItem(id, spawnPosition);
    }

    [Server]
    public virtual void SpawnCoin()
    {
        Vector3 spawnPosition = GetRandomPosition();
        _spawner.SpawnCoin(spawnPosition);
    }

    private Vector3 GetRandomPosition()
    {
        float x = Random.Range(_firstPoint.x, _lastPoint.x);
        float y = Random.Range(_firstPoint.y, _lastPoint.y);
        float z = Random.Range(_firstPoint.z, _lastPoint.z);

        Vector3 localPosition = new Vector3(x, y, z);
        return transform.TransformPoint(localPosition);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Vector3 boxCenter = (_firstPoint + _lastPoint) / 2f;
        Vector3 boxSize = new Vector3(
            Mathf.Abs(_lastPoint.x - _firstPoint.x),
            Mathf.Abs(_lastPoint.y - _firstPoint.y),
            Mathf.Abs(_lastPoint.z - _firstPoint.z)
        );

        Matrix4x4 oldGizmosMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;

        Gizmos.DrawWireCube(boxCenter, boxSize);

        Gizmos.matrix = oldGizmosMatrix;
    }

}