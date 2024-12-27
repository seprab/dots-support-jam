# dots-support-jam

## Concepto initial
Matar zombies por oleadas dentro de una casa tipo zombies

### Work log:
https://docs.google.com/document/d/1LTSLD87alCVcFJOanwuGD1yCD_U5vAgxALBG9eEagrY/edit?tab=t.0#heading=h.b5v02n6i8lhk

### Data worksheet
https://docs.google.com/spreadsheets/d/12CwXcNLp9tCjs1F_x9SzsIPCeOYH-dtTG89cJrdNyE0/edit?usp=sharing


## Project Structure
```
.
├── InputSystem_Actions.inputactions
├── Prefabs
│   └── Player.prefab
├── SceneDependencyCache
│   └── 2be018ea557a59021f0730b8ec704ad3.sceneWithBuildSettings
├── Scenes
│   ├── SampleScene
│   │   └── SubScene.unity
│   └── SampleScene.unity
├── Scripts
│   ├── Bakers
│   │   ├── EntitiesReferencesAuthoring.cs
│   │   ├── MyValueAuthoring.cs
│   │   ├── NetcodePlayerInputAuthoring.cs
│   │   ├── PlayerBaker.cs
│   │   └── PlayerSpawnerBaker.cs
│   ├── Components
│   │   ├── Direction.cs
│   │   ├── Health.cs
│   │   ├── Player.cs
│   │   ├── PlayerSpawner.cs
│   │   └── Speed.cs
│   ├── Core.asmdef
│   ├── Editor
│   ├── GameBootstrap.cs
│   ├── Monobehaviour
│   ├── RPCs
│   │   └── SimpleRpc.cs
│   ├── Systems
│   │   ├── GoInGameClientSystem.cs
│   │   ├── GoInGameServerSystem.cs
│   │   ├── NetcodePlayerInputSystem.cs
│   │   ├── NetcodePlayerMovementSystem.cs
│   │   ├── PlayerSpawnerSystem.cs
│   │   └── TestReadMyValue.cs
│   ├── TestNetcodeClientSystem.cs
│   └── TestNetcodeServerSystem.cs
└── Settings
    ├── DefaultVolumeProfile.asset
    ├── Mobile_RPAsset.asset
    ├── Mobile_Renderer.asset
    ├── PC_RPAsset.asset
    ├── PC_Renderer.asset
    ├── SampleSceneProfile.asset
    └── UniversalRenderPipelineGlobalSettings.asset
    
```

- Scripts/GameBootstrap: Handles the automatic connection from server and clients. This ensures that everyone has a network ID.
- Scripts/Systems/GoInGameClientSystem: Makes the client connection go in game
- Scripts/Systems/GoInGameServerSystem: Listens to all new client connections and create the corresponding player?
