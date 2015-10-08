using UnityEngine;
using UnityEditor;
using System.Collections;

namespace VoxelBusters.NativePlugins.Internal
{
	[InitializeOnLoad]
	public class WelcomeTourWindow : EditorWindow 
	{
		#region Properties

		private int						m_slideIndex				= 0;
		private int 					m_totalSlides				= 5;

		// Background textures
		private Texture2D[]				m_welcomeTourSlides;

		// GUI textures
		private Texture2D				m_leftArrowTexture;
		private Texture2D				m_rightArrowTexture;

		// Related to window
		protected Vector2				m_windowSize				= new Vector2(717f, 538f);

		#endregion

		#region Constants

		private const string			kShowWelcomeTourStatusKey	= "np-welcome-tour";

		#endregion

		#region Static Methods

		static WelcomeTourWindow ()
		{
			bool _alreadyShowedWelcomeTour	= EditorPrefs.GetBool(kShowWelcomeTourStatusKey, false);

			// If we havent shown welcome tour, then lets show it
			if (!_alreadyShowedWelcomeTour)
			{
				ShowWindow();

				// Update status in preference
				EditorPrefs.SetBool(kShowWelcomeTourStatusKey, true);
			}
		}

		public static void ShowWindow ()
		{
			WelcomeTourWindow _window	= EditorWindow.GetWindow<WelcomeTourWindow>(true);

			// Show window
			_window.ShowUtility();
		}

		#endregion

		#region Unity Methods

		private void OnEnable ()
		{
			// Set properties
			#if !(UNITY_5_0) && (UNITY_5 || UNITY_6 || UNITY_7)
			titleContent	= new GUIContent("Welcome Tour");
			#else
			title			= "Welcome Tour";
			#endif
			// Load textures
			m_leftArrowTexture				= AssetDatabase.LoadAssetAtPath(Constants.kEditorAssetsPath + "/WelcomeTour/LeftArrow.png", typeof(Texture2D)) as Texture2D;
			m_rightArrowTexture				= AssetDatabase.LoadAssetAtPath(Constants.kEditorAssetsPath + "/WelcomeTour/RightArrow.png", typeof(Texture2D)) as Texture2D;

			// Load background textures
			m_welcomeTourSlides				= new Texture2D[m_totalSlides];

			for (int _iter = 0; _iter < m_totalSlides; _iter++)
			{
				string _slideFileName		= string.Format("Slide{0}.png", (_iter + 1));
				string _slideFilePath		= Constants.kEditorAssetsPath + "/WelcomeTour/" + _slideFileName;

				// Load texture
				m_welcomeTourSlides[_iter]	= AssetDatabase.LoadAssetAtPath(_slideFilePath, typeof(Texture2D)) as Texture2D;
			}
		}

		private void OnGUI ()
		{
			minSize		 	= m_windowSize;
			maxSize			= minSize;

			// Sets background
			SetWelcomeTourSlide();

			GUILayout.BeginVertical();
			{
				// Flexispace
				GUILayout.FlexibleSpace();

				DrawNavigationButtons();

				// Flexispace
				GUILayout.FlexibleSpace();
			}
			GUILayout.EndVertical();
		}

		#endregion

		#region Draw Methods
		
		private void SetWelcomeTourSlide ()
		{
			Texture2D _slideTexture	= m_welcomeTourSlides[m_slideIndex];
			
			// Set slide background
			if (_slideTexture != null)
				GUI.Label(new Rect(0f, 0f, m_windowSize.x, m_windowSize.y), _slideTexture);
		}

		private void DrawNavigationButtons ()
		{
			// Change background color
			Color _originalBackgroundColor		= GUI.backgroundColor;
			GUI.backgroundColor 				= new Color(1f, 1f, 1f, 0.05f);

			GUILayout.BeginHorizontal();
			{
				// Goto previous slide button
				if (m_slideIndex > 0)
				{
					if (GUILayout.Button(m_leftArrowTexture))
					{
						m_slideIndex--;
					}
				}

				// Flexispace to have buttons cornered
				GUILayout.FlexibleSpace();

				// Goto next slide button
				if (m_slideIndex < (m_totalSlides - 1))
				{
					if (GUILayout.Button(m_rightArrowTexture))
					{
						m_slideIndex++;
					}
				}
			}
			GUILayout.EndHorizontal();
			
			// Reset background color
			GUI.backgroundColor	= _originalBackgroundColor;
		}

		#endregion
	}
}
