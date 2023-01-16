using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using FCIWeb.Areas.FCI.DAL;
using FCIWeb.Areas.FCI.Models;
using FCIWeb.Areas.FCI.Uility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Data.Entity;
using System.Text.RegularExpressions;
using AreaCommonWeb.Models;

namespace FCIWeb.Areas.FCI.Repository
{
    public partial class FCIRepository : IDisposable
    {

        public class colonneUocodeWL
        {
            public string wl_uocode { get; set; }            
        }
        
        public IEnumerable<Fogli> GetRichgetFogliieste(FiltriRichiesteModel filtri)
        {
            var uocodedaricercare = filtri.myUocode;
            int istituto = 0;
            if (!String.IsNullOrWhiteSpace(filtri.filtroUocode))
            {
                uocodedaricercare = filtri.filtroUocode;
            }
            if (filtri.istituto != null)
            {
                istituto = (int)filtri.istituto;
            }
            
            var richieste = db.Fogli
                .Include(a => a.Stato)
                //.Include(b => b.Stati)
                .Where(d => ((d.Stato.IsActive) && (d.istID == istituto))).ToList();

            //richieste = richieste.Where(d => uocodeOk(d, uocodedaricercare)).ToList();

            if ((richieste != null) && (filtri.idStatoOrdine != null))
            {
                richieste = richieste.Where(a => a.Stato.Id == filtri.idStatoOrdine).ToList();
            }
            if ((richieste != null) && (filtri.istituto != null))
            {
                richieste = richieste.Where(a => a.istID == filtri.istituto).ToList();
            }
            if ((richieste != null) && (!String.IsNullOrWhiteSpace(filtri.matricolaOperatore)))
            {
                richieste = richieste.Where(a => a.Matricola == filtri.matricolaOperatore).ToList();
            }
            if ((richieste != null) && !String.IsNullOrWhiteSpace(filtri.intestazione))
            {
                richieste = richieste.Where(a => ContainNormalized(a.NomeLista, filtri.intestazione)).ToList();
            }

            //if ((richieste != null) && (filtri.ndg != null))
            //{
            //    richieste = richieste.Where(a => a.NDG == (long)filtri.ndg).ToList();
            //}
            //if ((richieste != null) && !String.IsNullOrWhiteSpace(filtri.intestazione))
            //{
            //    richieste = richieste.Where(a => ContainNormalized(a.IntestazioneNDG, filtri.intestazione)).ToList();
            //}
            var datada = new DateUtil().TextToDate(filtri.dataOrdineDA);
            var dataa = new DateUtil().TextToDate(filtri.dataOrdineA);

            //implementare data
            //if ((richieste != null) && (datada != null) && (dataa != null))
            //{
            //    var da = (DateTime)datada;
            //    var a = ((DateTime)dataa).AddDays(1);
            //    richieste = richieste.Where(f => (f.data != null) && (f.DataRichiesta >= da) && (f.DataRichiesta <= a)).ToList();
            //}

            return richieste.OrderBy(a => a.UltimaModifica).ToList();
        }

        public bool ContainNormalized(string a, string b)
        {
            if (!String.IsNullOrWhiteSpace(a)) a = a.Replace(" ", "").ToLower();
            if (!String.IsNullOrWhiteSpace(b)) b = b.Replace(" ", "").ToLower();
            return a.Contains(b);
        }



        public object LeggiUOCode_WL(Guid? fogli_guid)
        {
            int result = 1;
            Fogli foglioDb = null;
            List<colonneUocodeWL> columns = new List<colonneUocodeWL>();
            colonneUocodeWL c;

            if (fogli_guid!=Guid.Empty)
            {
                


                //cancellare vecchi uocode
                //inserire nuovi uocode
                foglioDb = db.Fogli.Include(a => a.UocodeWL)
                    .Where(a => a.Guid == fogli_guid).FirstOrDefault();

                db.UocodeWL.Include(a => a.profilo).FirstOrDefault();
                
                foreach (UocodeWL u in db.UocodeWL)
                {
                    c = new colonneUocodeWL();
                    c.wl_uocode = u.uocode;
                    columns.Add(c);                      
                        
                }
            }

            //return new { result, foglioDb };
            return columns;
        }
        public object AggiornaUocode(string uocode, int profilo, Guid? fogli_guid)
        {
            int result = 1;
            if (!String.IsNullOrWhiteSpace(uocode))
            {
                Fogli foglioDb;
                
               
                //cancellare vecchi uocode
                //inserire nuovi uocode
                foglioDb = db.Fogli.Include(a => a.UocodeWL)                    
                    .Where(a => a.Guid == fogli_guid).FirstOrDefault();
                var prof_wl = db.Profili.Where(a => a.Id== profilo).FirstOrDefault();

                foreach (UocodeWL u in foglioDb.UocodeWL.ToList())
                {
                    db.UocodeWL.Remove(u);
                }

                var objectJson = JsonConvert.DeserializeObject<List< colonneUocodeWL>>(uocode);
               

                foreach (colonneUocodeWL u in objectJson)
                {
                    
                    UocodeWL w = new UocodeWL();
                    w.uocode = u.wl_uocode;
                    w.IsActive = true;
                    w.profilo = prof_wl;
                    foglioDb.UocodeWL.Add(w);

                    //db.UocodeWL.Add(w);
                }
                result = db.SaveChanges();
                
            }

            return new { result, fogli_guid };
        }

    }
}