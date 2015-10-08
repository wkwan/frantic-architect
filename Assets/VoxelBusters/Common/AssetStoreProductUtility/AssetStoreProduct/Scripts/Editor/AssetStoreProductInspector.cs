using UnityEngine;
using UnityEditor;
using System.Collections;
using VoxelBusters.Utility;

namespace VoxelBusters.AssetStoreProductUtility
{
	using Internal;

	public class AssetStoreProductInspector : Editor
	{
		#region Properties

		private		string				m_copyrightsText;

		// Related to GUI 
		private 	GUIStyle			m_guiStyle;

		#endregion

		#region Unity Methods

		protected virtual void OnEnable ()
		{
			m_copyrightsText	= "<i>" + Constants.kCopyrights + "</i>";
		}

		protected virtual void OnDisable ()
		{}

		public override void OnInspectorGUI ()
		{
			AssetStoreProduct _product	= (target as IAssetStoreProduct).AssetStoreProduct;

			if (_product == null || _product.LogoTexture == null)
				return;
		
			// GUI style
			m_guiStyle			= new GUIStyle("label");
			m_guiStyle.richText	= true;

			GUILayout.BeginHorizontal();
			{
				GUILayout.BeginVertical();
				{
					GUILayout.Space(10f);

					// Logo
					GUILayout.Label(_product.LogoTexture);
				}
				GUILayout.EndVertical();

				// Product details and copyrights
				GUILayout.BeginVertical();
				{
					// Product name
					m_guiStyle.fontSize	= 32;
					GUILayout.Label(_product.ProductName, m_guiStyle, GUILayout.Height(40f));

					// Product version info
					string _pVersion	= "Version " + _product.ProductVersion;
					m_guiStyle.fontSize	= 10;
					GUILayout.Label(_pVersion, m_guiStyle);

					// Copyrights info
					m_guiStyle.fontSize	= 10;
					GUILayout.Label(m_copyrightsText, m_guiStyle);
				}
				GUILayout.EndVertical();

				// To keep above GUI elements left aligned
				GUILayout.FlexibleSpace();
			}
			GUILayout.EndHorizontal();

			// Extra spacing
			GUILayout.Space(10f);
		}

		#endregion
	}
}