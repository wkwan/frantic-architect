using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Linq;
using System.IO;
using System.Diagnostics;

public class VunglePostBuilder : MonoBehaviour
{
	private static string _postBuildDirectoryKey { get { return "VunglePostBuildPath-" + PlayerSettings.productName; } }
	private static string postBuildDirectory
	{
		get
		{
			return EditorPrefs.GetString( _postBuildDirectoryKey );
		}
		set
		{
			EditorPrefs.SetString( _postBuildDirectoryKey, value );
		}
	}


	[PostProcessBuild( 800 )]
	private static void onPostProcessBuildPlayer( BuildTarget target, string pathToBuiltProject )
	{
		#if UNITY5_SCRIPTING_IN_UNITY4
			if( target == BuildTarget.iPhone )
		#else
			if( target == BuildTarget.iOS )
		#endif
		{
			postBuildDirectory = pathToBuiltProject;

			// grab the path to the postProcessor.py file
			var scriptPath = Path.Combine( Application.dataPath, "Editor/Vungle/VunglePostProcessor.py" );

			// sanity check
			if( !File.Exists( scriptPath ) )
			{
				UnityEngine.Debug.LogError( "Vungle post builder could not find the VunglePostProcessor.py file. Did you accidentally delete it?" );
				return;
			}

			var pathToNativeCodeFiles = Path.Combine( Application.dataPath, "Editor/Vungle/VungleSDK" );

			var args = string.Format( "\"{0}\" \"{1}\" \"{2}\"", scriptPath, pathToBuiltProject, pathToNativeCodeFiles );
			var proc = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = "python2.6",
					Arguments = args,
					UseShellExecute = false,
					RedirectStandardOutput = true,
					CreateNoWindow = true
				}
			};

			proc.Start();
			proc.WaitForExit();

			UnityEngine.Debug.Log( "Vungle post processor completed" );
		}
	}


	[UnityEditor.MenuItem( "Tools/Vungle/Open Documentation Website..." )]
	static void documentationSite()
	{
		UnityEditor.Help.BrowseURL( "https://github.com/Vungle/vungle-resources/tree/master/English/Unity" );
	}


	[UnityEditor.MenuItem( "Tools/Vungle/Run iOS Post Processor" )]
	static void runPostBuilder()
	{
		onPostProcessBuildPlayer( 		
#if UNITY5_SCRIPTING_IN_UNITY4
	BuildTarget.iPhone
#else
	BuildTarget.iOS
#endif
		                         , postBuildDirectory );
	}

	[UnityEditor.MenuItem( "Tools/Vungle/Run iOS Post Processor", true )]
	static bool validateRunPostBuilder()
	{
		var iPhoneProjectPath = postBuildDirectory;
		if( iPhoneProjectPath == null || !Directory.Exists( iPhoneProjectPath ) )
			return false;

		var projectFile = Path.Combine( iPhoneProjectPath, "Unity-iPhone.xcodeproj/project.pbxproj" );
		if( !File.Exists( projectFile ) )
			return false;

		return true;
	}
}
