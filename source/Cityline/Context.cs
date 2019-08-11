using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace Cityline
{
    public class Context : IContext
    {
        public IPrincipal User { get; set; }
        public Uri RequestUrl { get; set; }
    }
}