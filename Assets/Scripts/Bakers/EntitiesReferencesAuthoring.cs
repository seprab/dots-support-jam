using Unity.Entities;
using UnityEngine;

class EntitiesReferencesAuthoring : MonoBehaviour
{
    public GameObject playerPrefabGameObject;
    public GameObject bulletPrefabGameObject;
    public GameObject zombiePrefabGameObject;
    public int waveNumber;
    public int waveTimer;
}

class EntitiesReferencesAuthoringBaker : Baker<EntitiesReferencesAuthoring>
{
    public override void Bake(EntitiesReferencesAuthoring authoring)
    {
        var entity  = GetEntity(TransformUsageFlags.None);
        AddComponent(entity, new EntitiesReferences()
        {
            playerPrefabEntity = GetEntity(authoring.playerPrefabGameObject, TransformUsageFlags.Dynamic)
            , bulletPrefabEntity = GetEntity(authoring.bulletPrefabGameObject, TransformUsageFlags.Dynamic)
            , zombiePrefabEntity = GetEntity(authoring.zombiePrefabGameObject, TransformUsageFlags.Dynamic)
            ,waveNumber = authoring.waveNumber
            ,waveTimer = authoring.waveTimer
        } ); 
    }
}

public struct EntitiesReferences : IComponentData
{
    public Entity playerPrefabEntity;
    public Entity bulletPrefabEntity;
    public Entity zombiePrefabEntity;
    public int waveNumber;
    public int waveTimer;
}