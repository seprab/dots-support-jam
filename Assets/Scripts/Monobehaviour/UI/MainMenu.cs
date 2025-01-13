using System;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Button _startServerButton;
    [SerializeField] private UnityEngine.UI.Button _joinServerButton;

    [SerializeField] private UnityEngine.UI.Button _quitButton;

    //[SerializeField] private SceneAsset _entitiesSubscene;
    [SerializeField] private string sceneName = "SampleScene";
    public Text address;
    public Text port;

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

        // TODO check if we need to load the subscene or the scene 
        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);


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
        SceneManager.LoadScene("FrontendHUD");
        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        NetworkEndpoint connectNetworkEndpoint = NetworkEndpoint.Parse(address.text, ParsePortOrDefault(port.text));
        RefRW<NetworkStreamDriver> networkStreamDriver = clientWorld.EntityManager
            .CreateEntityQuery(typeof(NetworkStreamDriver)).GetSingletonRW<NetworkStreamDriver>();
        networkStreamDriver.ValueRW.Connect(clientWorld.EntityManager, connectNetworkEndpoint);
    }

    private void Quit()
    {
        Debug.Log("Quit");
    }

    // Tries to parse a port, returns true if successful, otherwise false
    // The port will be set to whatever is parsed, otherwise the default port of k_NetworkPort
    private UInt16 ParsePortOrDefault(string s)
    {
        if (!UInt16.TryParse(s, out var port))
        {
            Debug.LogWarning($"Unable to parse port, using default port {7979}");
            return 7979;
        }

        return port;
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