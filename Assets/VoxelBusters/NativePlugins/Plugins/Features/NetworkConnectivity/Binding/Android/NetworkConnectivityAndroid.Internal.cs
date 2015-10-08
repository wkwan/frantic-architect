using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VoxelBusters.Utility;
using VoxelBusters.DebugPRO;

#if UNITY_ANDROID
namespace VoxelBusters.NativePlugins
{
	using Internal;

	public partial class NetworkConnectivityAndroid : NetworkConnectivity 
	{
		#region Internal Variables

		private bool m_currentConnectionStatus 	= false;
		private bool m_networkHardwareConnectivityStatus = false;
		private bool m_urlReachabilityStatus 	= false;

		#endregion

		#region Internal Callbacks
	
		//This will be triggered from native.
		private void NetworkHardwareStatusChange(string _statusStr)
		{
			bool _status	= bool.Parse(_statusStr);
			m_networkHardwareConnectivityStatus = _status;
			CheckForNetworkReachabilityStatus();
		}

		
		private void URLReachabilityChange(bool _status)
		{
			m_urlReachabilityStatus = _status;
			CheckForNetworkReachabilityStatus();
		}

		#endregion

		#region Notifiers
		
		private void CheckForNetworkReachabilityStatus()
		{
			bool _newConnectionStatus = m_networkHardwareConnectivityStatus && m_urlReachabilityStatus;
			
			if (m_currentConnectionStatus != _newConnectionStatus)
			{
				m_currentConnectionStatus = _newConnectionStatus;
				ConnectivityChanged(_newConnectionStatus);


				if(_newConnectionStatus == false)
				{
					Console.LogWarning(Constants.kDebugTag, "[NetworkConnectivity] networkHardwareConnected ? " + m_networkHardwareConnectivityStatus + " URL Reachable ? " + m_urlReachabilityStatus);
				}
			}
		}
		
		#endregion

		#region Internal Helpers
		
		private IEnumerator MonitorNetworkConnectivity (NetworkConnectivitySettings _settings)
		{
			NetworkConnectivitySettings.AndroidSettings _androidSettings = _settings.Android;

			string _pingAddress		= _settings.IPAddress;
			int _maxRetryCount		= _androidSettings.MaxRetryCount;
			float _dt				= _androidSettings.TimeGapBetweenPolling;
			float _timeOutPeriod	= _androidSettings.TimeOutPeriod;
			bool _connectedToNw		= IsConnected;
			
			while (true)
			{
				bool _nowConnected	= false;
				
				for (int _rIter = 0; _rIter < _maxRetryCount; _rIter++)
				{
					Ping _ping			= new Ping(_pingAddress);
					float  _elapsedTime	= 0f;
					
					// Ping test
					while (!_ping.isDone && _elapsedTime < _timeOutPeriod)
					{
						_elapsedTime	+= Time.deltaTime;
						
						// Wait until next frame
						yield return null;
					}
					
					// Ping request complted within timeout period, so we are connected to network
					if (_ping.isDone && (_ping.time != -1) && _elapsedTime < _timeOutPeriod)
					{
						_nowConnected	= true;
						break;
					}
				}
				
				// Notify Manager about state change
				if (!_connectedToNw)
				{
					if (_nowConnected)
					{
						_connectedToNw	= true;
						URLReachabilityChange(_connectedToNw);
					}
				}
				else
				{
					if (!_nowConnected)
					{
						_connectedToNw	= false;
						URLReachabilityChange(_connectedToNw);
					}
				}
				
				// Wait
				yield return new WaitForSeconds(_dt);
			}
		}

		#endregion
	}
}
#endif