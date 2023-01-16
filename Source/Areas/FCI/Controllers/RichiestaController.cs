using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using AreaCommonWeb.Business;
using DAL.AreaCommonWeb;
using AreaCommonWeb.DAL;
using AreaCommonWeb.DAL.Profiles;
using AreaCommonWeb.Models;
using AreaCommonWeb.ModelView.Security;
using AreaCommonWeb.ModelView;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using WebServiceModel.WSAnagrafe;
using WebServiceModel.DAL;

namespace FCIWeb.Areas.FCI.Controllers
{
    public partial class RichiestaController : Controller
    {

        public RichiestaController()
        {
            UserProfile myProfile = new UserProfileManager().GetCurrent();
            codRuolo = myProfile.Utenza.CodRuolo;
            var myUocode = UserProfileManager.GetFiltroSicurezza(myProfile, "codUnitRuolo");
            string flagUffRuolo = UserProfileManager.GetFiltroSicurezza(myProfile, "flagUffRuolo"); //////non credo serva
            string filtroIst = UserProfileManager.GetFiltroSicurezza(myProfile, "IST");
            int? istID = !String.IsNullOrEmpty(filtroIst) ? Convert.ToInt32(filtroIst) : Convert.ToInt32(myProfile.UserInfo.CodIstituto);
            var matricola = myProfile.UserInfo.Matricola;
            ViewBag.Operatore = myProfile.UserInfo.Nome;

            if (istID != null) MioIstituto = (int)istID;
            MiaMatricola = matricola;
            MioUocode = myUocode;
            var myFlaf = myProfile.UserInfo.FlagUfficioFiliale;
            if ((myFlaf == "F") || (myFlaf == "O"))
            {
                isFiliale = true;
            }
            else
            {
                isFiliale = false;
            }

        }

        [HttpPost]
        [OutputCache(Duration = 0, NoStore = true, VaryByParam = "*")]
        public JsonResult GetFiliali(string prefix)
        {
            IQueryable<DimUO> filiali = commonBusiness.getUoByText(prefix).Where(a => a.FlagUfficioFiliale == "F" || a.FlagUfficioFiliale == "O");
            //var indirizzo = filiali.FirstOrDefault().Indirizzo;
            return Json(filiali, JsonRequestBehavior.AllowGet);
        }

