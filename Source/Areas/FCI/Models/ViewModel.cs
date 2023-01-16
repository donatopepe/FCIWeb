using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FCIWeb.Areas.FCI.Models
{
    public class ViewModel
    {
        public Guid guid { get; set; }
        public string NomeLista { get; set; }
        public Profili profilo { get; set; }
        public Stati stato { get; set; }
    }
}