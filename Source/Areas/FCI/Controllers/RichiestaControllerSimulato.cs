using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using FCIWeb.Areas.FCI.DAL;
using FCIWeb.Areas.FCI.Models;
using FCIWeb.Areas.FCI.Uility;
using AreaCommonWeb.Business;
using DAL.AreaCommonWeb;
using AreaCommonWeb.DAL;
using AreaCommonWeb.ModelView.Security;
using FCIWeb.Areas.FCI.Repository;
using AreaCommonWeb.ModelView;
using WebServiceModel.WSAnagrafe;
using WebServiceModel.DAL;
using AreaCommonWeb.Models;

namespace FCIWeb.Areas.FCI.Controllers
{
    public partial class RichiestaController : Controller
    {
        public RichiestaController()
        {

            UserProfile myProfile = new UserProfileManager().GetCurrent();
            var myUocode = UserProfileManager.GetFiltroSicurezza(myProfile, "codUnitRuolo");
            string flagUffRuolo = UserProfileManager.GetFiltroSicurezza(myProfile, "flagUffRuolo"); //////non credo serva
            string filtroIst = UserProfileManager.GetFiltroSicurezza(myProfile, "IST");
            int? istID = !String.IsNullOrEmpty(filtroIst) ? Convert.ToInt32(filtroIst) : Convert.ToInt32(myProfile.UserInfo.CodIstituto);
            var matricola = myProfile.UserInfo.Matricola;
            ViewBag.Operatore = myProfile.UserInfo.Nome;



            MiaMatricola = "131132";        ///////////////////////simulato
            MioUocode = "12345"; ////////////////////////simulato
            MioIstituto = 0; ////////////////////////simuulato

            isFiliale = true;
            ViewBag.Operatore = "Pluto";
            codRuolo = "adm";

        }



        [HttpPost]
        [OutputCache(Duration = 0, NoStore = true, VaryByParam = "*")]
        public JsonResult GetFiliali(string prefix)
        {
            //IQueryable<DimUO> filiali = commonBusiness.getUoByText(prefix).Where(a => a.FlagUfficioFiliale == "F" || a.FlagUfficioFiliale == "O");
            var filiali = new List<DimUO> {
                new DimUO { DescrizioneUO="Bari", UOCode="435678"},
                new DimUO { DescrizioneUO="Taranto", UOCode="55533"},
                new DimUO { DescrizioneUO="Foggia", UOCode="3534553"},
                new DimUO { DescrizioneUO="Brindisi", UOCode="54333"},
                new DimUO { DescrizioneUO="Roma", UOCode="6666666"},
                new DimUO { DescrizioneUO="Milano", UOCode="4444444"},
            };
            return Json(filiali, JsonRequestBehavior.AllowGet);
        }

