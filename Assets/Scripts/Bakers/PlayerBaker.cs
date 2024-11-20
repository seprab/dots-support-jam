 using Unity.Entities;
using UnityEngine;

class PlayerBaker : MonoBehaviour
{
}
class PlayerBakerBaker : Baker<PlayerBaker>
{
    public override void Bake(PlayerBaker authoring)
    {
        var player = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(player, new Player{} ); 
        AddComponent(player, new Direction(){Value = Vector2.zero} );
        AddComponent(player, new Health{Value = 100} );
        AddComponent(player, new Speed{ Value = 1 } );
        
    }
}
