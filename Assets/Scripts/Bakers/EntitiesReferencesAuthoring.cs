using Unity.Entities;
using UnityEngine;

class EntitiesReferencesAuthoring : MonoBehaviour
{
    public GameObject playerPrefabGameObject;
}

class EntitiesReferencesAuthoringBaker : Baker<EntitiesReferencesAuthoring>
{
    public override void Bake(EntitiesReferencesAuthoring authoring)
    {
        var entity  = GetEntity(TransformUsageFlags.None);
        AddComponent(entity, new EntitiesReferences()
        {
            playerPrefabEntity = GetEntity(authoring.playerPrefabGameObject, TransformUsageFlags.Dynamic)
        } ); 
    }
}

public struct EntitiesReferences : IComponentData
{
    public Entity playerPrefabEntity;
}