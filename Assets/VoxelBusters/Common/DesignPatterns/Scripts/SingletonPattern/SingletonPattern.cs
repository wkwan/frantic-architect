using UnityEngine;
using System;
using System.Collections;

namespace VoxelBusters.DesignPatterns
{
	public class SingletonPattern <T> : MonoBehaviour, ISingleton where T : MonoBehaviour
	{
		#region Static Properties

		private 		static 		bool 			destroyedOnApplicationQuit 	= false;
		private 		static 		object 			instanceLock			 	= new object();

		protected 		static 		T 				instance 					= null;
		/// <summary>
		/// Gets the singleton instance which will be persistent until Application quits.
		/// </summary>
		/// <value>The instance.</value>
		public			static 		T 				Instance
		{
			get 
			{
				System.Type _singletonType	= typeof(T);

				// We are requesting an instance after application is quit
				if (destroyedOnApplicationQuit) 
				{
					Debug.LogWarning("[SingletonPattern] " + _singletonType + " already destroyed ");
					return null;
				}

				lock (instanceLock)
				{
					if (instance == null)
					{
						// Check if there is already gameobject of this component type
						instance 			= FindObjectOfType(_singletonType) as T;
					
						// Check if multiple instances exist
						T []monocomponents 	= FindObjectsOfType(_singletonType) as T[];

						if (monocomponents.Length > 1)
						{
							Debug.LogError("[SingletonPattern] Multiple singleton instances are present in the scene");
							for (int iter = 0; iter < monocomponents.Length; iter++)
							{
								if (instance != monocomponents[iter])
									Destroy(monocomponents[iter].gameObject);
							}
						}

						// We need to create new instance
						if (instance == null)
						{
							// First search in resource if prefab exists for this class
							string _singletonName			= _singletonType.Name;
							GameObject _singletonPrefabGO 	=  Resources.Load("Singleton/" + _singletonName, typeof(GameObject)) as GameObject;

							if (_singletonPrefabGO != null)
							{
								Debug.Log("[SingletonPattern] Creating singeton using prefab");
								instance		= (Instantiate(_singletonPrefabGO) as GameObject).GetComponent<T>();	
							}
							else
							{
								GameObject _go	= new GameObject();
								instance 		= _go.AddComponent<T>();
							}

							// Update name 
							instance.name		= _singletonName;
						}
					}
				}

				return instance;
			}

			protected set
			{
				instance	= value;
			}
		}

		#endregion

		#region Properties
		
		// Components
		private 					Transform		m_transform;
		public 						Transform 		CachedTransform
		{
			get
			{
				if (m_transform == null)
					m_transform	= transform;
				
				return m_transform;
			}
		}
		
		private 					GameObject		m_gameObject;
		public 						GameObject 		CachedGameObject
		{
			get
			{
				if (m_gameObject == null)
					m_gameObject	= gameObject;
				
				return m_gameObject;
			}
		}
		
		protected					bool			IsInitialized
		{
			get;
			private set;
		}
		
		private 					bool			m_isForcefullyDestroyed		= false;
		
		#endregion

		#region Static Methods

		protected static void ResetStaticProperties ()
		{
			instance					= null;
			destroyedOnApplicationQuit	= false;
		}
	
		#endregion

		#region Methods

		protected virtual void Awake ()
		{
			// Just in case, handling so that only one instance is alive
			if (instance == null)
			{
				instance 	= this as T;
			}
			// Destroying the reduntant copy of this class type
			else if (instance != this)
			{
				Destroy(CachedGameObject);
				return;
			}

			// Set as initialized
			IsInitialized	= true;

			// Set it as persistent object
			DontDestroyOnLoad(CachedGameObject);
		}

		protected virtual void Start ()
		{}

		protected virtual void Reset ()
		{
			IsInitialized			= false;
			m_isForcefullyDestroyed	= false;
		}

		protected virtual void OnEnable ()
		{}

		protected virtual void OnDisable ()
		{}

		protected virtual void OnDestroy ()
		{
			// Singleton instance means same instance will run throughout the gameplay session
			// If its destroyed that means application is quit
			if (instance == this && !m_isForcefullyDestroyed)
				destroyedOnApplicationQuit = true;
		}

		#endregion
	
		#region Destroy Methods

		public void ForceDestroy ()
		{			
			// Destroy this gameobject
			m_isForcefullyDestroyed = true;
			Destroy(CachedGameObject);
		}

		#endregion
	}
}
