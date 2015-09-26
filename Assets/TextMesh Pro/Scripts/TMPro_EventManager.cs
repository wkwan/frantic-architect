using UnityEngine;
using System.Collections.Generic;



namespace TMPro
{
    public enum Compute_DistanceTransform_EventTypes { Processing, Completed };


    public static class TMPro_EventManager
    {

        //public delegate void PROGRESS_BAR_EVENT_HANDLER(object Sender, Progress_Bar_EventArgs e);
        //public static event PROGRESS_BAR_EVENT_HANDLER PROGRESS_BAR_EVENT;

        //public delegate void COMPUTE_DT_EVENT_HANDLER(object Sender, Compute_DT_EventArgs e);
        //public static event COMPUTE_DT_EVENT_HANDLER COMPUTE_DT_EVENT;
        public static readonly FastAction<object, Compute_DT_EventArgs> COMPUTE_DT_EVENT = new FastAction<object, Compute_DT_EventArgs>();

        // Event & Delegate used to notify TextMesh Pro objects that Material properties have been changed.
        //public delegate void MaterialProperty_Event_Handler(bool isChanged, Material mat);
        //public static event MaterialProperty_Event_Handler MATERIAL_PROPERTY_EVENT;
        public static readonly FastAction<bool, Material> MATERIAL_PROPERTY_EVENT = new FastAction<bool, Material>();

        //public delegate void FontProperty_Event_Handler(bool isChanged, TextMeshProFont font);
        //public static event FontProperty_Event_Handler FONT_PROPERTY_EVENT;
        public static readonly FastAction<bool, TextMeshProFont> FONT_PROPERTY_EVENT = new FastAction<bool, TextMeshProFont>();

        //public delegate void SpriteAssetProperty_Event_Handler(bool isChanged, Object obj);
        //public static event SpriteAssetProperty_Event_Handler SPRITE_ASSET_PROPERTY_EVENT;
        public static readonly FastAction<bool, Object> SPRITE_ASSET_PROPERTY_EVENT = new FastAction<bool, Object>();

        //public delegate void TextMeshProProperty_Event_Handler(bool isChanged, TextMeshPro obj);
        //public static event TextMeshProProperty_Event_Handler TEXTMESHPRO_PROPERTY_EVENT;
        public static readonly FastAction<bool, TextMeshPro> TEXTMESHPRO_PROPERTY_EVENT = new FastAction<bool, TextMeshPro>();

        //public delegate void DragAndDrop_Event_Handler(GameObject sender, Material currentMaterial, Material newMaterial);
        //public static event DragAndDrop_Event_Handler DRAG_AND_DROP_MATERIAL_EVENT;
        public static readonly FastAction<GameObject, Material, Material> DRAG_AND_DROP_MATERIAL_EVENT = new FastAction<GameObject, Material, Material>();

        //public delegate void TextStyle_Event_Handler(bool isChanged);
        //public static event TextStyle_Event_Handler TEXT_STYLE_PROPERTY_EVENT;
        public static readonly FastAction<bool> TEXT_STYLE_PROPERTY_EVENT = new FastAction<bool>();

#if UNITY_4_6 || UNITY_5
        //public delegate void TextMeshProUGUIProperty_Event_Handler(bool isChanged, TextMeshProUGUI obj);
        //public static event TextMeshProUGUIProperty_Event_Handler TEXTMESHPRO_UGUI_PROPERTY_EVENT;
        public static readonly FastAction<bool, TextMeshProUGUI> TEXTMESHPRO_UGUI_PROPERTY_EVENT = new FastAction<bool, TextMeshProUGUI>();

        //public delegate void BaseMaterial_Event_Handler(Material mat);
        //public static event BaseMaterial_Event_Handler BASE_MATERIAL_EVENT;
        public static readonly FastAction<Material> BASE_MATERIAL_EVENT = new FastAction<Material>();
#endif

