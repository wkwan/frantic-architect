// Copyright (C) 2014 - 2015 Stephan Bouchard - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms


using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TMPro
{

    public static class TMPro_ExtensionMethods
    {

        public static string ArrayToString(this char[] chars)
        {
            string s = string.Empty;

            for (int i = 0; i < chars.Length && chars[i] != 0; i++)
            {
                s += chars[i];
            }

            return s;
        }


        public static int FindInstanceID <T> (this List<T> list, T target) where T : Object
        {
            int targetID = target.GetInstanceID();
            
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].GetInstanceID() == targetID)
                    return i;
            }
            return -1;
        }


        public static bool Compare(this Color32 a, Color32 b)
        {
            return a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
        }

		public static bool CompareRGB(this Color32 a, Color32 b)
		{
			return a.r == b.r && a.g == b.g && a.b == b.b;
		}

		public static bool Compare(this Color a, Color b)
        {
            return a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
        }


		public static bool CompareRGB(this Color a, Color b)
		{
			return a.r == b.r && a.g == b.g && a.b == b.b;
		}


        public static Color32 Tint (this Color32 c1, Color32 c2)
        {
            byte r = (byte)((c1.r / 255f) * (c2.r / 255f) * 255);
            byte g = (byte)((c1.g / 255f) * (c2.g / 255f) * 255);
            byte b = (byte)((c1.b / 255f) * (c2.b / 255f) * 255);
            byte a = (byte)((c1.a / 255f) * (c2.a / 255f) * 255);

            return new Color32(r, g, b, a);
        }

        public static Color32 Tint(this Color32 c1, float tint)
        {
            byte r = (byte)(Mathf.Clamp(c1.r / 255f * tint * 255, 0, 255));
            byte g = (byte)(Mathf.Clamp(c1.g / 255f * tint * 255, 0, 255));
            byte b = (byte)(Mathf.Clamp(c1.b / 255f * tint * 255, 0, 255));
            byte a = (byte)(Mathf.Clamp(c1.a / 255f * tint * 255, 0, 255));

            return new Color32(r, g, b, a);
        }


        public static bool Compare(this Vector3 v1, Vector3 v2, int accuracy)
        {
            bool x = (int)(v1.x * accuracy) == (int)(v2.x * accuracy);
            bool y = (int)(v1.y * accuracy) == (int)(v2.y * accuracy);
            bool z = (int)(v1.z * accuracy) == (int)(v2.z * accuracy);

            return x && y && z;
        }

        public static bool Compare(this Quaternion q1, Quaternion q2, int accuracy)
        {
            bool x = (int)(q1.x * accuracy) == (int)(q2.x * accuracy);
            bool y = (int)(q1.y * accuracy) == (int)(q2.y * accuracy);
            bool z = (int)(q1.z * accuracy) == (int)(q2.z * accuracy);
            bool w = (int)(q1.w * accuracy) == (int)(q2.w * accuracy);

            return x && y && z && w;
        }
    }

    public static class TMP_Math
    {
        public static bool Equals(float a, float b)
        {
            return (b - 0.000001f) < a && a < (b + 0.000001f);
        }
    }

}
