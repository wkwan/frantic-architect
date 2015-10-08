//-----------------------------------------------------------------
//  Copyright 2013 Alex McAusland and Ballater Creations
//  All rights reserved
//  www.outlinegames.com
//-----------------------------------------------------------------
using System;
using UnityEngine;

namespace Uniject {
    public interface IResourceLoader {
        System.IO.TextReader openTextFile(string path);
    }
}

