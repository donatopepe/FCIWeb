using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AreaCommonWeb.Business;
using AreaCommonWeb.Controllers;
using AreaCommonWeb.DAL;
using AreaCommonWeb.ModelView;
using AreaCommonWeb.ModelView.Security;
using AreaCommonWeb.Models;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
//using ProfileManagerWeb.Business;
using Telerik.Web.Mvc;
using Elmah;
using Newtonsoft.Json;

namespace FCIWeb.Areas.FCI.Controllers
{
    public partial class RichiestaController : Controller
    {
        //public ActionResult Index()
        //{
        //  return View();
        //}

        //inizializzo il repository
        //private UtenzaBusiness utenzaBusiness = new UtenzaBusiness();
        //private CommonBusiness commonBusiness = new CommonBusiness();



        //[PermitAccess]
        //public ActionResult GestioneUtenti()
        //{
        //  //ISTITUTO
        //  var selectListIst = new[] { 
        //                            new Istituto { codice = 0 , nome = ""}, 
        //                            new Istituto { codice = 46, nome = "46"}, 
        //                            new Istituto { codice = 47, nome = "47"}
        //                        };
        //  ViewData["SelIstituti"] = new SelectList(selectListIst, "codice", "nome");

        //  //AREA
        //  var selectListArea = new[] {  

        //                            new Area { codice = 1, nome = "1"}, 
        //                            new Area { codice = 2, nome = "2"},
        //                            new Area { codice = 3, nome = "3"},
        //                            new Area { codice = 4, nome = "4"},
        //                            new Area { codice = 5, nome = "5"}
        //                        };
        //  ViewData["SelArea"] = new SelectList(selectListArea, "codice", "nome");

        //  //FILIALI
        //  IQueryable<Vista_Filiali> listFil = commonBusiness.GetFiliali(selectListIst.First().codice, 0);
        //  ViewData["SelFiliali"] = (IEnumerable<String>)listFil.Select(f => f.DescFiliale).ToList();

        //  //RUOLI
        //  List<DimRuoli> listRuoli = commonBusiness.GetRuoli(this.CodApp);
        //  ViewData["SelRuoli"] = new SelectList(listRuoli, "CodRuolo", "NomeRuolo");

        //  return View("GestioneUtenti");
        //}


        //[GridAction]
        //public ActionResult GestioneRuoli(string matricola)
        //{

        //  ViewData["tipoAzione"] = "";
        //  //ISTITUTO
        //  var selectListIst = new[] {
        //                            new Istituto { codice = 0 , nome = ""}, 
        //                            new Istituto { codice = 46, nome = "46"}, 
        //                            new Istituto { codice = 47, nome = "47"}
        //                        };
        //  ViewData["SelIstituti"] = new SelectList(selectListIst, "codice", "nome");

        //  //AREA
        //  var selectListArea = new[] {  

        //                            new Area { codice = 1, nome = "1"}, 
        //                            new Area { codice = 2, nome = "2"},
        //                            new Area { codice = 3, nome = "3"},
        //                            new Area { codice = 4, nome = "4"},
        //                            new Area { codice = 5, nome = "5"}
        //                        };
        //  ViewData["SelArea"] = new SelectList(selectListArea, "codice", "nome");

        //  //SEGMENTI
        //  var selectListSegmenti = new[] {  

        //                            new Segmento{codice = "R", nome = "R"},
        //                            new Segmento{codice = "C", nome = "C"}

        //                        };
        //  ViewData["SelSegmento"] = new SelectList(selectListSegmenti, "codice", "nome");

        //  //FILIALI
        //  IQueryable<Vista_Filiali> listFil = commonBusiness.GetFiliali(selectListIst.First().codice, 0);
        //  ViewData["SelFiliali"] = (IEnumerable<String>)listFil.Select(f => f.DescFiliale).ToList();

        //  //RUOLI
        //  List<DimRuoli> listRuoli = commonBusiness.GetRuoli(this.CodApp);
        //  ViewData["SelRuoli"] = new SelectList(listRuoli, "CodRuolo", "NomeRuolo");
        //  ViewData["matricola"] = matricola;
        //  return View("RuoliUtente");
        //}

        //[GridAction]
        //public ActionResult _GridElencoRuoliUtenteBinding(string matricola)
        //{
        //  WebServiceModel.WSSicurezza.Service WSSicurezza = new WebServiceModel.WSSicurezza.Service();
        //  IEnumerable<WebServiceModel.WSSicurezza.Utenza> listUtenze = WSSicurezza.GetUtenze(this.CodApp, matricola);
        //  return View(new GridModel(listUtenze));
        //}

        //[HttpPost]
        //public ActionResult _GridElencoUtentiOrgBinding(string matricola,
        //                                                    string nome,
        //                                                    string cognome
        //                                                    )
        //{
        //  //UTENTI DA ORGANIGRAMMA
        //  List<DimResource> elencoUtenti = utenzaBusiness.GetUtentiOrg(matricola, nome, cognome);
        //  return PartialView("_GridElencoUtentiOrg", elencoUtenti);
        //}

        //[HttpPost]
        //public void InsertRuolo(string matricola,
        //                                                string istitutoR,
        //                                                string areaR,
        //                                                string segmentoR,
        //                                                string ruoloR,
        //                                                string filialeR
        //                                                    )
        //{
        //  UserProfile myProfile = new UserProfileManager().GetCurrent();
        //  try
        //  {
        //    //INSERIMENTO RUOLO
        //    var insertRuolo = utenzaBusiness.InsertRuoloUtente(matricola, istitutoR, areaR, segmentoR, ruoloR,
        //                                                       filialeR, CodApp,
        //                                                       myProfile.UserInfo.Matricola);
        //  }
        //  catch (Exception ex)
        //  {
        //    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
        //    if (ex.GetType().Name.Equals("DbUpdateException"))
        //    {
        //      string message = "Seleziona Ruolo!";
        //      ViewBag.ErrorMessage = message;
        //    }
        //  }
        //}

