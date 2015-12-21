using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;




namespace TMPro.EditorUtilities
{

    public static class TMPro_CreateStyleAssetMenu
    {

        [MenuItem("Assets/Create/TextMeshPro - Style Sheet", false, 120)]
        public static void CreateTextMeshProObjectPerform()
        {
            // Get the path to the selected texture.
            string filePath = AssetDatabase.GetAssetPath(Selection.activeObject);
            string filePathWithName = AssetDatabase.GenerateUniqueAssetPath(filePath + "/TMP_DefaultStyleSheet.asset");

            // Create new Sprite Asset using this texture
            TMP_StyleSheet styleManager = ScriptableObject.CreateInstance<TMP_StyleSheet>();

            //colorGradient.SetDefaultColors(Color.white);

            AssetDatabase.CreateAsset(styleManager, filePathWithName);

            EditorUtility.SetDirty(styleManager);

            AssetDatabase.SaveAssets();
        }
    }

}