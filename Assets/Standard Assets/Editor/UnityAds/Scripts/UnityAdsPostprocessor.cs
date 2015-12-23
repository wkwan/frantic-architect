using System;
using System.IO;
using System.Xml;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

namespace UnityEngine.Advertisements
{
  using UnityEngine.Advertisements.XCodeEditor;

  public static class UnityAdsPostprocessor
  {
    [PostProcessBuild(1080)]
    public static void OnPostProcessBuild (BuildTarget target, string path)
    {
#if UNITY_5
      if (target == BuildTarget.iOS) {
#else
      if (target == BuildTarget.iPhone) {
#endif
        PostProcessBuild_iOS (path);
      }
    }

    [PostProcessBuild(-10)]
    public static void OnPostProcessBuildEarly (BuildTarget target, string path)
    {
#if UNITY_5
      if (target == BuildTarget.iOS) {
#else
      if (target == BuildTarget.iPhone) {
#endif
        FixUnityPlistAppendBug (path);
      }
    }

    private static void PostProcessBuild_iOS (string path)
    {
      bool osxEditor = (Application.platform == RuntimePlatform.OSXEditor);
      CreateModFile (path, !osxEditor || !EditorUserBuildSettings.symlinkLibraries);
      ProcessXCodeProject (path);
    }

    private static void ProcessXCodeProject (string path)
    {
      XCProject project = new XCProject (path);

      string modsPath = System.IO.Path.Combine (path, "UnityAds");
      string[] files = System.IO.Directory.GetFiles (modsPath, "*.projmods", System.IO.SearchOption.AllDirectories);

      foreach (string file in files) {
        project.ApplyMod (Application.dataPath, file);
      }

      project.Save ();
    }

    private static void CreateModFile (string path, bool copyDependencies)
    {
      string modPath = System.IO.Path.Combine (path, "UnityAds");

      if (Directory.Exists (modPath)) {
        ClearDirectory (modPath, false);
      } else {
        Directory.CreateDirectory (modPath);
      }

      Dictionary<string, object> mod = new Dictionary<string, object> ();

      List<string> patches = new List<string> ();
      List<string> libs = new List<string> ();
      List<string> librarysearchpaths = new List<string> ();
      List<string> frameworksearchpaths = new List<string> ();

      string pluginsPath = Path.Combine (Application.dataPath, PathWithPlatformDirSeparators ("Plugins/iOS"));

      frameworksearchpaths.Add (copyDependencies ? "$(SRCROOT)/UnityAds" : MacPath (pluginsPath));

      List<string> frameworks = new List<string> ();
      frameworks.Add ("AdSupport.framework");
      frameworks.Add ("StoreKit.framework");
      frameworks.Add ("CoreTelephony.framework");

      List<string> dependencyList = new List<string> ();
      dependencyList.Add ("UnityAds.framework");
      dependencyList.Add ("UnityAds.bundle");
      dependencyList.Add ("UnityAdsUnityWrapper.h");
      dependencyList.Add ("UnityAdsUnityWrapper.mm");

      string dependencyTargetPath = copyDependencies ? modPath : pluginsPath;
      List<string> files = new List<string> ();

      foreach (string dependencyFile in dependencyList) {
        string targetFile = Path.Combine (dependencyTargetPath, dependencyFile);

        if (copyDependencies) {
          try {
            string source = Path.Combine (pluginsPath, dependencyFile);

            if (Directory.Exists (source)) {
              DirectoryCopy (source, targetFile);
            } else if (File.Exists (source)) {
              File.Copy (source, targetFile);
            }
          } catch (Exception e) {
            Debug.Log ("Unable to copy file or directory, " + e);
          }
        }

        files.Add (MacPath (targetFile));
      }


      List<string> headerpaths = new List<string> ();
      headerpaths.Add (copyDependencies ? "$(SRCROOT)/UnityAds" : MacPath (pluginsPath));

      List<string> folders = new List<string> ();
      List<string> excludes = new List<string> ();

      mod.Add ("group", "UnityAds");
      mod.Add ("patches", patches);
      mod.Add ("libs", libs);
      mod.Add ("librarysearchpaths", librarysearchpaths);
      mod.Add ("frameworksearchpaths", frameworksearchpaths);
      mod.Add ("frameworks", frameworks);
      mod.Add ("headerpaths", headerpaths);
      mod.Add ("files", files);
      mod.Add ("folders", folders);
      mod.Add ("excludes", excludes);

      string jsonMod = MiniJSON.Json.Serialize (mod);

      string file = System.IO.Path.Combine (modPath, "UnityAdsXCode.projmods");

      if (!Directory.Exists (modPath)) {
        Directory.CreateDirectory (modPath);
      }
      if (File.Exists (file)) {
        File.Delete (file);
      }

      using (StreamWriter streamWriter = File.CreateText(file)) {
        streamWriter.Write (jsonMod);
      }
    }

    private static void DirectoryCopy (string sourceDirName, string destDirName)
    {
      DirectoryInfo dir = new DirectoryInfo (sourceDirName);
      DirectoryInfo[] dirs = dir.GetDirectories ();

      if (!dir.Exists) {
        return;
      }

      if (!Directory.Exists (destDirName)) {
        Directory.CreateDirectory (destDirName);
      }

      FileInfo[] files = dir.GetFiles ();

      foreach (FileInfo file in files) {
        if(!file.Extension.ToLower().EndsWith("meta")) {
          string temppath = Path.Combine (destDirName, file.Name);
          file.CopyTo (temppath, false);
        }
      }

      foreach (DirectoryInfo subdir in dirs) {
        string temppath = Path.Combine (destDirName, subdir.Name);
        DirectoryCopy (subdir.FullName, temppath);
      }
    }

    public static void ClearDirectory (string path, bool deleteParent)
    {
      if (path != null) {
        string[] folders = Directory.GetDirectories (path);

        foreach (string folder in folders) {
          ClearDirectory (folder, true);
        }

        string[] files = Directory.GetFiles (path);

        foreach (string file in files) {
          File.Delete (file);
        }

        if (deleteParent) {
          Directory.Delete (path);
        }
      }
    }

    public static string PathWithPlatformDirSeparators (string path)
    {
      if (Path.DirectorySeparatorChar == '/') {
        return path.Replace ("\\", Path.DirectorySeparatorChar.ToString ());
      } else if (Path.DirectorySeparatorChar == '\\') {
        return path.Replace ("/", Path.DirectorySeparatorChar.ToString ());
      }

      return path;
    }

    public static string MacPath (string path)
    {
      return path.Replace (@"\", "/");
    }

    // This fixes an Info.plist append bug near UIInterfaceOrientation on some Unity versions (atleast on Unity 4.2.2)
    private static void FixUnityPlistAppendBug (string path)
    {
      try {
        string file = System.IO.Path.Combine (path, "Info.plist");

        if (!File.Exists (file)) {
          return;
        }

        string processedContents = "";
        bool bugFound = false;

        using (StreamReader sr = new StreamReader(file)) {
          bool previousWasEndString = false;
          while (sr.Peek() >= 0) {
            string line = sr.ReadLine ();

            if (previousWasEndString && line.Trim ().StartsWith ("</string>")) {
              bugFound = true;
            } else {
              processedContents += line + "\n";
            }

            previousWasEndString = line.Trim ().EndsWith ("</string>");
          }
        }

        if (bugFound) {
          File.Delete (file);

          using (StreamWriter streamWriter = File.CreateText(file)) {
            streamWriter.Write (processedContents);
          }

          Debug.Log ("UnityAdsPostprocessor found and fixed a known Unity plist append bug in the Info.plist.");
        }
      } catch (Exception e) {
        Debug.Log ("Unable to process plist file: " + e);
      }
    }
  }

}
