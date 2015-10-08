//-----------------------------------------------------------------
//  Copyright 2013 Alex McAusland and Ballater Creations
//  All rights reserved
//  www.outlinegames.com
//-----------------------------------------------------------------
using System;

namespace Uniject.Editor {
    public interface IEditorUtil {
        string getAssetsDirectoryPath();
        string guidToAssetPath(string guid);
    }
}

