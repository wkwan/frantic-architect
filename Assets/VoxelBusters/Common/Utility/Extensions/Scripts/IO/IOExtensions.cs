using UnityEngine;
using System.Collections;
using System;
using System.IO;

namespace VoxelBusters.Utility
{
	public static class IOExtensions 
	{
		public static string MakeRelativePath (this string _fromPath, string _toPath)
		{
			if (string.IsNullOrEmpty(_fromPath)) 
				throw new ArgumentNullException("_fromPath");

			if (string.IsNullOrEmpty(_toPath))   
				throw new ArgumentNullException("_toPath");

			return MakeRelativePath(new Uri(_fromPath), _toPath);
		}

		public static string MakeRelativePath (this Uri _fromUri, string _toPath)
		{
#if !NETFX_CORE
			if (_fromUri == null)
				throw new ArgumentNullException("_fromUri");

			Uri 	_toUri 			= new Uri(_toPath);
			
			// Path can't be made relative.
			if (_fromUri.Scheme != _toUri.Scheme) 
				return _toPath;
			
			Uri 	_relativeUri 	= _fromUri.MakeRelativeUri(_toUri);
			string 	_relativePath 	= Uri.UnescapeDataString(_relativeUri.ToString());
			
			if (_toUri.Scheme.ToUpperInvariant() == "_curFile")
				_relativePath = _relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

			return _relativePath;
#else
			return null;
#endif
		}

		public static void CopyFilesRecursively (string _sourceDirectory, string _destinationDirectory, bool _excludeMetaFiles = true) 
		{
			#if !(UNITY_WEBPLAYER || UNITY_WEBGL || NETFX_CORE)

			// Get the subdirectories for the specified directory.
			DirectoryInfo 		_sourceDirectoryInfo 		= new DirectoryInfo(_sourceDirectory);
			DirectoryInfo[]	 	_subDirectories 			= _sourceDirectoryInfo.GetDirectories();
			
			if (!_sourceDirectoryInfo.Exists)
				throw new DirectoryNotFoundException("Source directory does not exist or could not be found=" + _sourceDirectory);

			// If the destination directory doesn't exist, create it. 
			if (!Directory.Exists(_destinationDirectory))
				Directory.CreateDirectory(_destinationDirectory);

			// Get the files in the directory and copy them to the new location.
			FileInfo[] 			_files = _sourceDirectoryInfo.GetFiles();

			if (_excludeMetaFiles)
			{
				foreach (FileInfo _curFile in _files)
				{
					if (_curFile.Extension == ".meta")
						continue;

					_curFile.CopyTo(Path.Combine(_destinationDirectory, _curFile.Name), true);
				}
			}
			else
			{
				foreach (FileInfo _curFile in _files)
					_curFile.CopyTo(Path.Combine(_destinationDirectory, _curFile.Name), true);
			}
			
			// If copying subdirectories, copy them and their contents to new location. 
			foreach (DirectoryInfo _subDirectory in _subDirectories)
				CopyFilesRecursively(_subDirectory.FullName, Path.Combine(_destinationDirectory, _subDirectory.Name));

			#else

			Debug.LogError("IOExtensions] Copy files not supported on web player");

			#endif
		}
	}
}