        [OutputCache(Duration = 0, NoStore = true, VaryByParam = "*")]
        public ActionResult GestioneRichieste(Guid? id)
        {
            UserProfile myProfile = new UserProfileManager().GetCurrent();
            ViewData.Add("istituto", myProfile.UserInfo.CodIstituto);
            ViewData.Add("myUocode", myProfile.UserInfo.CodUnit);


            var myUocode = UserProfileManager.GetFiltroSicurezza(myProfile, "codUnitRuolo");

            string flagUffRuolo = UserProfileManager.GetFiltroSicurezza(myProfile, "flagUffRuolo");

            //ISTITUTO
            var selectListIst = new[] {
                                new Istituto { codice = 0 , nome = ""},
                                new Istituto { codice = 46, nome = "46"},
                                new Istituto { codice = 47, nome = "47"}
                            };


            //ViewData["SelIstituti"] = new SelectList(selectListIst, "codice", "nome");


            //AREA
            string filtroSeg = UserProfileManager.GetFiltroSicurezza(myProfile, "SEG");
            string segArea = !String.IsNullOrEmpty(filtroSeg)
                                     ? filtroSeg
                                     : UserProfileManager.GetFiltroSicurezza(myProfile, "mercato");
            //segArea = "";
            IQueryable listAree;

            //FILIALI
            IQueryable<Vista_Filiali> listFil = commonBusiness.GetFilialiHub(selectListIst.First().codice, "");

            //Gestori
            string filtroIst = UserProfileManager.GetFiltroSicurezza(myProfile, "IST");
            int? istituto = !String.IsNullOrEmpty(filtroIst) ?
                                   Convert.ToInt32(filtroIst) :
                                   Convert.ToInt32(myProfile.UserInfo.CodIstituto);

            ViewData["DataDa"] = System.DateTime.Today.AddMonths(-1).ToString().Substring(0, 10);
            ViewData["DataA"] = System.DateTime.Today.ToString().Substring(0, 10);

            int? istID = !String.IsNullOrEmpty(filtroIst) ?
                                     Convert.ToInt32(filtroIst) :
                                     Convert.ToInt32(myProfile.UserInfo.CodIstituto);

            FiltriHomeModel filtri = new FiltriHomeModel();

            //Check Mercati
            //List<CheckMercatiModel> checkMercati = richiestaBusiness.GetCheckBoxMercati(segArea, myProfile.RuoliSicurezza, "");
            //ViewData["checkBoxMercati"] = checkMercati;

            if (UserProfileManager.ContainsSottoFunzione(myProfile.RuoliSicurezza, "S01"))
            {
                istID = null;
                listAree = commonBusiness.GetAree("", null);
                ViewData["SelIstituti"] = new SelectList(selectListIst, "codice", "nome", 0);
                ViewData["SelArea"] = new SelectList(listAree, "UOArea", "CodArea", 0);
                ViewData["SelFilialiHub"] = new SelectList(new[] { "" }, "", "");
                ViewData["SelFilialiSpoke"] = new SelectList(new[] { "" }, "", "");
                ViewData["selFiltriHome"] = "false";
            }
            else
            {
                istID = Convert.ToInt32(istID);
                listAree = commonBusiness.GetAree("", istID);
                filtri = commonBusiness.GetFiltriHome(istID, myUocode, flagUffRuolo, segArea);

                ViewData["SelIstituti"] = new SelectList(selectListIst, "codice", "nome", istID);
                ViewData["SelArea"] = new SelectList(listAree, "UOArea", "CodArea", filtri.area);
                if (filtri.filialiHub.Any() && filtri.filialiHub.First().FlagUfficioCapofila == "T")
                {
                    ViewData["SelFilialiHub"] = new SelectList(filtri.filialiHub.Select(x => new { UOFiliale = x.UOCapofila, Filiale = x.DescCapofila.Split('-')[1] }).Distinct(), "UOFiliale", "Filiale"); //(IEnumerable<String>)filtri.filialiHub.Select(f => f.DescFiliale).ToList();

                }
                else
                {
                    ViewData["SelFilialiHub"] = new SelectList(filtri.filialiHub, "UOFiliale", "Filiale"); //(IEnumerable<String>)filtri.filialiHub.Select(f => f.DescFiliale).ToList();

                }
                ViewData["SelFilialiSpoke"] = new SelectList(filtri.filialiSpoke, "UOFiliale", "Filiale"); //(IEnumerable<String>)filtri.filialiSpoke.Select(f => f.DescFiliale).ToList();
                ViewData["selFiltriHome"] = "true";
            }



            ViewData["istituto"] = istID;
            ViewData["area"] = filtri.area;
            ViewData["flagUff"] = flagUffRuolo;
            ViewData["uocode"] = myUocode;
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
            ViewData["ApriRichiestaId"] = "";
            if ((id != null) && (id != Guid.Empty))
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
            filtri.myUocode = myUocode;
            filtri.flagUffFil = FlagUff;
            filtri.campoGenerico = propBlocPost;

            if (filtri.istituto == null) filtri.istituto = MioIstituto;
            ViewData["istituto"] = filtri.istituto; // da valutare
            ViewData["uocode"] = myUocode; // da valutare

            return PartialView("_GridElencoRichieste", repository.getFogli(filtri, true));

        }
        [OutputCache(Duration = 0, NoStore = true, VaryByParam = "*")]
        public ActionResult InitSearch()
        {
            return View(new AreaCommonWeb.Models.CampiRicerca());
        }

        public JsonResult _GetUnitaOrganizzativa(string text)
        {
            //IQueryable<DimUO> listaUnita = commonBusiness.getUoByText(text).Where(a => a.FlagUfficioFiliale == "F" || a.FlagUfficioFiliale == "O");
            IQueryable<DimUO> listaUnita = commonBusiness.getUoByText(text);
            return Json(listaUnita);
        }

        [HttpPost]
        public string DescFilialeByUocode(string uocode)
        {

            string uoForm = "000000" + uocode;
            string uoSubstring = uoForm.Substring(uoForm.Length - 6);

            string desc = "";
            var result = resourcesRepository.GetDimUoByUocode(uoSubstring).FirstOrDefault();
            if (result != null)
            {
                desc = result.DescrizioneUO;
            }

            return desc;
        }


    }
}
