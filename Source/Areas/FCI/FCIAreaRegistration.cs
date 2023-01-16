using System.Web.Mvc;

namespace FCIWeb.Areas.FCI
{
    public class FCIAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "FCI";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {

            //context.MapRoute(
            //    "FCI_default2",
            //    "FCI/{controller}/{action}#{id}",
            //    new { controller = "Richieste", action = "Index", id = UrlParameter.Optional },
            //       namespaces: new[] { "FCIWeb.Areas.FCI.Controllers" }
            //);

            context.MapRoute(
                "FCI_default",
                "FCI/{controller}/{action}/{id}",
                new { controller = "Richiesta", action = "GestioneRichieste", id = UrlParameter.Optional },
                namespaces: new[] { "FCIWeb.Areas.FCI.Controllers" }
            );

        }
    }
}
