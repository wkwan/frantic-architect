#if !(UNITY_WINRT || UNITY_WEBPLAYER || UNITY_WEBGL)
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using VoxelBusters.Utility;
using VoxelBusters.ThirdParty.XUPorter;

namespace VoxelBusters.NativePlugins
{
	public class PostProcessBuild
	{
		#region Constants

		// Fabric
		private const string 	kFabricKitJsonStringFormat				= "{{\"Fabric\":{{\"APIKey\":\"{0}\",\"Kits\":[{{\"KitInfo\":{{\"consumerKey\":\"\",\"consumerSecret\":\"\"}},\"KitName\":\"Twitter\"}}]}}}}";
		private const string 	kFabricKitRootKey 						= "Fabric";

		// Path
		private const string 	kInfoPlistFileRelativePath				= "Info.plist";
		private const string 	kInfoPlistBackupFileRelativePath		= "Info.backup.plist";
		private const string 	kPrecompiledFileRelativeDirectoryPath	= "Classes/";
		private const string 	kPrecompiledHeaderExtensionPattern		= "*.pch";
		
		// File modification
		private const string	kPrecompiledHeaderRegexPattern			= @"#ifdef __OBJC__(\n?\t?[#|//](.)*)*";
		private const string	kPrecompiledHeaderEndIfTag				= "#endif";
		private const string	kPrecompiledHeaderInsertText			= "#import \"Defines.h\"";

		// Twitter
		private const string	kRelativePathToTwitterNativeFiles		= "Assets/VoxelBusters/NativePlugins/Plugins/NativeIOSCode/Twitter";
		private	const string	kTwitterConfigKey						= "twitter-config";
		private	const string	kExtenalFolderRelativePath				= "NativePlugins";

		// Plist keys
		private const string	kDeviceCapablitiesKey					= "UIRequiredDeviceCapabilities";

		#endregion

		#region Methods

		[PostProcessBuild(0)]
		public static void OnPostprocessBuild (BuildTarget _target, string _buildPath) 
		{			
#if UNITY_5 || UNITY_6 || UNITY_7
			if (_target == BuildTarget.iOS)
#else
			if (_target == BuildTarget.iPhone)
#endif
			{
#if !NATIVE_PLUGINS_LITE_VERSION
				RemoveOlderVersionFiles();

				// Xcode project modification section
				if (NPSettings.Application.SupportedFeatures.UsesTwitter)
				{
					// Decompress zip files and add it to project
					AddTwitterFilesToProject();
				}
				else
				{
					string 		_twitterXcodeModFile		= GetTwitterXcodeModFilePath();

					if (File.Exists(_twitterXcodeModFile))
						File.Delete(_twitterXcodeModFile);

					// Remove twitter config key
					EditorPrefs.DeleteKey(kTwitterConfigKey);
				}

				// Info plist modification section
				ModifyInfoPlist(_buildPath);
#endif

				// Append code
				AppendCode(_buildPath);
			}
		}

#if !NATIVE_PLUGINS_LITE_VERSION
		private static void AddTwitterFilesToProject ()
		{
			string				_twitterNativeFolder	= AssetsUtility.AssetPathToAbsolutePath(kRelativePathToTwitterNativeFiles);
			string				_twitterConfileFile		= Path.Combine(_twitterNativeFolder, "Config.txt");

			if (!Directory.Exists(_twitterNativeFolder)) 
				return;

			// Re move the files if version has changed
			if (File.Exists(_twitterConfileFile))
			{
				string			_fileVersion			= File.ReadAllText(_twitterConfileFile);

				if (string.Compare(_fileVersion, EditorPrefs.GetString(kTwitterConfigKey, "0")) == 0)
					return;

				EditorPrefs.SetString(kTwitterConfigKey, _fileVersion);
			}
			
			// Start moving files and framework
			string			_projectPath			= AssetsUtility.GetProjectPath();
			string			_twitterExternalFolder	= Path.Combine(_projectPath, kExtenalFolderRelativePath + "/Twitter");
			
			if (Directory.Exists(_twitterExternalFolder))
				Directory.Delete(_twitterExternalFolder, true);

			Directory.CreateDirectory(_twitterExternalFolder);
			
			List<string> 	_twitterFilesList		= new List<string>();
			List<string> 	_twitterFolderList		= new List<string>();

			// ***********************
			// Source code section
			// ***********************
			string			_nativeCodeSourceFolder	= Path.Combine(_twitterNativeFolder, 	"Source");
			string			_nativeCodeDestFolder	= Path.Combine(_twitterExternalFolder, 	"Source");
			
			// Copying folder
			IOExtensions.CopyFilesRecursively(_nativeCodeSourceFolder, _nativeCodeDestFolder);
			
			// Adding source folder to modifier
			_twitterFolderList.Add("Twitter/Source:-fno-objc-arc");
			
			// ***********************
			// Framework Section
			// ***********************
			string[] 		_zippedFrameworkFiles 	= Directory.GetFiles(_twitterNativeFolder, "*.gz", SearchOption.AllDirectories);
			string			_destFrameworkFolder	= Path.Combine(_twitterExternalFolder, "Framework");
			
			if (!Directory.Exists(_destFrameworkFolder))
				Directory.CreateDirectory(_destFrameworkFolder);
			
			// Iterate through each zip files
			foreach (string _curZippedFile in _zippedFrameworkFiles) 
			{
				Zip.DecompressToDirectory(_curZippedFile, _destFrameworkFolder);

				// Adding file to modifier
				_twitterFilesList.Add("Twitter/Framework/" + Path.GetFileNameWithoutExtension(_curZippedFile));
			}

			// ***********************
			// Xcode modifier Section
			// ***********************
			Dictionary<string, object> _xcodeModDict	= new Dictionary<string, object>();
			_xcodeModDict["group"]						= "NativePlugins-Twitter";
			_xcodeModDict["libs"]						= new string[0];
			_xcodeModDict["frameworks"]					= new string[] {
				"Accounts.framework:weak",
				"Social.framework:weak"
			};
			_xcodeModDict["headerpaths"]				= new string[0];
			_xcodeModDict["files"]						= _twitterFilesList;
			_xcodeModDict["folders"]					= _twitterFolderList;
			_xcodeModDict["excludes"]					= new string[] {
				"^.*.meta$",
				"^.*.mdown$",
				"^.*.pdf$",
				"^.*.DS_Store"
			};
			_xcodeModDict["compiler_flags"]				= new string[0];
			_xcodeModDict["linker_flags"]				= new string[0];

			File.WriteAllText(GetTwitterXcodeModFilePath(), _xcodeModDict.ToJSON());
		}

