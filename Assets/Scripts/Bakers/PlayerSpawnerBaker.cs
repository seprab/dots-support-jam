using Unity.Entities;
using UnityEngine;

class PlayerSpawnerBaker : MonoBehaviour
{
    public GameObject PlayerPrefab;
    
}

class PlayerSpawnerBakerBaker : Baker<PlayerSpawnerBaker>
{
    public override void Bake(PlayerSpawnerBaker authoring)
    {
        var spawner = GetEntity(TransformUsageFlags.None);
        var player  = GetEntity(authoring.PlayerPrefab, TransformUsageFlags.Dynamic);
        // store the reference to the entity in the Spawner component
        AddComponent(spawner, new PlayerSpawner()
        {
            playerPrefab = player
        } ); 
    }
}
