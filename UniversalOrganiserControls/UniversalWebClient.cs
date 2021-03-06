﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace UniversalOrganiserControls
{
    public class UniversalWebClient : WebClient
    {

        protected override WebRequest GetWebRequest(Uri uri)
        {
            HttpWebRequest w = (HttpWebRequest)base.GetWebRequest(uri);
            w.UserAgent = "UniversalOrganiserControls Version/1.0";
            w.ProtocolVersion = Version.Parse("1.0");
            return (WebRequest)w;
        }
    }
}
