#open 
- open with version 2022.3.62f3

#Setup
- main is is called   Logic

#Access point
Setup
- Assets
-   SystemCore
-   SystemCoreVR


#Access point
The Scene  object file you want has to be tagged like   [MonitorTrainer]_BandManager    [MonitorTrainer]_MonitorTrainerRoot   [MonitorTrainer]_PhysicalConsole

This is  placed into EnvironmentManager.cs

Turn on / of  VR   #if VR_INTERACTION
- UNITY EDITOR
- Open Edit → Project Settings.
- Select Player.
- Expand Other Settings.
- Find Scripting Define Symbols.
- Add:
- VR_INTERACTION

#Weird errors on loading
- add package by name       com.meta.xr.sdk.core


#Load Stuff at runtime
- HouseBuilderRoot.cs 293
- Core.Environment.OnEnvironmentLoadingComplete += OnEnvironmentLoadingComplete;
- Core.SceneLoader   Core.PhotonMultiplayerRef  ect  
- this.WaitUntil(1, () => (m_InitialisationsToWaitForTotal == m_InitialisationsToWaitForSoFar), () =>  // wait 1 frmae, check the next part, then repeat , until next part is true
- this.WaitFor(time, () =>


#helpers
- private PoolManagerLocalComponent<ImageMouseMoveLimited> m_CirlcePool;
- TaskAction task = new TaskAction(3, () => {stuff});   //task.Increment();
- Core.Scene.SpawnObject
- tableRoot = this.gameObject.SearchComponent<Transform>("Table");