        [OutputCache(Duration = 0, NoStore = true, VaryByParam = "*")]
        public ActionResult GestioneRichieste(Guid? id)
        {
            //FiltriRichiesteModel filtri = new FiltriRichiesteModel();

            UserProfile myProfile = new UserProfileManager().GetCurrent();
            ViewData.Add("istituto", myProfile.UserInfo.CodIstituto);
            ViewData.Add("myUocode", myProfile.UserInfo.CodUnit);

            var myUocode = UserProfileManager.GetFiltroSicurezza(myProfile, "codUnitRuolo");

            string flagUffRuolo = UserProfileManager.GetFiltroSicurezza(myProfile, "flagUffRuolo");

            //ISTITUTO
            var selectListIst = new[]
            {
            //new Istituto {codice = 0, nome = ""},
            new Istituto {codice = 46, nome = "46"},
            new Istituto {codice = 47, nome = "47"}
          };
            ViewData["SelIstituti"] = new SelectList(selectListIst, "codice", "nome");

            //string filtroIst = UserProfileManager.GetFiltroSicurezza(myProfile, "IST");
            //int? istID = !String.IsNullOrEmpty(filtroIst)
            //  ? Convert.ToInt32(filtroIst)
            //  : Convert.ToInt32(myProfile.UserInfo.CodIstituto);


            //IQueryable listAree;
            //if (UserProfileManager.ContainsSottoFunzione(myProfile.RuoliSicurezza, "S01"))
            //{
            //    istID = Convert.ToInt32(istID);
            //    ViewData["SelIstituti"] = new SelectList(selectListIst, "codice", "nome", istID);
            //}
            //else
            //{
            //    istID = Convert.ToInt32(istID);
            //    ViewData["SelIstituti"] = new SelectList(selectListIst, "codice", "nome", istID);

            //}

            ViewData["istituto"] = MioIstituto; //istID;
            ViewData["haveSottoFunzS01"] = UserProfileManager.ContainsSottoFunzione(myProfile.RuoliSicurezza, "S01");

            var lista = new List<SelectListItem>();
            foreach (var item in repository.GetStati().Where(a => a.IsActive))
            {
                var pippo = new SelectListItem();
                pippo.Text = item.Descrizione;
                pippo.Value = item.Id.ToString();
                lista.Add(pippo);
            }
            ViewData["StatoRichiestaF"] = lista;



            //ViewBag.MioUocode = MioUocode;
            //ViewBag.MiaMatricola = MiaMatricola;
            //filtri.filtroUocode = MioUocode;

            //return View(new AreaCommonWeb.Models.CampiRicerca());

            ViewData["ApriRichiestaId"] = "";
            if ((id != null)&&(id!=Guid.Empty))
            {
                ViewData["ApriRichiestaId"] = id;
            }
            return View("GestioneRichieste");
        }

        [HttpPost]
        [OutputCache(Duration = 0, NoStore = true, VaryByParam = "*")]
        public PartialViewResult _GridElencoRichieste(string intestazione,
  string DataRichiestaDA,
  string DataRichiestaA,
  int? StatoRichiestaF,
  string ndg,
  string istitutoR,
  string FlagUff,
  string uocode,
  string propBlocPost

)
        {
            UserProfile myProfile = new UserProfileManager().GetCurrent();

            var myUocode = UserProfileManager.GetFiltroSicurezza(myProfile, "codUnitRuolo");

            FiltriRichiesteModel filtri = new FiltriRichiesteModel();
            filtri.istituto = istitutoR != "" ? Convert.ToInt32(istitutoR) : (int?)null;
            filtri.idStatoOrdine = StatoRichiestaF;
            filtri.intestazione = intestazione;
            filtri.ndg = ndg != "" ? Convert.ToInt64(ndg) : (long?)null;
            filtri.dataOrdineA = DataRichiestaA;
            filtri.dataOrdineDA = DataRichiestaDA;
            filtri.ruoliSicurezza = myProfile.RuoliSicurezza;
            filtri.filtroUocode = uocode;
            filtri.myUocode = MioUocode;            //////simulato
            filtri.flagUffFil = FlagUff;
            filtri.campoGenerico = propBlocPost;


            if (filtri.istituto == null) filtri.istituto = MioIstituto;
            ViewData["istituto"] = filtri.istituto; // da valutare
            ViewData["uocode"] = filtri.myUocode; // da valutare

            return PartialView("_GridElencoRichieste", repository.getFogli(filtri, true));
        }

        public JsonResult _GetUnitaOrganizzativa(string text)
        {
            //IQueryable<DimUO> listaUnita = commonBusiness.getUoByText(text);
            var listaUnita = new List<DimUO> {
                    new DimUO { DescrizioneUO="Bari", UOCode="435678"},
                    new DimUO { DescrizioneUO="Taranto", UOCode="55533"},
                    new DimUO { DescrizioneUO="Foggia", UOCode="3534553"},
                    new DimUO { DescrizioneUO="Brindisi", UOCode="54333"},
                    new DimUO { DescrizioneUO="Roma", UOCode="6666666"},
                    new DimUO { DescrizioneUO="Milano", UOCode="4444444"},
                };
            return Json(listaUnita);
        }

    }
}
