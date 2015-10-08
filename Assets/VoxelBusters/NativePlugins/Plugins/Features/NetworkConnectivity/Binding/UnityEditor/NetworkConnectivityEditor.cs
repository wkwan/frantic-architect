using UnityEngine;
using System.Collections;
using VoxelBusters.DebugPRO;

#if UNITY_EDITOR
namespace VoxelBusters.NativePlugins
{
	using Internal;

	public partial class NetworkConnectivityEditor : NetworkConnectivity 
	{
		#region API
		
		public override void Initialise ()
		{
			Console.Log(Constants.kDebugTag, "[NetworkConnectivity] Initialised");

			NetworkConnectivitySettings _settings = NPSettings.NetworkConnectivity;
			// Starts scheduler to monitor connectivity
			StopCoroutine("MonitorNetworkConnectivity");
			StartCoroutine(MonitorNetworkConnectivity(_settings));
		}
		
		#endregion

		#region Connectivity Methods
		
		private IEnumerator MonitorNetworkConnectivity (NetworkConnectivitySettings _settings)
		{
			NetworkConnectivitySettings.EditorSettings	_editorSettings	= _settings.Editor;
			string _pingAddress		= _settings.IPAddress;
			int _maxRetryCount		= _editorSettings.MaxRetryCount;
			float _dt				= _editorSettings.TimeGapBetweenPolling;
			float _timeOutPeriod	= _editorSettings.TimeOutPeriod;
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
						ConnectivityChanged(_connectedToNw);
					}
				}
				else
				{
					if (!_nowConnected)
					{
						_connectedToNw	= false;
						ConnectivityChanged(_connectedToNw);
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