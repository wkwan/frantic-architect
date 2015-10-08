using UnityEngine;
using System.Collections;

namespace VoxelBusters.NativePlugins.Internal
{
	public class Constants : MonoBehaviour
	{
		#region Errors

		public const string kDebugTag							= "Native Plugins";
		public const string kErrorMessage						= "Not supported in Editor";
		public const string kiOSFeature							= "iOS feature";

		#endregion

		// Assets path	
		public const string kEditorAssetsPath					= "Assets/VoxelBusters/NativePlugins/EditorResources";
		public const string kLogoPath							= kEditorAssetsPath + "/Logo/NativePlugins.png";
		
		// GUI Style
		public const string kSampleUISkin						= "AssetStoreProductUISkin";//Available in AssetStoreProduct submodule
		public const string kSubTitleStyle  					= "sub-title";

		// Asset store
		public const string	kAssetStorePath						= "http://bit.ly/1Fnpb5j";
		public const string	kPurchaseFullVersionButton			= "Purchase Full Version";
		public const string	kFeatureNotSupportedInLiteVersion	= "Feature not supported in Lite version. Please purchase full version of Native Plugins.";

		//Plugins Details
		public const string kAndroidPluginsLibraryPath			= "Assets/Plugins/Android/native_plugins_lib";
		public const string kAndroidPluginsJARPath				= kAndroidPluginsLibraryPath + "/libs";
		public const string kBillingInterfaceJARName			= "billing_interface";
		public const string kDisabledBillingWarning				= "Billing feature got disabled. You can't refer billing classes in your code until you enable it.";

		
	}
}