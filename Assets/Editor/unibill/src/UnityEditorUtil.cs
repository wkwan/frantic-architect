//-----------------------------------------------------------------
//  Copyright 2013 Alex McAusland and Ballater Creations
//  All rights reserved
//  www.outlinegames.com
//-----------------------------------------------------------------
using System;
using Uniject;
using Uniject.Editor;
using UnityEditor;

namespace Uniject.Impl {
    public class UnityEditorUtil : IEditorUtil {
        public string getAssetsDirectoryPath () {
            return "Assets";
        }

        public string guidToAssetPath(string guid) {
            return AssetDatabase.GUIDToAssetPath(guid);
        }
    }
}

