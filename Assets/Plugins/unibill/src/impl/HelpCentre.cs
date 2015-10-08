//-----------------------------------------------------------------
//  Copyright 2013 Alex McAusland and Ballater Creations
//  All rights reserved
//  www.outlinegames.com
//-----------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using Uniject;

namespace Unibill.Impl {
    public class HelpCentre {
        private Dictionary<string, object> helpMap;
        public HelpCentre (string json) {
            helpMap = (Dictionary<string, object>)Unibill.Impl.MiniJSON.jsonDecode(json);
        }

        public string getMessage(UnibillError error) {
            string url = string.Format("http://www.outlinegames.com/unibillerrors#{0}", error);
            return string.Format ("{0}.\nSee {1}", helpMap[error.ToString()], url);
        }
    }
}