        //[HttpPost]
        //public void UpdateRuolo(string idUtenzaR,
        //                        string istitutoR,
        //                        string areaR,
        //                        string segmentoR,
        //                        string ruoloR,
        //                        string filialeR)
        //{
        //  UserProfile myProfile = new UserProfileManager().GetCurrent();
        //  //UPDATE RUOLO
        //  var utenza = utenzaBusiness.UpdateRuoloUtente(idUtenzaR, istitutoR, areaR, segmentoR, ruoloR, filialeR, this.CodApp, myProfile.UserInfo.Matricola);
        //}

        //[HttpPost]
        //public ActionResult GetUtenzaById(string idUtenza)
        //{
        //  //RECUPERO L'UTENZA PER POPOLARE IL FILTRI IN VIEW
        //  Dictionary<String, String> utenza = utenzaBusiness.GetUtenzaById(idUtenza);
        //  return Json(utenza);
        //}

        //[HttpPost]
        //public void DeleteRuolo(string idUtenza, string matricola)
        //{
        //  UserProfile myProfile = new UserProfileManager().GetCurrent();
        //  //ELIMINAZIONE RUOLO
        //  var deleteRuolo = utenzaBusiness.DeleteRuoloUtente(idUtenza, myProfile.UserInfo.Matricola, this.CodApp);
        //}

        //[HttpPost]
        //[HttpGet]
        //public ActionResult _FilialiBinding(string istituto, string area)
        //{
        //  int codArea = 0;
        //  //REBIND DELL'AUTOCOMPLETE DELLE FILIALI
        //  if (area != null && !area.Equals(""))
        //  {
        //    codArea = Convert.ToInt32(area);
        //  }
        //  IQueryable<Vista_Filiali> listFil = commonBusiness.GetFiliali(Convert.ToInt32(istituto), codArea);
        //  return new JsonResult { Data = (IEnumerable<String>)listFil.Select(f => f.DescFiliale).ToList(), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        //}


        //public ActionResult _GridElencoUtentiCensBinding(string matricola,
        //                                                string nome,
        //                                                string cognome,
        //                                                string istituto,
        //                                                string area,
        //                                                string segmento,
        //                                                string ruolo,
        //                                                string filiale

        //                                                    )
        //{
        //  List<GridElencoUtentiCensModel> elencoUtenti = utenzaBusiness.GetUtentiCens(matricola, nome, cognome, istituto, area, segmento, filiale, ruolo, this.CodApp);
        //  return PartialView("_GridElencoUtentiCensiti", elencoUtenti);
        //}
        //public ActionResult GestioneUtentiOrg(string matricola)
        //{
        //  return View("_FiltriUtenteOrg");
        //}

        //[GridAction]
        //public ActionResult _GridElencoRuoliUtentiCensBinding(string matricola)
        //{
        //  List<GridElencoRuoliUtenteCen> elencoRuoli = utenzaBusiness.GetRuoliUtente(matricola, this.CodApp);
        //  return View(new GridModel(elencoRuoli));
        //}

        public ActionResult GestioneProfili(Guid? id)
        {
            if ((id != null) && (id != Guid.Empty))
            {
                ViewData["IdFoglio"] = id;
                return View("GestioneProfili");
            }
            return RedirectToAction("ApriRichiestaVuota");
        }




        public JsonResult _GetMatricoleOrg(string text)
        {
            IQueryable<ResourcesModel> listaUnita = commonBusiness.getResourcesByText(text, "matricola");

            return Json(listaUnita);
        }

        public JsonResult _GetUtenze(string text)
        {
            IQueryable<ResourcesModel> listaUnita = commonBusiness.getResourcesByText(text, "cognome");

            return Json(listaUnita);
        }

        public ActionResult ElencoUtenti(string UocodeSelez, string MatricolaSelez)
        {
            //ErrorSignal.FromCurrentContext().Raise(new Exception("UocodeSelez:" + UocodeSelez));
            //ErrorSignal.FromCurrentContext().Raise(new Exception("MatricolaSelez:" + MatricolaSelez));
            Dictionary<string, string> model = new Dictionary<string, string>();
            model.Add(UocodeSelez, MatricolaSelez);
            return View(model);
        }

        public ActionResult _GridElencoUtenti([DataSourceRequest] DataSourceRequest request, string UocodeSelez, string MatricolaSelez)
        {
            //in caso di uocode e matricola entrambe selezionate avrà priorità la matricola
            //ErrorSignal.FromCurrentContext().Raise(new Exception("UocodeSelez:" + UocodeSelez));
            //ErrorSignal.FromCurrentContext().Raise(new Exception("MatricolaSelez:" + MatricolaSelez));

            IQueryable<vw_Prof_Resource> resource = commonBusiness.getResourcesByUoCodeMatricola(UocodeSelez, MatricolaSelez);
            //ErrorSignal.FromCurrentContext().Raise(new Exception("resource" + JsonConvert.SerializeObject(resource)));
            return Json(resource.ToDataSourceResult(request));
        }

        //public void resetUtenza()
        //{
        //  UserProfileManager.RemoveCurrent();
        //  Session.Abandon();
        //}

    }
}
