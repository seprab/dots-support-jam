using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    [SerializeField] private  UnityEngine.UI.Button _startServerButton;
    [SerializeField] private  UnityEngine.UI.Button _joinServerButton;
    [SerializeField] private  UnityEngine.UI.Button _quitButton;
    [SerializeField] private SceneAsset _entitiesSubscene;
    
    [SerializeField] private SceneAsset sceneName;
    
    private void Start()
    {
        _startServerButton.onClick.AddListener(StartServer);
        _joinServerButton.onClick.AddListener(JoinServer);
        //_quitButton.onClick.AddListener(Quit);
    }
    private void StartServer()
    {
        Debug.Log("Start Server");
        World serverWorld = ClientServerBootstrap.CreateServerWorld("ServerWorld");
        World clientWorld = ClientServerBootstrap.CreateClientWorld("ClientWorld");

        foreach (World world in World.All)
        {
            if (world.Flags == WorldFlags.Game)
            {
                world.Dispose();
                break;
            }
        }

        if (World.DefaultGameObjectInjectionWorld == null)
        {
            World.DefaultGameObjectInjectionWorld = serverWorld;
        }
        //SceneManager.LoadSceneAsync(_entitiesSubscene.name, LoadSceneMode.Additive);
        
        SceneManager.LoadScene("FrontendHUD");

        //Destroy the local simulation world to avoid the game scene to be loaded into it
        //This prevent rendering (rendering from multiple world with presentation is not greatly supported)
        //and other issues.
        DestroyLocalSimulationWorld();
        
        //TODO  revisar esto que es copiado del ejemplo de netcode 
        if (World.DefaultGameObjectInjectionWorld == null)
            World.DefaultGameObjectInjectionWorld = serverWorld;
        
        SceneManager.LoadSceneAsync(sceneName.name, LoadSceneMode.Additive);
        
        
        ushort port = 7979;
        RefRW<NetworkStreamDriver> networkStreamDriver = serverWorld.EntityManager
            .CreateEntityQuery(typeof(NetworkStreamDriver)).GetSingletonRW<NetworkStreamDriver>();
        networkStreamDriver.ValueRW.Listen(NetworkEndpoint.AnyIpv4.WithPort(port));
        
        NetworkEndpoint connectNetworkEndpoint = NetworkEndpoint.LoopbackIpv4.WithPort(port);
        networkStreamDriver = clientWorld.EntityManager
            .CreateEntityQuery(typeof(NetworkStreamDriver)).GetSingletonRW<NetworkStreamDriver>();
        networkStreamDriver.ValueRW.Connect(clientWorld.EntityManager, connectNetworkEndpoint);

    }
    private void JoinServer()
    {
        Debug.Log("Join Server");
        World clientWorld = ClientServerBootstrap.CreateClientWorld("ClientWorld");

        foreach (World world in World.All)
        {
            if (world.Flags == WorldFlags.Game)
            {
                world.Dispose();
                break;
            }
        }

        if (World.DefaultGameObjectInjectionWorld == null)
        {
            World.DefaultGameObjectInjectionWorld = clientWorld;
        }
        SceneManager.LoadSceneAsync(_entitiesSubscene.name, LoadSceneMode.Additive);

        ushort port = 7979;
        string ip = "127.0.0.1";
        
        NetworkEndpoint connectNetworkEndpoint = NetworkEndpoint.Parse(ip, port);
        RefRW<NetworkStreamDriver> networkStreamDriver = clientWorld.EntityManager
            .CreateEntityQuery(typeof(NetworkStreamDriver)).GetSingletonRW<NetworkStreamDriver>();
        networkStreamDriver.ValueRW.Connect(clientWorld.EntityManager, connectNetworkEndpoint);
    }
    private void Quit()
    {
        Debug.Log("Quit");
    }
    
    protected void DestroyLocalSimulationWorld()
    {
        foreach (var world in World.All)
        {
            if (world.Flags == WorldFlags.Game)
            {
                //OldFrontendWorldName = world.Name;
                world.Dispose();
                break;
            }
        }
    }
}
