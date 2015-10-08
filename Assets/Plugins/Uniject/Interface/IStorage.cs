//-----------------------------------------------------------------
//  Copyright 2013 Alex McAusland and Ballater Creations
//  All rights reserved
//  www.outlinegames.com
//-----------------------------------------------------------------
using System;

namespace Uniject {
    public interface IStorage {
        int GetInt(string key, int defaultValue);
        void SetInt(string key, int value);
		string GetString(string key, string defaultValue);
		void SetString(string key, string val);
    }
}
