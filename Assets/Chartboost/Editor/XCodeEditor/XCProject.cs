using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace UnityEditor.XCodeEditorChartboost
{
	using Debug = UnityEngine.Debug;
	public partial class XCProject : System.IDisposable
	{
		
//		private string _filePath;
		private PBXDictionary _datastore;
		public PBXDictionary _objects;
		private PBXDictionary _configurations;
		
		private PBXGroup _rootGroup;
		private string _defaultConfigurationName;
		private string _rootObjectKey;
	
		public string projectRootPath { get; private set; }
		private FileInfo projectFileInfo;
		
		public string filePath { get; private set; }
		private string sourcePathRoot;
		private bool modified = false;
		
		#region Data
		
		// Objects
		private PBXDictionary<PBXBuildFile> _buildFiles;
		private PBXDictionary<PBXGroup> _groups;
		private PBXDictionary<PBXFileReference> _fileReferences;
		private PBXDictionary<PBXNativeTarget> _nativeTargets;
		
		private PBXDictionary<PBXFrameworksBuildPhase> _frameworkBuildPhases;
		private PBXDictionary<PBXResourcesBuildPhase> _resourcesBuildPhases;
		private PBXDictionary<PBXShellScriptBuildPhase> _shellScriptBuildPhases;
		private PBXDictionary<PBXSourcesBuildPhase> _sourcesBuildPhases;
		private PBXDictionary<PBXCopyFilesBuildPhase> _copyBuildPhases;
				
		private PBXDictionary<XCBuildConfiguration> _buildConfigurations;
		private PBXDictionary<XCConfigurationList> _configurationLists;
		
		private PBXProject _project;
		
		#endregion
		#region Constructor
		
		public XCProject()
		{
			
		}
		
		public XCProject( string filePath ) : this()
		{
			if( !System.IO.Directory.Exists( filePath ) ) {
				Debug.LogWarning( "Path does not exists." );
				return;
			}
			
			if( filePath.EndsWith( ".xcodeproj" ) ) {
				//Debug.Log( "Opening project " + filePath );
				this.projectRootPath = Path.GetDirectoryName( filePath );
				this.filePath = filePath;
			} else {
				//Debug.Log( "Looking for xcodeproj files in " + filePath );
				string[] projects = System.IO.Directory.GetDirectories( filePath, "*.xcodeproj" );
				if( projects.Length == 0 ) {
					Debug.LogWarning( "Error: missing xcodeproj file" );
					return;
				}
				
				this.projectRootPath = filePath;
				this.filePath = projects[ 0 ];	
			}
			
			// Convert to absolute
			this.projectRootPath = Path.GetFullPath(this.projectRootPath);
			
			projectFileInfo = new FileInfo( Path.Combine( this.filePath, "project.pbxproj" ) );
			StreamReader sr = projectFileInfo.OpenText();
			string contents = sr.ReadToEnd();
			sr.Close();
			
			PBXParser parser = new PBXParser();
			_datastore = parser.Decode( contents );
			if( _datastore == null ) {
				throw new System.Exception( "Project file not found at file path " + filePath );
			}

			if( !_datastore.ContainsKey( "objects" ) ) {
				Debug.Log( "Errore " + _datastore.Count );
				return;
			}
			
			_objects = (PBXDictionary)_datastore["objects"];
			modified = false;
			
			_rootObjectKey = (string)_datastore["rootObject"];
			if( !string.IsNullOrEmpty( _rootObjectKey ) ) {
//				_rootObject = (PBXDictionary)_objects[ _rootObjectKey ];
				_project = new PBXProject( _rootObjectKey, (PBXDictionary)_objects[ _rootObjectKey ] );
//				_rootGroup = (PBXDictionary)_objects[ (string)_rootObject[ "mainGroup" ] ];
				_rootGroup = new PBXGroup( _rootObjectKey, (PBXDictionary)_objects[ _project.mainGroupID ] );
			}
			else {
				Debug.LogWarning( "error: project has no root object" );
				_project = null;
				_rootGroup = null;
			}

		}
		
		#endregion
		#region Properties
		
		public PBXProject project {
			get {
				return _project;
			}
		}
		
		public PBXGroup rootGroup {
			get {
				return _rootGroup;
			}
		}
		
		public PBXDictionary<PBXBuildFile> buildFiles {
			get {
				if( _buildFiles == null ) {
					_buildFiles = new PBXDictionary<PBXBuildFile>( _objects );
				}
				return _buildFiles;
			}
		}
		
		public PBXDictionary<PBXGroup> groups {
			get {
				if( _groups == null ) {
					_groups = new PBXDictionary<PBXGroup>( _objects );
				}
				return _groups;
			}
		}
		
		public PBXDictionary<PBXFileReference> fileReferences {
			get {
				if( _fileReferences == null ) {
					_fileReferences = new PBXDictionary<PBXFileReference>( _objects );
				}
				return _fileReferences;
			}
		}
		
		public PBXDictionary<PBXNativeTarget> nativeTargets {
			get {
				if( _nativeTargets == null ) {
					_nativeTargets = new PBXDictionary<PBXNativeTarget>( _objects );
				}
				return _nativeTargets;
			}
		}
		
		public PBXDictionary<XCBuildConfiguration> buildConfigurations {
			get {
				if( _buildConfigurations == null ) {
					_buildConfigurations = new PBXDictionary<XCBuildConfiguration>( _objects );
				}
				return _buildConfigurations;
			}
		}
		
		public PBXDictionary<XCConfigurationList> configurationLists {
			get {
				if( _configurationLists == null ) {
					_configurationLists = new PBXDictionary<XCConfigurationList>( _objects );
				}
				return _configurationLists;
			}
		}
		
		public PBXDictionary<PBXFrameworksBuildPhase> frameworkBuildPhases {
			get {
				if( _frameworkBuildPhases == null ) {
					_frameworkBuildPhases = new PBXDictionary<PBXFrameworksBuildPhase>( _objects );
				}
				return _frameworkBuildPhases;
			}
		}
	
		public PBXDictionary<PBXResourcesBuildPhase> resourcesBuildPhases {
			get {
				if( _resourcesBuildPhases == null ) {
					_resourcesBuildPhases = new PBXDictionary<PBXResourcesBuildPhase>( _objects );
				}
				return _resourcesBuildPhases;
			}
		}
	
		public PBXDictionary<PBXShellScriptBuildPhase> shellScriptBuildPhases {
			get {
				if( _shellScriptBuildPhases == null ) {
					_shellScriptBuildPhases = new PBXDictionary<PBXShellScriptBuildPhase>( _objects );
				}
				return _shellScriptBuildPhases;
			}
		}
	
		public PBXDictionary<PBXSourcesBuildPhase> sourcesBuildPhases {
			get {
				if( _sourcesBuildPhases == null ) {
					_sourcesBuildPhases = new PBXDictionary<PBXSourcesBuildPhase>( _objects );
				}
				return _sourcesBuildPhases;
			}
		}
	
		public PBXDictionary<PBXCopyFilesBuildPhase> copyBuildPhases {
			get {
				if( _copyBuildPhases == null ) {
					_copyBuildPhases = new PBXDictionary<PBXCopyFilesBuildPhase>( _objects );
				}
				return _copyBuildPhases;
			}
		}
								
		
		#endregion
		#region PBXMOD
		
		public bool AddOtherCFlags( string flag )
		{
			return AddOtherCFlags( new PBXList( flag ) ); 
		}
		
		public bool AddOtherCFlags( PBXList flags )
		{
			foreach( KeyValuePair<string, XCBuildConfiguration> buildConfig in buildConfigurations ) {
				buildConfig.Value.AddOtherCFlags( flags );
			}
			modified = true;
			return modified;	
		}

		public bool AddOtherLDFlags( string flag )
		{
			return AddOtherLDFlags( new PBXList( flag ) ); 
		}
		
		public bool AddOtherLDFlags( PBXList flags )
		{
			foreach( KeyValuePair<string, XCBuildConfiguration> buildConfig in buildConfigurations ) {
				buildConfig.Value.AddOtherLDFlags( flags );
			}
			modified = true;
			return modified;	
		}

		public bool GccEnableCppExceptions (string value)
		{
			foreach( KeyValuePair<string, XCBuildConfiguration> buildConfig in buildConfigurations ) {
				buildConfig.Value.GccEnableCppExceptions( value );
			}
			modified = true;
			return modified;	
		}

		public bool GccEnableObjCExceptions (string value)
		{
			foreach( KeyValuePair<string, XCBuildConfiguration> buildConfig in buildConfigurations ) {
				buildConfig.Value.GccEnableObjCExceptions( value );
			}
			modified = true;
			return modified;
		}

		public bool AddHeaderSearchPaths( string path )
		{
			return AddHeaderSearchPaths( new PBXList( path ) );
		}
		
		public bool AddHeaderSearchPaths( PBXList paths )
		{
			foreach( KeyValuePair<string, XCBuildConfiguration> buildConfig in buildConfigurations ) {
//				Debug.Log( "ADDING HEADER PATH: " + paths + " to " + buildConfig.Key );
				buildConfig.Value.AddHeaderSearchPaths( paths );
			}
			modified = true;
			return modified;
		}
		
		public bool AddLibrarySearchPaths( string path )
		{
			return AddLibrarySearchPaths( new PBXList( path ) );
		}
		
		public bool AddLibrarySearchPaths( PBXList paths )
		{
			foreach( KeyValuePair<string, XCBuildConfiguration> buildConfig in buildConfigurations ) {
				buildConfig.Value.AddLibrarySearchPaths( paths );
			}
			modified = true;
			return modified;
		}

		public bool AddFrameworkSearchPaths(string path)
		{
			return AddFrameworkSearchPaths(new PBXList(path));
		}

		public bool AddFrameworkSearchPaths(PBXList paths)
		{
			foreach (KeyValuePair<string, XCBuildConfiguration> buildConfig in buildConfigurations)
			{
				buildConfig.Value.AddFrameworkSearchPaths(paths);
			}
			modified = true;
			return modified;
		}

		//FRAMEWORK_SEARCH_PATHS = (
			//		"$(inherited)",
				//	"\"$(SRCROOT)/../../../../../../../Documents/FacebookSDK\"",
				//);
		
		
//		public PBXList GetObjectOfType( string type )
//		{
//			PBXList result = new PBXList();
//			foreach( KeyValuePair<string, object> current in _objects ) {
//				//Debug.Log( "object: " + ((PBXDictionary)current.Value)["isa"] );
//				if( string.Compare( (string)((PBXDictionary)current.Value)["isa"], type ) == 0 )
//					result.Add( current.Value );
//			}
//			return result;
//		}
		
		public object GetObject( string guid )
		{
			return _objects[guid];
		}
	
		
//		public PBXDictionary<PBXBuildPhase> GetBuildPhase( string buildPhase )
//		{
//			switch( buildPhase ) {
//				case "PBXFrameworksBuildPhase":
//					return (PBXDictionary<PBXBuildPhase>)frameworkBuildPhases;
//				case "PBXResourcesBuildPhase":
//					return (PBXDictionary<PBXBuildPhase>)resourcesBuildPhases;
//				case "PBXShellScriptBuildPhase":
//					return (PBXDictionary<PBXBuildPhase>)shellScriptBuildPhases;
//				case "PBXSourcesBuildPhase":
//					return (PBXDictionary<PBXBuildPhase>)sourcesBuildPhases;
//				case "PBXCopyFilesBuildPhase":
//					return (PBXDictionary<PBXBuildPhase>)copyBuildPhases;
//				default:
//					return default(T);
//			}
//		}
		
		public PBXDictionary AddFile( string filePath, PBXGroup parent = null, string tree = "SOURCE_ROOT", bool createBuildFiles = true, bool weak = false )
		{
			PBXDictionary results = new PBXDictionary();
			string absPath = string.Empty;
			
			if( Path.IsPathRooted( filePath ) ) {
				absPath = filePath;
//				Debug.Log( "Is rooted: " + absPath );
			}
			else if( tree.CompareTo( "SDKROOT" ) != 0) {
				absPath = Path.Combine( Application.dataPath.Replace("Assets", ""), filePath );
//				Debug.Log( "RElative: " + absPath );
			}
			
			if( !( File.Exists( absPath ) || Directory.Exists( absPath ) ) && tree.CompareTo( "SDKROOT" ) != 0 ) {
				Debug.Log( "Missing file: " + absPath + " > " + filePath );
				return results;
			}
			else if( tree.CompareTo( "SOURCE_ROOT" ) == 0 || tree.CompareTo( "GROUP" ) == 0 ) {
				System.Uri fileURI = new System.Uri( absPath );
				System.Uri rootURI = new System.Uri( ( projectRootPath + "/." ) );
				filePath = rootURI.MakeRelativeUri( fileURI ).ToString();
			}
//			else {
//				tree = "<absolute>";
//				Debug.Log( "3: " + filePath );
//			}
//			Debug.Log( "Add file result path: " + filePath );
			
			if( parent == null ) {
				parent = _rootGroup;
			}
			
			// TODO: Aggiungere controllo se file già presente
			PBXFileReference fileReference = GetFile( System.IO.Path.GetFileName( filePath ) );	
			if( fileReference != null ) {
//				Debug.Log( "File già presente." );
				return null;
			}
			
			fileReference = new PBXFileReference( filePath, (TreeEnum)System.Enum.Parse( typeof(TreeEnum), tree ) );
			parent.AddChild( fileReference );
			fileReferences.Add( fileReference );
			results.Add( fileReference.guid, fileReference );

			//Create a build file for reference
			if( !string.IsNullOrEmpty( fileReference.buildPhase ) && createBuildFiles ) {
//				PBXDictionary<PBXBuildPhase> currentPhase = GetBuildPhase( fileReference.buildPhase );
				PBXBuildFile buildFile;
				switch( fileReference.buildPhase ) {
					case "PBXFrameworksBuildPhase":
						foreach( KeyValuePair<string, PBXFrameworksBuildPhase> currentObject in frameworkBuildPhases ) {
							buildFile = new PBXBuildFile( fileReference, weak );
							buildFiles.Add( buildFile );
							currentObject.Value.AddBuildFile( buildFile );
						}

						if ( !string.IsNullOrEmpty( absPath ) && File.Exists(absPath) && tree.CompareTo( "SOURCE_ROOT" ) == 0 ) {
							//Debug.LogError(absPath);
							string libraryPath = Path.Combine( "$(SRCROOT)", Path.GetDirectoryName( filePath ) );
							this.AddLibrarySearchPaths( new PBXList(libraryPath) );
						}
						else if (!string.IsNullOrEmpty( absPath ) && Directory.Exists(absPath) && absPath.EndsWith(".framework") && tree.CompareTo("GROUP") == 0) { // Annt: Add framework search path for FacebookSDK
							string frameworkPath = Path.Combine( "$(SRCROOT)", Path.GetDirectoryName( filePath ) );
							this.AddFrameworkSearchPaths(new PBXList(frameworkPath));
						}
						break;
					case "PBXResourcesBuildPhase":
						foreach( KeyValuePair<string, PBXResourcesBuildPhase> currentObject in resourcesBuildPhases ) {
							buildFile = new PBXBuildFile( fileReference, weak );
							buildFiles.Add( buildFile );
							currentObject.Value.AddBuildFile( buildFile );
						}
						break;
					case "PBXShellScriptBuildPhase":
						foreach( KeyValuePair<string, PBXShellScriptBuildPhase> currentObject in shellScriptBuildPhases ) {
							buildFile = new PBXBuildFile( fileReference, weak );
							buildFiles.Add( buildFile );
							currentObject.Value.AddBuildFile( buildFile );
						}
						break;
					case "PBXSourcesBuildPhase":
						foreach( KeyValuePair<string, PBXSourcesBuildPhase> currentObject in sourcesBuildPhases ) {
							buildFile = new PBXBuildFile( fileReference, weak );
							buildFiles.Add( buildFile );
							currentObject.Value.AddBuildFile( buildFile );
						}
						break;
					case "PBXCopyFilesBuildPhase":
						foreach( KeyValuePair<string, PBXCopyFilesBuildPhase> currentObject in copyBuildPhases ) {
							buildFile = new PBXBuildFile( fileReference, weak );
							buildFiles.Add( buildFile );
							currentObject.Value.AddBuildFile( buildFile );
						}
						break;
					case null:
						Debug.LogWarning( "fase non supportata null" );
						break;
					default:
						Debug.LogWarning( "fase non supportata def" );
						return null;
				}
			}
			
//			Debug.Log( "Results " + results.Count + " - " );
//			foreach( KeyValuePair<string, object> obj in results ){
//				Debug.Log( obj.Key + " - " + obj.Value.GetType().Name );
//			}
			return results;
			
//		def add_file(self, f_path, parent=None, tree='SOURCE_ROOT', create_build_files=True, weak=False):
//        results = []
//
//        abs_path = ''
//
//        if os.path.isabs(f_path):
//            abs_path = f_path
//
//            if not os.path.exists(f_path):
//                return results
//            elif tree == 'SOURCE_ROOT':
//                f_path = os.path.relpath(f_path, self.source_root)
//            else:
//                tree = '<absolute>'
//
//        if not parent:
//            parent = self.root_group
//        elif not isinstance(parent, PBXGroup):
//            # assume it's an id
//            parent = self.objects.get(parent, self.root_group)
//
//        file_ref = PBXFileReference.Create(f_path, tree)
//        parent.add_child(file_ref)
//        results.append(file_ref)
//        # create a build file for the file ref
//        if file_ref.build_phase and create_build_files:
//            phases = self.get_build_phases(file_ref.build_phase)
//
//            for phase in phases:
//                build_file = PBXBuildFile.Create(file_ref, weak=weak)
//
//                phase.add_build_file(build_file)
//                results.append(build_file)
//
//            if abs_path and tree == 'SOURCE_ROOT' and os.path.isfile(abs_path)\
//                and file_ref.build_phase == 'PBXFrameworksBuildPhase':
//
//                library_path = os.path.join('$(SRCROOT)', os.path.split(f_path)[0])
//
//                self.add_library_search_paths([library_path], recursive=False)
//
//        for r in results:
//            self.objects[r.id] = r
//
//        if results:
//            self.modified = True
//
//        return results
		}
		
		public bool AddFolder( string folderPath, PBXGroup parent = null, string[] exclude = null, bool recursive = true, bool createBuildFile = true )
		{
			if( !Directory.Exists( folderPath ) )
				return false;
			DirectoryInfo sourceDirectoryInfo = new DirectoryInfo( folderPath );

			if( exclude == null )
				exclude = new string[] {};
			string regexExclude = string.Format( @"{0}", string.Join( "|", exclude ) );
			
			if( parent == null )
				parent = rootGroup;
			
			// Create group
			PBXGroup newGroup = GetGroup( sourceDirectoryInfo.Name, null /*relative path*/, parent );
//			groups.Add( newGroup );
			
			foreach( string directory in Directory.GetDirectories( folderPath ) )
			{
				if( Regex.IsMatch( directory, regexExclude ) ) {
					continue;
				}

//				special_folders = ['.bundle', '.framework', '.xcodeproj']	
				//Debug.Log( "DIR: " + directory );
				if( directory.EndsWith( ".bundle" ) ) {
					// Treath it like a file and copy even if not recursive
					Debug.LogWarning( "This is a special folder: " + directory );
					AddFile( directory, newGroup, "SOURCE_ROOT", createBuildFile );
					Debug.Log( "fatto" );
					continue;
				}
				
				if( recursive ) {
					Debug.Log( "recursive" );
					AddFolder( directory, newGroup, exclude, recursive, createBuildFile );
				}
			}
			// Adding files.
			foreach( string file in Directory.GetFiles( folderPath ) ) {
				if( Regex.IsMatch( file, regexExclude ) ) {
					continue;
				}
				//Debug.Log( "--> " + file + ", " + newGroup );
				AddFile( file, newGroup, "SOURCE_ROOT", createBuildFile );
			}

			modified = true;
			return modified;
//		def add_folder(self, os_path, parent=None, excludes=None, recursive=True, create_build_files=True):
//        if not os.path.isdir(os_path):
//            return []
//
//        if not excludes:
//            excludes = []
//
//        results = []
//
//        if not parent:
//            parent = self.root_group
//        elif not isinstance(parent, PBXGroup):
//            # assume it's an id
//            parent = self.objects.get(parent, self.root_group)
//
//        path_dict = {os.path.split(os_path)[0]:parent}
//        special_list = []
//
//        for (grp_path, subdirs, files) in os.walk(os_path):
//            parent_folder, folder_name = os.path.split(grp_path)
//            parent = path_dict.get(parent_folder, parent)
//
//            if [sp for sp in special_list if parent_folder.startswith(sp)]:
//                continue
//
//            if folder_name.startswith('.'):
//                special_list.append(grp_path)
//                continue
//
//            if os.path.splitext(grp_path)[1] in XcodeProject.special_folders:
//                # if this file has a special extension (bundle or framework mainly) treat it as a file
//                special_list.append(grp_path)
//
//                new_files = self.verify_files([folder_name], parent=parent)
//
//                if new_files:
//                    results.extend(self.add_file(grp_path, parent, create_build_files=create_build_files))
//
//                continue
//
//            # create group
//            grp = self.get_or_create_group(folder_name, path=self.get_relative_path(grp_path) , parent=parent)
//            path_dict[grp_path] = grp
//
//            results.append(grp)
//
//            file_dict = {}
//
//            for f in files:
//                if f[0] == '.' or [m for m in excludes if re.match(m,f)]:
//                    continue
//
//                kwds = {
//                    'create_build_files': create_build_files,
//                    'parent': grp,
//                    'name': f
//                }
//
//                f_path = os.path.join(grp_path, f)
//
//                file_dict[f_path] = kwds
//
//            new_files = self.verify_files([n.get('name') for n in file_dict.values()], parent=grp)
//
//            add_files = [(k,v) for k,v in file_dict.items() if v.get('name') in new_files]
//
//            for path, kwds in add_files:
//                kwds.pop('name', None)
//
//                self.add_file(path, **kwds)
//
//            if not recursive:
//                break
//
//        for r in results:
//            self.objects[r.id] = r
//
//        return results
		}
		
		#endregion
		#region Getters
		public PBXFileReference GetFile( string name )
		{
			if( string.IsNullOrEmpty( name ) ) {
				return null;
			}
			
			foreach( KeyValuePair<string, PBXFileReference> current in fileReferences ) {
				if( !string.IsNullOrEmpty( current.Value.name ) && current.Value.name.CompareTo( name ) == 0 ) {
					return current.Value;
				}
			}
			
			return null;
		}
		
		
		public PBXGroup GetGroup( string name, string path = null, PBXGroup parent = null )
		{
//			Debug.Log( "GetGroup: " + name + ", " + path + ", " + parent );
			if( string.IsNullOrEmpty( name ) )
				return null;
			
			if( parent == null )
				parent = rootGroup;
			
			foreach( KeyValuePair<string, PBXGroup> current in groups ) {
				
//				Debug.Log( "current: " + current.Value.guid + ", " + current.Value.name + ", " + current.Value.path + " - " + parent.HasChild( current.Key ) );
				if( string.IsNullOrEmpty( current.Value.name ) ) { 
					if( current.Value.path.CompareTo( name ) == 0 && parent.HasChild( current.Key ) ) {
						return current.Value;
					}
				}
				else if( current.Value.name.CompareTo( name ) == 0 && parent.HasChild( current.Key ) ) {
					return current.Value;
				}
			}
			
			PBXGroup result = new PBXGroup( name, path );
			groups.Add( result );
			parent.AddChild( result );
			
			modified = true;
			return result;
			
//		def get_or_create_group(self, name, path=None, parent=None):
//        if not name:
//            return None
//
//        if not parent:
//            parent = self.root_group
//        elif not isinstance(parent, PBXGroup):
//            # assume it's an id
//            parent = self.objects.get(parent, self.root_group)
//
//        groups = self.get_groups_by_name(name)
//
//        for grp in groups:
//            if parent.has_child(grp.id):
//                return grp
//
//        grp = PBXGroup.Create(name, path)
//        parent.add_child(grp)
//
//        self.objects[grp.id] = grp
//
//        self.modified = True
//
//        return grp
		}
			
		#endregion
//		#region Files
//
//
//		/// <summary>
//		/// Returns all file resources in the project, as an array of `XCSourceFile` objects.
//		/// </summary>
//		/// <returns>
//		/// The files.
//		/// </returns>
//		public ArrayList GetFiles()
//		{
//			return null;
//		}
//
//		/// <summary>
//		/// Returns the project file with the specified key, or nil.
//		/// </summary>
//		/// <returns>
//		/// The file with key.
//		/// </returns>
//		/// <param name='key'>
//		/// Key.
//		/// </param>
//		public XCSourceFile GetFileWithKey( string key )
//		{
//			return null;
//		}
//		
//		/// <summary>
//		/// Returns the project file with the specified name, or nil. If more than one project file matches the specified name,
//		/// which one is returned is undefined.
//		/// </summary>
//		/// <returns>
//		/// The file with name.
//		/// </returns>
//		/// <param name='name'>
//		/// Name.
//		/// </param>
//		public XCSourceFile GetFileWithName( string name )
//		{
//			return null;
//		}
//
//		/// <summary>
//		/// Returns all header files in the project, as an array of `XCSourceFile` objects.
//		/// </summary>
//		/// <returns>
//		/// The header files.
//		/// </returns>
//		public ArrayList GetHeaderFiles()
//		{
//			return null;
//		}
//
//		/**
//		* Returns all implementation obj-c implementation files in the project, as an array of `XCSourceFile` objects.
//		*/
//		public ArrayList GetObjectiveCFiles()
//		{
//			return null;
//		}
//
//		/**
//		* Returns all implementation obj-c++ implementation files in the project, as an array of `XCSourceFile` objects.
//		*/
//		public ArrayList GetObjectiveCPlusPlusFiles()
//		{
//			return null;
//		}
//
//		/**
//		* Returns all the xib files in the project, as an array of `XCSourceFile` objects.
//		*/
//		public ArrayList GetXibFiles()
//		{
//			return null;
//			
//		}
//
//		public ArrayList getImagePNGFiles()
//		{
//			return null;
//		}
//		
//		#endregion
//		#region Groups
//		/**
//		* Lists the groups in an xcode project, returning an array of `PBXGroup` objects.
//		*/
//		public PBXList groups {
//			get {
//				return null;
//			}
//		}
//		
//		/**
//		 * Returns the root (top-level) group.
//		 */
//		public PBXGroup rootGroup {
//			get {
//				return null;	
//			}
//		}
//		
//		/**
//		 * Returns the root (top-level) groups, if there are multiple. An array of rootGroup if there is only one.
//		 */
//		public ArrayList rootGroups {
//			get {
//				return null;	
//			}
//		}
//		
//		/**
//		* Returns the group with the given key, or nil.
//		*/
//		public PBXGroup GetGroupWithKey( string key )
//		{
//			return null;
//		}
//		
//		/**
//		 * Returns the group with the specified display name path - the directory relative to the root group. Eg Source/Main
//		 */
//		public PBXGroup GetGroupWithPathFromRoot( string path )
//		{
//			return null;
//		}
//		
//		/**
//		* Returns the parent group for the group or file with the given key;
//		*/
//		public PBXGroup GetGroupForGroupMemberWithKey( string key )
//		{
//			return null;
//		}
//		
//		/**
//		 * Returns the parent group for the group or file with the source file
//		 */
//		public PBXGroup GetGroupWithSourceFile( XCSourceFile sourceFile )
//		{
//			return null;
//		}
//		
//		#endregion
//		#region Target
//		
//		/**
//		* Lists the targets in an xcode project, returning an array of `XCTarget` objects.
//		*/
//		public ArrayList targets {
//			get {
//				return null;
//			}
//		}
//		
//		/**
//		* Returns the target with the specified name, or nil. 
//		*/
//		public XCTarget GetTargetWithName( string name )
//		{
//			return null;
//		}
//		
//		#endregion
//		#region Configurations
//		
//		/**
//		* Returns the target with the specified name, or nil. 
//		*/
//		public Dictionary<string, string> configurations {
//			get {
//				return null;
//			}
//		}
//			
//		public Dictionary<string, string> GetConfigurationWithName( string name )
//		{
//			return null;
//		}
//
//		public XCBuildConfigurationList defaultConfiguration {
//			get {
//				return null;
//			}
//		}
//		
//		#endregion
		#region Mods
		
		public void ApplyMod( string pbxmod )
		{
			XCMod mod = new XCMod( pbxmod );
			ApplyMod( mod );
		}
		
		public void ApplyMod( XCMod mod )
		{	
			PBXGroup modGroup = this.GetGroup( mod.group );
			
			//Debug.Log( "Adding libraries..." );
			//PBXGroup librariesGroup = this.GetGroup( "Libraries" );
			foreach( XCModFile libRef in mod.libs ) {
				string completeLibPath = System.IO.Path.Combine( "usr/lib", libRef.filePath );
				this.AddFile( completeLibPath, modGroup, "SDKROOT", true, libRef.isWeak );
			}
			
			//Debug.Log( "Adding frameworks..." );
			PBXGroup frameworkGroup = this.GetGroup( "Frameworks" );
			foreach( string framework in mod.frameworks ) {
				string[] filename = framework.Split( ':' );
				bool isWeak = ( filename.Length > 1 ) ? true : false;
				string completePath = System.IO.Path.Combine( "System/Library/Frameworks", filename[0] );
				this.AddFile( completePath, frameworkGroup, "SDKROOT", true, isWeak );
			}
			
			//Debug.Log( "Adding files..." );
			foreach( string filePath in mod.files ) {
				string absoluteFilePath = System.IO.Path.Combine( mod.path, filePath );


				if( filePath.EndsWith(".framework") )
					this.AddFile( absoluteFilePath, frameworkGroup, "GROUP", true, false);
				else
					this.AddFile( absoluteFilePath, modGroup );
			}
			
			//Debug.Log( "Adding folders..." );
			foreach( string folderPath in mod.folders ) {
				string absoluteFolderPath = System.IO.Path.Combine( mod.path, folderPath );
				this.AddFolder( absoluteFolderPath, modGroup, (string[])mod.excludes.ToArray( typeof(string) ) );
			}
			
			//Debug.Log( "Adding headerpaths..." );
			foreach( string headerpath in mod.headerpaths ) {
				string absoluteHeaderPath = System.IO.Path.Combine( mod.path, headerpath );
				this.AddHeaderSearchPaths( absoluteHeaderPath );
			}

			//Debug.Log( "Configure build settings..." );
			Hashtable buildSettings = mod.buildSettings;
			if( buildSettings.ContainsKey("OTHER_LDFLAGS") )
			{
				Debug.Log( "    Adding other linker flags..." );
				ArrayList otherLinkerFlags = (ArrayList) buildSettings["OTHER_LDFLAGS"];
				foreach( string linker in otherLinkerFlags ) 
				{
					string _linker = linker;
					if( !_linker.StartsWith("-") )
						_linker = "-" + _linker;
					this.AddOtherLDFlags( _linker );
				}
			}

			if( buildSettings.ContainsKey("GCC_ENABLE_CPP_EXCEPTIONS") )
			{
				Debug.Log( "    GCC_ENABLE_CPP_EXCEPTIONS = " + buildSettings["GCC_ENABLE_CPP_EXCEPTIONS"] );
				this.GccEnableCppExceptions( (string) buildSettings["GCC_ENABLE_CPP_EXCEPTIONS"] );
			}

			if( buildSettings.ContainsKey("GCC_ENABLE_OBJC_EXCEPTIONS") )
			{
				Debug.Log( "    GCC_ENABLE_OBJC_EXCEPTIONS = " + buildSettings["GCC_ENABLE_OBJC_EXCEPTIONS"] );
				this.GccEnableObjCExceptions( (string) buildSettings["GCC_ENABLE_OBJC_EXCEPTIONS"] );
			}

			this.Consolidate();
		}
		
		#endregion
		#region Savings
			
		public void Consolidate()
		{
			PBXDictionary consolidated = new PBXDictionary();
			consolidated.Append<PBXBuildFile>( this.buildFiles );
			consolidated.Append<PBXGroup>( this.groups );
			consolidated.Append<PBXFileReference>( this.fileReferences );
//			consolidated.Append<PBXProject>( this.project );
			consolidated.Append<PBXNativeTarget>( this.nativeTargets );
			consolidated.Append<PBXFrameworksBuildPhase>( this.frameworkBuildPhases );
			consolidated.Append<PBXResourcesBuildPhase>( this.resourcesBuildPhases );
			consolidated.Append<PBXShellScriptBuildPhase>( this.shellScriptBuildPhases );
			consolidated.Append<PBXSourcesBuildPhase>( this.sourcesBuildPhases );
			consolidated.Append<PBXCopyFilesBuildPhase>( this.copyBuildPhases );
			consolidated.Append<XCBuildConfiguration>( this.buildConfigurations );
			consolidated.Append<XCConfigurationList>( this.configurationLists );
			consolidated.Add( project.guid, project.data );
			_objects = consolidated;
			consolidated = null;
		}
		
		
		public void Backup()
		{
			string backupPath = Path.Combine( this.filePath, "project.backup.pbxproj" );
			
			// Delete previous backup file
			if( File.Exists( backupPath ) )
				File.Delete( backupPath );
			
			// Backup original pbxproj file first
			File.Copy( System.IO.Path.Combine( this.filePath, "project.pbxproj" ), backupPath );
		}
		
		/// <summary>
		/// Saves a project after editing.
		/// </summary>
		public void Save()
		{
			PBXDictionary result = new PBXDictionary();
			result.Add( "archiveVersion", 1 );
			result.Add( "classes", new PBXDictionary() );
			result.Add( "objectVersion", 45 );
			
			Consolidate();
			result.Add( "objects", _objects );
			
			result.Add( "rootObject", _rootObjectKey );
			
			Backup();
			
			// Parse result object directly into file
			PBXParser parser = new PBXParser();
			StreamWriter saveFile = File.CreateText( System.IO.Path.Combine( this.filePath, "project.pbxproj" ) );
			saveFile.Write( parser.Encode( result, false ) );
			saveFile.Close();

//			Xcode4Controller.Connect();
//			Xcode4Controller.OpenProject(filePath);
		}
		
		/**
		* Raw project data.
		*/
		public Dictionary<string, object> objects {
			get {
				return null;
			}
		}
		
		
		#endregion
		
		public void Dispose()
		{
			
		}
	}
}
