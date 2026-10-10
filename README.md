# Introduction 
TODO: Give a short introduction of your project. Let this section explain the objectives or the motivation behind this project. 

# Getting Started
TODO: Guide users through getting your code up and running on their own system. In this section you can talk about:
1.	Installation process
2.	Software dependencies
3.	Latest releases
4.	API references

# Build and Test
TODO: Describe and show how to build your code and run the tests. 

# Contribute
TODO: Explain how other users and developers can contribute to make your code better. 

If you want to learn more about creating good readme files then refer the following [guidelines](https://docs.microsoft.com/en-us/azure/devops/repos/git/create-a-readme?view=azure-devops). You can also seek inspiration from the below readme files:
- [ASP.NET Core](https://github.com/aspnet/Home)
- [Visual Studio Code](https://github.com/Microsoft/vscode)
- [Chakra Core](https://github.com/Microsoft/ChakraCore)


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
- TaskAction task = new TaskAction(3, () => {stuff});   //task.Increment();
- Core.Scene.SpawnObject



