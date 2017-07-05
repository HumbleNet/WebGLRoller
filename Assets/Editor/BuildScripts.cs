using UnityEngine;
using UnityEditor;

using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;


public class BuildScripts : MonoBehaviour 
{

	private static string[] scenes = new string[] { 
		"Assets/_Scenes/MainGame.unity"
	};

	[MenuItem("Tools/Build/WebGL/Debug")]
	protected static void BuildWebGLDebug()
	{
		RunBuild(BuildTarget.WebGL, true);
	}

	[MenuItem("Tools/Build/WebGL/Release")]
	protected static void BuildWebGLRelease()
	{
		RunBuild(BuildTarget.WebGL, false);
	}

	[MenuItem("Tools/Build/Linux/Debug")]
	protected static void BuildLinuxDebug()
	{
		RunBuild(BuildTarget.StandaloneLinuxUniversal, true);
	}

	[MenuItem("Tools/Build/Linux/Release")]
	protected static void BuildLinuxRelease()
	{
		RunBuild(BuildTarget.StandaloneLinuxUniversal, false);
	}

	[MenuItem("Tools/Build/Mac/Debug")]
	protected static void BuildOSXDebug()
	{
		RunBuild(BuildTarget.StandaloneOSXUniversal, true);
	}

	[MenuItem("Tools/Build/Mac/Release")]
	protected static void BuildOSXRelease()
	{
		RunBuild(BuildTarget.StandaloneOSXUniversal, false);
	}

    [MenuItem("Tools/Build/Win/Debug")]
    protected static void BuildWinDebug()
    {
        RunBuild(BuildTarget.StandaloneWindows, true);
    }

    [MenuItem("Tools/Build/Win/Release")]
    protected static void BuildWinRelease()
    {
        RunBuild(BuildTarget.StandaloneWindows, false);
    }

    protected static void RunBuild(BuildTarget target, bool isDebug)
	{
		ClearLog();

		string buildLocation = GetBuildDirectory(target);
		string buildFile = GetBuildFile(target);

		BuildCreateFolder(buildLocation, buildFile, target);

		BuildUnityProject(target, isDebug, buildLocation, buildFile);

	}

	private static void BuildUnityProject(BuildTarget target, bool isDebug, string buildLocation, string buildFile)
	{
		BuildOptions options;
		
		if (isDebug) 
		{
			options = BuildOptions.Development;
			
			//DEVELOPMENT BUILD
			EditorUserBuildSettings.development = true;
			EditorUserBuildSettings.allowDebugging = true;
			
			//soft null ref exceptions
			PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.Full;
		} 
		else //RELEASE
		{
			options = BuildOptions.None;
			
			//RELEASE BUILD
			EditorUserBuildSettings.development = false;
			
			//soft null ref exceptions off
			PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.None;
		}

		// Enable C++11 for emscripten
		PlayerSettings.SetPropertyString("emscriptenArgs","-std=c++11", BuildTargetGroup.WebGL);

		//set some properties for both debug and release 
		PlayerSettings.strippingLevel = StrippingLevel.UseMicroMSCorlib;

		string buildResult = BuildPipeline.BuildPlayer(scenes, Path.Combine(buildLocation, buildFile), target, options);

		if (!string.IsNullOrEmpty(buildResult))
		{
			UnityEngine.Debug.LogError(buildResult);
		}
		else
		{
			UnityEngine.Debug.Log("Build: Unity Build Successful.");
		}
	}

	private static string GetRelativePath(string filespec, string folder)
	{
		Uri pathUri = new Uri(filespec);
		// Folders must end in a slash
		if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
		{
			folder += Path.DirectorySeparatorChar;
		}
		Uri folderUri = new Uri(folder);
		return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString());
	}
	
	private static void GetFilesInTree(string root, ref List<string> result) {
		foreach(var f in Directory.GetFiles(root)) {
			result.Add(f);
		}
		foreach(var d in Directory.GetDirectories(root)) {
			GetFilesInTree(d, ref result);
		}
	}
	
	private static string GetBuildDirectory(BuildTarget target)
	{
		string buildLocation = Path.GetFullPath(Application.dataPath + "/../Builds/");

		string suffix = "";

		switch (target) {
		case BuildTarget.WebGL:
			suffix = "ASMJS/";
			break;
		case BuildTarget.StandaloneLinux:
		case BuildTarget.StandaloneLinux64:
		case BuildTarget.StandaloneLinuxUniversal:
			suffix = "LINUX/";
			break;
		case BuildTarget.StandaloneOSXIntel:
		case BuildTarget.StandaloneOSXIntel64:
		case BuildTarget.StandaloneOSXUniversal:
			suffix = "OSX/";
			break;
		}

		return buildLocation + suffix;
	}
		
	private static string GetBuildFile(BuildTarget target)
	{
		return "WebGLRoller";
	}

	private static void BuildCreateFolder(string buildLocation, string buildFile, BuildTarget target)
	{
		DirectoryInfo dirBuild = new DirectoryInfo(Path.Combine(buildLocation, buildFile));
		
		// if the directory exists, delete it.
		if (dirBuild.Exists)
		{
			UnityEngine.Debug.Log("Build: Cleaning Directory.");
			
			MakeFilesWritable(dirBuild);
			Directory.Delete(buildLocation + buildFile, true);
		}
		
		dirBuild.Create();
		UnityEngine.Debug.Log("Build: Created: " + Path.GetFullPath(Path.Combine(buildLocation, buildFile)));
	}

	private static void MakeFilesWritable(DirectoryInfo directory)
	{
		FileInfo[] files = directory.GetFiles("*", SearchOption.AllDirectories);
		foreach (FileInfo file in files)
		{
			file.IsReadOnly = false;
		}
	}

	public static void ClearLog()
	{
		Assembly assembly = Assembly.GetAssembly(typeof(SceneView));

		Type type = assembly.GetType("UnityEditorInternal.LogEntries");
		MethodInfo method = type.GetMethod("Clear");
		method.Invoke(new object(), null);
	}
}
