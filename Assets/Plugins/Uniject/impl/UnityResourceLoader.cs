//-----------------------------------------------------------------
//  Copyright 2013 Alex McAusland and Ballater Creations
//  All rights reserved
//  www.outlinegames.com
//-----------------------------------------------------------------
using System;
using UnityEngine;

namespace Uniject.Impl {
    public class UnityResourceLoader : Uniject.IResourceLoader {
        public System.IO.TextReader openTextFile (string path) {
			return new System.IO.StringReader(((TextAsset) Resources.Load(path, typeof(TextAsset))).text);
        }
    }
}
