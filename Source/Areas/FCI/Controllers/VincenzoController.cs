using FCIWeb.Areas.FCI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FCIWeb.Areas.FCI.DAL;
using System.Data.Entity;

using System.Data;
using AreaCommonWeb.DAL;
using Kendo.Mvc.Extensions;

namespace FCIWeb.Areas.FCI.Models
{
    //public class CitiesViewModel
    //{
    //    public IEnumerable<string> SelectedCities { get; set; }
    //    public IEnumerable<SelectListItem> Cities { get; set; }
    //}

    
    public class uocodeViewModel : Fogli
    {
        public IEnumerable<string> valoreselzionato { get; set; }
        public IEnumerable<SelectListItem> UOCodelist { get; set; }
    }
    
}


namespace FCIWeb.Areas.FCI.Controllers
{
    public partial class RichiestaController : Controller
    {



        //public ActionResult AutorizzaUOCode(Guid? id)
        //{
        //    if (id != null)
        //    {
        //        if (Request.IsAjaxRequest())
        //        {
        //            return PartialView("AutorizzaUOCode", id);
        //        }
        //    }
        //    return RedirectToAction("GestioneRichieste");
        //}

        public ActionResult AutorizzaUOCode()
        {
            ViewData["auth"] = true;
            //var lista = repository.getFogli().Where(a => a.UOCode == MioUocode).ToList();

            return PartialView("AutorizzaUOCode", repository.getFogli());

        }

        [OutputCache(Duration = 0, NoStore = true, VaryByParam = "*")]
        public ActionResult AssociaUOCODE(Guid? guid)
        {
            FCIContext db = new FCIContext();
            List<SelectListItem> listSelectListItems = new List<SelectListItem>();

            var richieste = db.Fogli
                .Include(a => a.UocodeWL)
                .Where(b => b.Guid == guid).FirstOrDefault();

            foreach (UocodeWL f in richieste.UocodeWL)
            {
                SelectListItem selectList = new SelectListItem()
                {
                    Text = f.uocode,
                    Value = f.Id.ToString(),
                    Selected = false
                };
                listSelectListItems.Add(selectList);
            }

            uocodeViewModel uocodesel = new uocodeViewModel()
            {
                UOCodelist = listSelectListItems
            };
            ViewBag.guid = guid;
            return View(uocodesel);


            //CitiesViewModel cit = new CitiesViewModel()
            //{
            //    Cities = listSelectListItems
            //};
            //return View(cit);



            //if (guid == null) guid = Guid.Empty;
            //var result = new ViewModel { guid = (Guid)guid };
            //ViewBag.guid = guid;
            //Console.WriteLine("guid test " + guid);
            //return View(result);
        }

        public ActionResult View1()
        {
            Guid myguid = Guid.Parse("8293F01F-DBAE-42DA-B2D2-78C001B60B7D");
            FCIContext db = new FCIContext();
            List<SelectListItem> listSelectListItems = new List<SelectListItem>();
            var richieste = db.Fogli
                .Include(a => a.UocodeWL)
                .Where(b => b.Guid == myguid).FirstOrDefault();

            foreach (UocodeWL f in richieste.UocodeWL)
            {
                SelectListItem selectList = new SelectListItem()
                {
                    Text = f.uocode,
                    Value = f.Id.ToString(),
                    Selected = false
                };
                listSelectListItems.Add(selectList);
            }

            ViewModel v = new ViewModel();
            v.guid = myguid;
            return View(v);
        }

        [HttpPost]
        public JsonResult AggiornaUocode(string uocode, int profilo, Guid? fogli_guid)
        {
            return Json(repository.AggiornaUocode(uocode, profilo, (Guid)fogli_guid), JsonRequestBehavior.AllowGet);
            
        }
        public JsonResult LeggiUOCode_WL( Guid? fogli_guid)
        {
            
            JsonResult result= Json(repository.LeggiUOCode_WL((Guid)fogli_guid), JsonRequestBehavior.AllowGet);
            //JsonResult result = Json("[{\'wl_uocode\':\'44444\'}]", JsonRequestBehavior.AllowGet);
            return result;
            //"[{\"wl_uocode\":\"44444\"}]"
            //var result = repository.LeggiUOCode_WL((Guid)fogli_guid);
            //if (result!=null)
            //{

            //    //IEnumerable<UocodeWL> uu = ((IEnumerable<UocodeWL>)result);
            //    foreach (UocodeWL u in ((IEnumerable<UocodeWL>)result))
            //    {

            //    }

            //}

            //return null;

        }
    }
}
