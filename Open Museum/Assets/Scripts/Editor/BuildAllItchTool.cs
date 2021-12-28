using System.Collections.Generic;
using System.IO;
using UnityEditor;

public class BuildAllItchTool
{
    [MenuItem("Tools/Build All (Itch)")]
    public static void BuildGame()
    {

        // Get filename.
        //string path = EditorUtility.SaveFolderPanel("Choose Location of Built Game", "", "");
        string path = "D:/Builds/Museum/";

        if (!EditorUtility.DisplayDialog("Really build?", "Are you sure you want to build?", "Yes", "No wait!!"))
        {
            return;
        }


        UnityEngine.Debug.ClearDeveloperConsole();

        List<string> Levels = new List<string>();

        Levels.Add("Assets/Scenes/Lockpicking.unity");


        // Build player.

        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, "CROSS_PLATFORM_INPUT");

        BuildPipeline.BuildPlayer(Levels.ToArray(), path + "Museum of Mechanics - Windows/Museum of Mechanics.exe", BuildTarget.StandaloneWindows64, BuildOptions.None);
        BuildPipeline.BuildPlayer(Levels.ToArray(), path + "Museum of Mechanics - Linux/Museum_Of_Mechanics.x86_64", BuildTarget.StandaloneLinux64, BuildOptions.None);
        BuildPipeline.BuildPlayer(Levels.ToArray(), path + "Museum of Mechanics - OSX/Museum of Mechanics - OSX.app", BuildTarget.StandaloneOSX, BuildOptions.None);

    }

}
