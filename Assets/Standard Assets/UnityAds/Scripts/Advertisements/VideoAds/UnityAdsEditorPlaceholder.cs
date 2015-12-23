#if UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)

namespace UnityEngine.Advertisements
{
  using UnityEngine;
  using System.Collections;
  using System.IO;

	internal class UnityAdsEditorPlaceholder : MonoBehaviour {
		Texture2D placeHolderLandscape, placeHolderPortrait;
		Texture2D blackTex = null;
		bool showing = false;
		public void init () {
			blackTex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
			blackTex.SetPixel(0, 0, new Color(0.0f, 0.0f, 0.0f, 1f));
			blackTex.Apply();
			placeHolderLandscape = textureFromFile(Application.dataPath + "/Standard Assets/UnityAds/Textures/test_unit_800.png");
      placeHolderPortrait = textureFromFile(Application.dataPath + "/Standard Assets/UnityAds/Textures/test_unit_600.png");
		}

		Texture2D textureFromFile(string filePath) {
			return textureFromBytes(textureBytesForFrame(filePath));
		}
		
		Texture2D textureFromBytes(byte[] bytes) {
			Texture2D texture2D = new Texture2D(1, 1);
			texture2D.LoadImage(bytes);
			return texture2D;
		}
		
	 byte[] textureBytesForFrame(string imageURL) {
			byte[] array = null;
			using(FileStream fileStream = File.OpenRead(imageURL)) {
				int num = (int)fileStream.Length;
				array = new byte[num];
				int num2 = 0;
				int num3;
				while((num3 = fileStream.Read (array, num2, num - num2)) > 0) {
					num2 += num3;
				}
			}
			return array;
		}
		
		void windowFunc (int id) {
			GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), blackTex);
			if (Screen.width > Screen.height) {
				float aspect = (float)placeHolderLandscape.width/(float)placeHolderLandscape.height;
				float finalWidth = Screen.width;
				float finalHeight = (float)finalWidth/(float)aspect;
				Rect currentRect = new Rect(0, (Screen.height-finalHeight)/2, finalWidth, finalHeight);
				GUI.DrawTexture(currentRect, placeHolderLandscape);
			}
			else {
				float aspect = (float)placeHolderPortrait.width/(float)placeHolderPortrait.height;
				float finalWidth = Screen.width;
				float finalHeight = (float)finalWidth/(float)aspect;
				Rect currentRect = new Rect(0, (Screen.height-finalHeight)/2, finalWidth, finalHeight);
				GUI.DrawTexture(currentRect, placeHolderPortrait);
			}
      
      if (GUI.Button(new Rect(Screen.width - 150, 0, 150, 50), @"Close")) {
        Hide();
      }
		}
    
		public void OnGUI () {
			if (!showing) return;
			Color tmpColor = GUI.color;
			GUI.color = new Color(1,1,1,0);
			GUI.ModalWindow(0, new Rect(0, 0, Screen.width, Screen.height), windowFunc, "");
			GUI.color = tmpColor;
		}
		
		public void Show() {
			showing = true;
		}
		
		public void Hide() {
      UnityAds.SharedInstance.onVideoCompleted(@"(null);false");
      UnityAds.SharedInstance.onHide();
			showing = false;
		}		
	}
}

#endif
