using AreaCommonWeb.Business;
using AreaCommonWeb.DAL.Profiles;
using AreaCommonWeb.ModelView.Security;
using DAL.AreaCommonWeb;
using FCIWeb.Areas.FCI.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FCIWeb.Areas.FCI.Controllers
{
    public partial class RichiestaController : Controller
    {
        private FCIRepository repository = new FCIRepository();
        private ResourcesRepository resourcesRepository = new ResourcesRepository();
        private CommonBusiness commonBusiness = new CommonBusiness();
        private ProfilesRepository profilesRepository = new ProfilesRepository();


        private int MioIstituto;
        private string MioUocode;
        private string MiaMatricola;
        private bool isFiliale;
        private string codRuolo;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // db.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}
