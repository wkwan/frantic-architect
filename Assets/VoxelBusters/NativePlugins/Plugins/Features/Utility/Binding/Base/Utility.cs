using UnityEngine;
using System.Collections;
using VoxelBusters.DebugPRO;

namespace VoxelBusters.NativePlugins
{
	using Internal;

	/// <summary>
	/// Utility provides an unique way to access some utility functions. 
	/// </summary>
	public class Utility : MonoBehaviour 
	{
		#region Properties

		private RateMyApp		m_rateMyApp;
		public RateMyApp		RateMyApp
		{
			get 
			{
				return	m_rateMyApp;
			}

			private set
			{
				m_rateMyApp	= value;
			}
		}

		#endregion

		#region Unity Methods

		protected virtual void Awake ()
		{
			if (NPSettings.Utility.RateMyApp.IsEnabled)
			{
				m_rateMyApp	= gameObject.AddComponent<RateMyApp>();
			}
		}

		#endregion

		#region API's

		/// <summary>
		/// Gets the UUID.
		/// </summary>
		/// <returns>An unique identifier.</returns>
		public virtual string GetUUID ()
		{
			return System.Guid.NewGuid().ToString();
		}

		/// <summary>
		/// Opens the store link for that platform.
		/// </summary>
		/// <param name="_applicationID">Application identifier which defines the application uniquely.
		///	On iOS this is the id that identifies app on appstore. On Android this is the build identifier (com.example.test)
		///	</param>
		public virtual void OpenStoreLink (string _applicationID)
		{
			Console.LogError(Constants.kDebugTag, Constants.kErrorMessage);
		}

		/// <summary>
		/// Sets the number as the badge of the app icon.
		/// </summary>
		/// <param name="_badgeNumber">The number currently set as badge.</param>
		public virtual void SetApplicationIconBadgeNumber (int _badgeNumber)
		{
			Console.LogError(Constants.kDebugTag, Constants.kErrorMessage);
		}

		/// <summary>
		/// Gets the bundle version.
		/// </summary>
		/// <returns>Application bundle version.</returns>
		public string GetBundleVersion ()
		{
			return VoxelBusters.Utility.PlayerSettings.GetBundleVersion();
		}

		/// <summary>
		/// Gets the bundle identifier.
		/// </summary>
		/// <returns>Application bundle identifier.</returns>
		public string GetBundleIdentifier ()
		{
			return VoxelBusters.Utility.PlayerSettings.GetBundleIdentifier();
		}

		#endregion
	}
}
