// Copyright (C) 2014 - 2015 Stephan Bouchard - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms


using UnityEngine;
using System.Collections.Generic;


namespace TMPro
{

    //[System.Serializable]
    public class TMP_SpriteAsset : TMP_Asset
    {

        // The texture which contains the sprites.
        public Texture spriteSheet;

        // The material used to render these sprites.
        public Material material;

        // List which contains the SpriteInfo for the sprites contained in the sprite sheet.
        public List<TMP_Sprite> spriteInfoList;


        // List which contains the individual sprites.
        private List<Sprite> m_sprites;




        void OnEnable()
        {

        }


        public void AddSprites(string path)
        {

        }


        void OnValidate()
        {
            //Debug.Log("OnValidate called on SpriteAsset.");
            
            //if (updateSprite)
            //{
                //UpdateSpriteArray();
            //    updateSprite = false;
            //}

            TMPro_EventManager.ON_SPRITE_ASSET_PROPERTY_CHANGED(true, this);

        }


#if UNITY_EDITOR
        public void LoadSprites()
        {
            if (m_sprites != null && m_sprites.Count > 0)
                return;

            Debug.Log("Loading Sprite List");
            
            string filePath = UnityEditor.AssetDatabase.GetAssetPath(spriteSheet);

            Object[] objects = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(filePath);

            m_sprites = new List<Sprite>();

            foreach (Object obj in objects)
            {
                if (obj.GetType() == typeof(Sprite))
                {
                    Sprite sprite = obj as Sprite;
                    Debug.Log("Sprite # " + m_sprites.Count + " Rect: " + sprite.rect);
                    m_sprites.Add(sprite);
                }
            }
        }



        public List<Sprite> GetSprites()
        {
            if (m_sprites != null && m_sprites.Count > 0)
                return m_sprites;

            //Debug.Log("Loading Sprite List");

            string filePath = UnityEditor.AssetDatabase.GetAssetPath(spriteSheet);

            Object[] objects = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(filePath);

            m_sprites = new List<Sprite>();

            foreach (Object obj in objects)
            {
                if (obj.GetType() == typeof(Sprite))
                {
                    Sprite sprite = obj as Sprite;
                    //Debug.Log("Sprite # " + m_sprites.Count + " Rect: " + sprite.rect);
                    m_sprites.Add(sprite);
                }
            }

            return m_sprites;
        }
#endif
      
    }
}
