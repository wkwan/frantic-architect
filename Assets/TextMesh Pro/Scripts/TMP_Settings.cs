using UnityEngine;
using System.Collections;


namespace TMPro
{
    [System.Serializable]
    public class TMP_Settings : ScriptableObject
    {
        public TextMeshProFont fontAsset;

        public SpriteAsset spriteAsset;

        public TMP_StyleSheet styleSheet;

    }
}