		private static string GetTwitterXcodeModFilePath ()
		{
			string			_projectPath		= AssetsUtility.GetProjectPath();
			string			_externalFolder		= Path.Combine(_projectPath, kExtenalFolderRelativePath);
			string			_twitterModFile		= Path.Combine(_externalFolder, "Twitter.xcodemods");

			return _twitterModFile;
		}

		private static void RemoveOlderVersionFiles ()
		{
			string			_projectPath		= AssetsUtility.GetProjectPath();
			string			_externalFolder		= Path.Combine(_projectPath, kExtenalFolderRelativePath);

			// Remove xcode mod file
			string			_oldModFile			= Path.Combine(_externalFolder, "NPFramework.xcodemods");

			if (File.Exists(_oldModFile))
				File.Delete(_oldModFile);

			// Remove framework folder
			string 			_oldFrameworkFolder	= Path.Combine(_externalFolder, "Framework");

			if (Directory.Exists(_oldFrameworkFolder))
				Directory.Delete(_oldFrameworkFolder, true);
		}


//		{
//			"Fabric": {
//				"APIKey": "{0}",
//				"Kits": [
//				    {
//					"KitInfo": {
//						"consumerKey": "",
//						"consumerSecret": ""
//					},
//					"KitName": "Twitter"
//				    }
//				    ]
//			}
//		}

		private static void ModifyInfoPlist (string _buildPath)
		{
			ApplicationSettings.Features _supportedFeatures	= NPSettings.Application.SupportedFeatures;

			if (!(_supportedFeatures.UsesTwitter || _supportedFeatures.UsesGameServices))
				return;

			string 				_path2InfoPlist				= Path.Combine(_buildPath, kInfoPlistFileRelativePath);
			string 				_path2InfoPlistBackupFile	= Path.Combine(_buildPath, kInfoPlistBackupFileRelativePath);
			Plist 				_infoPlist					= Plist.LoadPlistAtPath(_path2InfoPlist);

			// Create backup
			_infoPlist.Save(_path2InfoPlistBackupFile);

			// Adding twitter settings 
			if (_supportedFeatures.UsesTwitter)
			{
				TwitterSettings 	_twitterSettings			= NPSettings.SocialNetworkSettings.TwitterSettings;
				string 				_fabricJsonStr				= string.Format(kFabricKitJsonStringFormat, _twitterSettings.ConsumerKey);
				IDictionary 		_fabricJsonDictionary		= JSONUtility.FromJSON(_fabricJsonStr) as IDictionary;

				// Add fabric
				_infoPlist.AddValue(kFabricKitRootKey, _fabricJsonDictionary[kFabricKitRootKey] as IDictionary);
			}

			// Adding gamecenter settings
			if (_supportedFeatures.UsesGameServices)
			{
				IList				_deviceCapablitiesList		= _infoPlist.GetKeyPathValue(kDeviceCapablitiesKey) as IList;

				if (_deviceCapablitiesList == null)
					_deviceCapablitiesList						= new List<string>();

				if (!_deviceCapablitiesList.Contains("gamekit"))
					_deviceCapablitiesList.Add("gamekit");

				_infoPlist.AddValue(kDeviceCapablitiesKey, _deviceCapablitiesList);
			}

			// Save
			_infoPlist.Save(_path2InfoPlist);
		}
#endif

		private static void AppendCode (string _buildPath)
		{
			string 		_pchFileDirectory	= Path.Combine(_buildPath, kPrecompiledFileRelativeDirectoryPath);
			string[] 	_pchFiles 			= Directory.GetFiles(_pchFileDirectory, kPrecompiledHeaderExtensionPattern);
			string 		_pchFilePath 		= null;

			// There will be only one file per project if it exists.
			if (_pchFiles.Length > 0)
				_pchFilePath =  _pchFiles[0];

			if (File.Exists(_pchFilePath))
			{
				string 	_fileContents 	= File.ReadAllText(_pchFilePath);

				// Make sure content doesnt exist
				if (_fileContents.Contains(kPrecompiledHeaderInsertText))
					return;

				Regex 	_regex				= new Regex(kPrecompiledHeaderRegexPattern);
				Match 	_match 				= _regex.Match(_fileContents);
				int		_endOfPatternIndex	= _match.Groups[0].Index + _match.Groups[0].Length;

				// We should append text within end tag
				if (_match.Value.Contains(kPrecompiledHeaderEndIfTag))
					_endOfPatternIndex	-= kPrecompiledHeaderEndIfTag.Length;

				string 	_updatedContents	= _fileContents.Insert(_endOfPatternIndex, "\t" + kPrecompiledHeaderInsertText + "\n");

				// Write updated text
				File.WriteAllText(_pchFilePath, _updatedContents);
			}
		}
		
		#endregion
	}
}
#endif