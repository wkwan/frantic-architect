using UnityEngine;
using System.Collections;

namespace VoxelBusters.NativePlugins
{
	/// <summary>
	/// Network Connectivity Settings provides interface to configure properties related to network connectivity.
	/// </summary>
	[System.Serializable]
	public class NetworkConnectivitySettings
	{
		#region Editor Settings

		/// <summary>
		/// Network Connectivity Settings specific to Unity Editor.
		/// </summary>
		[System.Serializable]
		public class EditorSettings
		{
			[SerializeField]
			private int 				m_timeOutPeriod = 60;
			/// <summary>
			/// Gets or sets the time out period.
			/// </summary>
			/// <value>The time out period.</value>
			public int 					TimeOutPeriod
			{
				get 
				{ 
					return m_timeOutPeriod; 
				}

				set
				{
					m_timeOutPeriod	= value;
				}
			}
			
			[SerializeField]
			private int 				m_maxRetryCount = 2;
			/// <summary>
			/// Gets or sets the max retry count, whenever polling fails.
			/// </summary>
			/// <value>The max retry count.</value>
			public int 					MaxRetryCount
			{
				get 
				{ 
					return m_maxRetryCount; 
				}
				
				set
				{
					m_maxRetryCount	= value;
				}
			}
			
			[SerializeField]
			private float 				m_timeGapBetweenPolling = 2.0f;
			/// <summary>
			/// Gets or sets the time gap between successive polling.
			/// </summary>
			/// <value>The time gap between polling.</value>
			public float 				TimeGapBetweenPolling
			{
				get 
				{ 
					return m_timeGapBetweenPolling;
				}
				
				set
				{
					m_timeGapBetweenPolling	= value;
				}
			}
		}

		#endregion

		#region Android Settings

		/// <summary>
		/// Network Connectivity Settings specific to Android Editor.
		/// </summary>
		[System.Serializable]
		public class AndroidSettings
		{
			[SerializeField]
			private int 				m_timeOutPeriod = 60;
			/// <summary>
			/// Gets or sets the time out period.
			/// </summary>
			/// <value>The time out period.</value>
			public int 					TimeOutPeriod
			{
				get 
				{ 
					return m_timeOutPeriod; 
				}
				
				set
				{
					m_timeOutPeriod	= value;
				}
			}
			
			[SerializeField]
			private int 				m_maxRetryCount = 2;
			/// <summary>
			/// Gets or sets the max retry count, whenever polling fails.
			/// </summary>
			/// <value>The max retry count.</value>
			public int 					MaxRetryCount
			{
				get 
				{ 
					return m_maxRetryCount; //TODO should allow setters as well
				}
				
				set
				{
					m_maxRetryCount	= value;
				}
			}
			
			[SerializeField]
			private float 				m_timeGapBetweenPolling = 2.0f;
			/// <summary>
			/// Gets or sets the time gap between successive polling.
			/// </summary>
			/// <value>The time gap between polling.</value>
			public float 				TimeGapBetweenPolling
			{
				get 
				{ 
					return m_timeGapBetweenPolling; //TODO should allow setters as well
				}
				
				set
				{
					m_timeGapBetweenPolling	= value;
				}
			}
		}

		#endregion

		#region Properties
		
		[SerializeField]
		private string 					m_ipAddress = "8.8.8.8";
		/// <summary>
		/// Gets or sets the IP Address. IP Address is used to check connectivity.
		/// </summary>
		/// <value>Address to check reachabilty.</value>
		public string 					IPAddress
		{
			get 
			{ 
				return m_ipAddress; 
			}

		 	set 
			{ 
				m_ipAddress = value; 
			}
		}

		[SerializeField]
		private EditorSettings			m_editor;
		/// <summary>
		/// Gets or sets the Network Connectivity Settings specific to Unity Editor.
		/// </summary>
		/// <value>The android.</value>
		public	EditorSettings			Editor
		{
			get 
			{ 
				return m_editor; 
			}
			
			set 
			{ 
				m_editor = value; 
			}
		}

		[SerializeField]
		private AndroidSettings			m_android;
		/// <summary>
		/// Gets or sets the Network Connectivity Settings specific to Android platform.
		/// </summary>
		/// <value>The android.</value>
		public	AndroidSettings			Android
		{
			get 
			{ 
				return m_android; 
			}

		 	set 
			{ 
				m_android = value; 
			}
		}
		
		#endregion

		#region Constructor

		public NetworkConnectivitySettings ()
		{
			Android		= new AndroidSettings();
			Editor		= new EditorSettings();
		}

		#endregion
	}
}