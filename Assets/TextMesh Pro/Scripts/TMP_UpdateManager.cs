using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


namespace TMPro
{

    public class TMP_UpdateManager
    {
        public static TMP_UpdateManager s_Instance;

        private readonly List<TMP_Text> m_LayoutRebuildQueue = new List<TMP_Text>();
        private Dictionary<int, int> m_LayoutQueueLookup = new Dictionary<int, int>();

        private readonly List<TMP_Text> m_GraphicRebuildQueue = new List<TMP_Text>();
        private Dictionary<int, int> m_GraphicQueueLookup = new Dictionary<int, int>();

        //private bool m_PerformingGraphicRebuild;
        //private bool m_PerformingLayoutRebuild;

        /// <summary>
        /// Get a singleton instance of the registry
        /// </summary>
        public static TMP_UpdateManager instance
        {
            get
            {
                if (TMP_UpdateManager.s_Instance == null)
                    TMP_UpdateManager.s_Instance = new TMP_UpdateManager();
                return TMP_UpdateManager.s_Instance;
            }
        }


        /// <summary>
        /// Register to receive callback from the Canvas System.
        /// </summary>
        protected TMP_UpdateManager()
        {
            Camera.onPreRender += new Camera.CameraCallback(this.OnCameraPreRender);
        }


        /// <summary>
        /// Function to register elements which require a layout rebuild.
        /// </summary>
        /// <param name="element"></param>
        public static void RegisterTextElementForLayoutRebuild(TMP_Text element)
        {
            TMP_UpdateManager.instance.InternalRegisterTextElementForLayoutRebuild(element);
        }

        private bool InternalRegisterTextElementForLayoutRebuild(TMP_Text element)
        {
            int id = element.GetInstanceID();

            if (this.m_LayoutQueueLookup.ContainsKey(id))
                return false;

            m_LayoutQueueLookup[id] = id;
            this.m_LayoutRebuildQueue.Add(element);

            return true;
        }


        /// <summary>
        /// Function to register elements which require a layout rebuild.
        /// </summary>
        /// <param name="element"></param>
        public static void RegisterTextElementForGraphicRebuild(TMP_Text element)
        {
            TMP_UpdateManager.instance.InternalRegisterTextElementForGraphicRebuild(element);
        }

        private bool InternalRegisterTextElementForGraphicRebuild(TMP_Text element)
        {
            int id = element.GetInstanceID();

            if (this.m_GraphicQueueLookup.ContainsKey(id))
                return false;

            m_GraphicQueueLookup[id] = id;
            this.m_GraphicRebuildQueue.Add(element);

            return true;
        }


        /// <summary>
        /// Callback which occurs just before the cam is rendered.
        /// </summary>
        /// <param name="cam"></param>
        void OnCameraPreRender(Camera cam)
        {
            // Handle Layout Rebuild Phase
            for (int i = 0; i < m_LayoutRebuildQueue.Count; i++)
            {
                m_LayoutRebuildQueue[i].Rebuild(CanvasUpdate.Prelayout);
            }

            if (m_LayoutRebuildQueue.Count > 0)
            {
                m_LayoutRebuildQueue.Clear();
                m_LayoutQueueLookup.Clear();
            }

            // Handle Graphic Rebuild Phase
            for (int i = 0; i < m_GraphicRebuildQueue.Count; i++)
            {
                m_GraphicRebuildQueue[i].Rebuild(CanvasUpdate.PreRender);
            }

            // If there are no objects in the queue, we don't need to clear the lists again.
            if (m_GraphicRebuildQueue.Count > 0)
            {
                m_GraphicRebuildQueue.Clear();
                m_GraphicQueueLookup.Clear();
            }
        }


        /// <summary>
        /// Function to unregister elements which no longer require a rebuild.
        /// </summary>
        /// <param name="element"></param>
        public static void UnRegisterTextElementForRebuild(TMP_Text element)
        {
            TMP_UpdateManager.instance.InternalUnRegisterTextElementForGraphicRebuild(element);
            TMP_UpdateManager.instance.InternalUnRegisterTextElementForLayoutRebuild(element);
        }

        private void InternalUnRegisterTextElementForGraphicRebuild(TMP_Text element)
        {
            //if (this.m_PerformingGraphicRebuild)
            //{
            //    Debug.LogError((object)string.Format("Trying to remove {0} from rebuild list while we are already inside a rebuild loop. This is not supported.", (object)element));
            //}
            //else
            //{
                int id = element.GetInstanceID();

                //element.LayoutComplete();
                TMP_UpdateManager.instance.m_GraphicRebuildQueue.Remove(element);
                m_GraphicQueueLookup.Remove(id);
            //}
        }

        private void InternalUnRegisterTextElementForLayoutRebuild(TMP_Text element)
        {
            //if (this.m_PerformingLayoutRebuild)
            //{
            //    Debug.LogError((object)string.Format("Trying to remove {0} from rebuild list while we are already inside a rebuild loop. This is not supported.", (object)element));
            //}
            //else
            //{
                int id = element.GetInstanceID();

                //element.LayoutComplete();
                TMP_UpdateManager.instance.m_LayoutRebuildQueue.Remove(element);
                m_LayoutQueueLookup.Remove(id);
            //}
        }


    }
}