using AreaCommonWeb.Business;
using AreaCommonWeb.DAL.Profiles;
using DAL.AreaCommonWeb;
using FCIWeb.Areas.FCI.Models;
using FCIWeb.Areas.FCI.Repository;
using FCIWeb.Areas.FCI.Uility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace FCIWeb.Areas.FCI.Controllers
{
    [OutputCache(Duration = 0, NoStore = true, VaryByParam = "*")]
    public partial class RichiestaController : Controller
    {
        public JsonResult getFoglio(Guid? guid)
        {
            var foglio = repository.getFoglio((Guid)guid);
            object result = new {
                foglio.NomeLista,
                RowVersion = foglio.RowVersion.ToString(),
                stato = foglio.Stato,
                stati = repository.GetStati((Guid)guid),
            };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getColonne(Guid? guid)
        {
            return Json(repository.getColonne((Guid)guid), JsonRequestBehavior.AllowGet);
        }
 
        public JsonResult UpdateFoglio(Guid? guid, string NomeLista, string rowversion)
        {
            return Json(repository.UpdateFoglio((Guid)guid, NomeLista, rowversion), JsonRequestBehavior.AllowGet);
        }
 
        public JsonResult UpdateColonne(Guid? guid, string colonneJson, string rowversion)
        {
            return Json(repository.UpdateColonna((Guid)guid, colonneJson, rowversion), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult LogStati(Guid? id)
        {
            if (id != null)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("LogStati", repository.GetLogStati((Guid)id));
                }
            }
            return RedirectToAction("GestioneRichieste");
        }


        public ActionResult ApriRichiesta(Guid? guid)
        {
            if (guid == null) guid = Guid.Empty;
            var result = repository.ApriRichiesta((Guid)guid);
            return View(result);
        }

        public ActionResult Elimina(Guid? id)
        {
            if (id == null) id = Guid.Empty;
            var result = repository.Elimina((Guid)id);
            return RedirectToAction("GestioneRichieste", new { id = result });
        }

        public ActionResult Details(Guid? guid)
        {
            var profilo = new Profili();
            profilo.Descrizione = "Details";
            profilo.Nome = "Details";
            profilo.ModificaLista = false;
            profilo.Compilazione = false;
            profilo.Esportazione = false;

            if (guid == null) guid = Guid.Empty;
            var foglio = repository.getFoglio((Guid)guid);
            var result = new ViewModel { guid = (Guid)guid, profilo = profilo, stato = foglio.Stato, NomeLista=foglio.NomeLista };
            return View("ApriRichiesta", result);
        }

        public ActionResult ApriRichiestaVuota(Guid? id)
        {
            if ((id != null) && (id != Guid.Empty))
            {
                ViewData["ApriRichiestaId"] = id;
            }
            return View();
        }


        [HttpPost]
        public ActionResult ImportaFile(HttpPostedFileBase FileForm, string Nome)
        {
            var fileForm = new FromFileHttp().FileForm(FileForm);
            
            if ((fileForm != null) && (String.IsNullOrWhiteSpace(fileForm.err)))
            {
                var result = repository.Write(fileForm, Nome);
                return RedirectToAction("ApriRichiestaVuota", new { id = result });
            }
            else
            {
                if (fileForm != null)
                {
                    //ErroreSpecifico("FileForm", fileForm.err);
                }
                else
                {
                    //ErroreSpecifico("FileForm", "Errore file");
                }
            }
            return RedirectToAction("ApriRichiestaVuota");
        }

        public JsonResult Lista(Guid? guid)
        {
            var result = Json(repository.getFoglioJExcel(guid), JsonRequestBehavior.AllowGet);
            return result;
        }
   
        public JsonResult UpdateCella(Guid? guid, string cellName, string value, string rowversion)
        {

            if (String.IsNullOrWhiteSpace(cellName)) return null;

            return Json(repository.UpdateCellaJExcel((Guid)guid, cellName, value, rowversion), JsonRequestBehavior.AllowGet);
        }
    
        public JsonResult InsertRow(Guid? guid, int y, int Before, string rowversions)
        {
            var RowVersions = JsonConvert.DeserializeObject<List<string>>(rowversions).Select(a => Convert.ToInt64(a)).ToList();

            if (y < 0) return null;
            return Json(repository.InsertRowJExcel((Guid)guid, y, Before, RowVersions), JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteRow(Guid? guid, int y, string rowversions)
        {
            
            var RowVersions = JsonConvert.DeserializeObject<List<string>>(rowversions).Select(a=>Convert.ToInt64(a)).ToList();

            if ( y < 0) return null;

            return Json(repository.DeleteRowJExcel((Guid)guid, y, RowVersions), JsonRequestBehavior.AllowGet);
        }
        public JsonResult InsertCol(Guid? guid, int x, int Before, string rowversions)
        {
            var RowVersions = JsonConvert.DeserializeObject<List<string>>(rowversions).Select(a => Convert.ToInt64(a)).ToList();

            if ( x < 0) return null;
            return Json(repository.InsertColJExcel((Guid)guid, x, Before, RowVersions), JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteCol(Guid? guid, int x, string rowversions)
        {
            var RowVersions = JsonConvert.DeserializeObject<List<string>>(rowversions).Select(a => Convert.ToInt64(a)).ToList();

            if ( x < 0) return null;

            return Json(repository.DeleteColJExcel((Guid)guid, x, RowVersions), JsonRequestBehavior.AllowGet);
        }
        public JsonResult RenameCol(Guid? guid, int x, string title)
        {

            if (x < 0) return null;

            return Json(repository.RenameColJExcel((Guid)guid, x, title), JsonRequestBehavior.AllowGet);
        }
        public JsonResult ResizeCol(Guid? guid, int x, int width)
        {
            if (x < 0) return null;
            return Json(repository.ResizeColJExcel((Guid)guid, x, width), JsonRequestBehavior.AllowGet);
        }
        public JsonResult SaveStyle(Guid? guid, string StileFoglio)
        {
            return Json(repository.SaveStyle((Guid)guid, StileFoglio), JsonRequestBehavior.AllowGet);
        }

        public JsonResult SetStato(Guid? guid, string Nome, string Motivazione,  string rowversion)
        {

            if (String.IsNullOrWhiteSpace(Nome)) return null;

            return Json(repository.SetStato((Guid)guid, Nome, Motivazione, rowversion), JsonRequestBehavior.AllowGet);
        }
        public JsonResult getprofiliAvviabili()
        {
            return Json(repository.getprofiliAvviabili(), JsonRequestBehavior.AllowGet);
        }
        public JsonResult getProfiliWL(Guid? guid)
        {
            return Json(repository.getProfiliWL((Guid)guid), JsonRequestBehavior.AllowGet);
        }
        public JsonResult SetProfilo(Guid? guid, string istituto, string uocodematricola, string profilo)
        {

            if (String.IsNullOrWhiteSpace(profilo)) return null;

            return Json(repository.SetProfilo((Guid)guid, istituto, uocodematricola, profilo), JsonRequestBehavior.AllowGet);
        }
        public JsonResult AddProfilo(Guid? guid, string istituto, string uocodematricola, bool IsUocode)
        {

            if (String.IsNullOrWhiteSpace(uocodematricola)) return null;

            return Json(repository.AddProfilo((Guid)guid, istituto, uocodematricola, IsUocode), JsonRequestBehavior.AllowGet);
        }
        public JsonResult RemoveProfilo(Guid? guid, string istituto, string uocodematricola, bool IsUocode)
        {

            if (String.IsNullOrWhiteSpace(uocodematricola)) return null;

            return Json(repository.RemoveProfilo((Guid)guid, istituto, uocodematricola, IsUocode), JsonRequestBehavior.AllowGet);
        }
    }
}