        //public delegate void OnPreRenderObject_Event_Handler();
        //public static event OnPreRenderObject_Event_Handler OnPreRenderObject_Event;
        public static readonly FastAction OnPreRenderObject_Event = new FastAction();

        //public delegate void OnTextChanged_Event_Handler(Object obj);
        //public static event OnTextChanged_Event_Handler TEXT_CHANGED_EVENT;
        public static readonly FastAction<Object> TEXT_CHANGED_EVENT = new FastAction<Object>();


        public static readonly FastAction WILL_RENDER_CANVASES = new FastAction();



        static TMPro_EventManager()
        {
            // Register to the willRenderCanvases callback once
            // then the WILL_RENDER_CANVASES FastAction will handle the rest
            Canvas.willRenderCanvases += WILL_RENDER_CANVASES.Call;
        }

        public static void ON_PRE_RENDER_OBJECT_CHANGED()
        {
            OnPreRenderObject_Event.Call();
        }

        public static void ON_MATERIAL_PROPERTY_CHANGED(bool isChanged, Material mat)
        {
            MATERIAL_PROPERTY_EVENT.Call(isChanged, mat);
        }

        public static void ON_FONT_PROPERTY_CHANGED(bool isChanged, TextMeshProFont font)
        {
            FONT_PROPERTY_EVENT.Call(isChanged, font);
        }

        public static void ON_SPRITE_ASSET_PROPERTY_CHANGED(bool isChanged, Object obj)
        {
            SPRITE_ASSET_PROPERTY_EVENT.Call(isChanged, obj);
        }

        public static void ON_TEXTMESHPRO_PROPERTY_CHANGED(bool isChanged, TextMeshPro obj)
        {
            TEXTMESHPRO_PROPERTY_EVENT.Call(isChanged, obj);
        }

        public static void ON_DRAG_AND_DROP_MATERIAL_CHANGED(GameObject sender, Material currentMaterial, Material newMaterial)
        {
            DRAG_AND_DROP_MATERIAL_EVENT.Call(sender, currentMaterial, newMaterial);
        }

        public static void ON_TEXT_STYLE_PROPERTY_CHANGED(bool isChanged)
        {
            TEXT_STYLE_PROPERTY_EVENT.Call(isChanged);
        }

        public static void ON_TEXT_CHANGED(Object obj)
        {
            TEXT_CHANGED_EVENT.Call(obj);

        }

#if UNITY_4_6 || UNITY_5
        public static void ON_TEXTMESHPRO_UGUI_PROPERTY_CHANGED(bool isChanged, TextMeshProUGUI obj)
        {
            TEXTMESHPRO_UGUI_PROPERTY_EVENT.Call(isChanged, obj);
        }
      
        public static void ON_BASE_MATERIAL_CHANGED(Material mat)
        {
            BASE_MATERIAL_EVENT.Call(mat);
        }
#endif


        //public static void ON_PROGRESSBAR_UPDATE(Progress_Bar_EventTypes event_type, Progress_Bar_EventArgs eventArgs)
        //{
        //    if (PROGRESS_BAR_EVENT != null)
        //        PROGRESS_BAR_EVENT(event_type, eventArgs);      
        //}

        public static void ON_COMPUTE_DT_EVENT(object Sender, Compute_DT_EventArgs e)
        {
            COMPUTE_DT_EVENT.Call(Sender, e);
        }
    }


    public class Compute_DT_EventArgs
    {
        public Compute_DistanceTransform_EventTypes EventType;
        public float ProgressPercentage;
        public Color[] Colors;


        public Compute_DT_EventArgs(Compute_DistanceTransform_EventTypes type, float progress)
        {
            EventType = type;
            ProgressPercentage = progress;
        }

        public Compute_DT_EventArgs(Compute_DistanceTransform_EventTypes type, Color[] colors)
        {
            EventType = type;
            Colors = colors;
        }

    }

}