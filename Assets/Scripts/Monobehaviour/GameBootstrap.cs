using Unity.NetCode;
using UnityEngine;

[UnityEngine.Scripting.Preserve]
public class GameBootstrap : ClientServerBootstrap
{
    public override bool Initialize(string defaultWorldName)
    {
        return false; // Disabling automatic connection to enable it through the UI
        AutoConnectPort = 7979;
        return base.Initialize(defaultWorldName);
    }
}
