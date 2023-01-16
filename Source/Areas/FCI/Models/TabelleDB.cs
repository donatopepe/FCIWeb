
using System;
using System.Collections.Generic;

using System.Linq;
using System.Threading.Tasks;

using System.Globalization;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.IO;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Web;
using FCIWeb.Areas.FCI.Uility;
using Elmah;
using DAL.AreaCommonWeb;
using AreaCommonWeb.DAL;
using AreaCommonWeb.Business;

namespace FCIWeb.Areas.FCI.Models
{
    public class ParametriApp
    {
        [Key]
        public int Id { get; set; }
        public string Key { get; set; }
        public string Val { get; set; }
        public string Descrizione { get; set; }
        public bool IsActive { get; set; } = true;
    }
    public class Comune
    {
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid Guid { get; set; }

        [ScaffoldColumn(false)]
        [HiddenInput(DisplayValue = true)]
        [Display(Name = "istID ultima modifica")]
        public int? istIDUltimaModifica { get; set; }
        [ScaffoldColumn(false)]
        [HiddenInput(DisplayValue = true)]
        [Display(Name = "UOCode ultima modifica")]
        public string UOCodeUltimaModifica { get; set; }
        [ScaffoldColumn(false)]
        [HiddenInput(DisplayValue = true)]
        [Display(Name = "Matricola Addetto ultima modifica")]
        public string MatricolaUltimaModifica { get; set; }
        [ScaffoldColumn(false)]
        [Display(Name = "Data Creazione")]
        [HiddenInput(DisplayValue = true)]
        [DisplayFormatAttribute(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        public DateTime? DataCreazione { get; set; }
        [Display(Name = "Ultima Modifica")]
        [HiddenInput(DisplayValue = true)]
        [DisplayFormatAttribute(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        public DateTime? UltimaModifica { get; set; }
        [HiddenInput(DisplayValue = true)]
        [Display(Name = "Modificato da:")]
        public string AddettoUltimaModifica
        {
            get
            {
                var addetto = MatricolaUltimaModifica;
                try
                {
                    if (!String.IsNullOrWhiteSpace(addetto))
                    {
                        addetto = new ResourcesRepository().GetNomeUtente(addetto);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    ErrorSignal.FromCurrentContext().Raise(ex);
                }
                return addetto;
            }
            set { }
        }
        [HiddenInput(DisplayValue = true)]
        [Display(Name = "Gestito da:")]
        public string StrutturaUltimaModifica
        {
            get
            {
                var uocode = UOCodeUltimaModifica;
                try
                {
                    if (!String.IsNullOrWhiteSpace(uocode))
                    {
                        var infoUocode = new ResourcesRepository().GetDimUoByUocode(uocode).FirstOrDefault();
                        //var infoUocode = new CommonBusiness().getUoByText(uocode).FirstOrDefault();
                        if (infoUocode != null)
                        {
                            uocode = infoUocode.DescrizioneUO;
                        }
                        //var descUOcode = new ResourcesRepository().GetUOCodeDesc(uocode, Convert.ToString(istIDUltimaModifica));
                        //if (!String.IsNullOrWhiteSpace(descUOcode)) uocode = descUOcode;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    ErrorSignal.FromCurrentContext().Raise(ex);
                }
                return uocode;
            }
            set { }
        }

        [ScaffoldColumn(false)]
        [HiddenInput(DisplayValue = false)]
        public long RowVersion { get; set; }
    }

    //public class Cartelle : Comune
    //{
    //    public Cartelle()
    //    {
    //        fogli = new HashSet<Fogli>();
    //    }

    //    public string name { get; set; }
    //    public virtual ICollection<Fogli> fogli { get; set; }
    //}
    public class Profili
    {
        [Key]
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Descrizione { get; set; }
        public bool IsActive { get; set; } = true;

        public bool Compilazione { get; set; } = false;
        public bool ModificaLista { get; set; } = false;
        public bool Esportazione { get; set; } = false;
        public bool SoloPubbliche { get; set; } = true;
        public bool TutteVisibili { get; set; } = false;
    }
    public class ProfiliWL
    {
        [Key]
        public long Id { get; set; }
        public string uocodematricola { get; set; }
        public bool IsUocode { get; set; } = true;
        public Profili profilo { get; set; }
        public bool IsActive { get; set; } = true;
        public int? istID { get; set; }
        public bool Notificata { get; set; } = false;

        [HiddenInput(DisplayValue = true)]
        [Display(Name = "Modificato da:")]
        public string descrizione
        {
            get
            {
                if (IsUocode)
                {
                    var uocode = uocodematricola;
                    try
                    {
                        if (!String.IsNullOrWhiteSpace(uocode))
                        {
                            var infoUocode = new ResourcesRepository().GetDimUoByUocode(uocode).FirstOrDefault();
                            //var infoUocode = new CommonBusiness().getUoByText(uocode).FirstOrDefault();
                            if (infoUocode != null)
                            {
                                uocode = infoUocode.DescrizioneUO;
                            }
                            //var descUOcode = new ResourcesRepository().GetUOCodeDesc(uocode, Convert.ToString(istID));
                            //if (!String.IsNullOrWhiteSpace(descUOcode)) uocode = descUOcode;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                        ErrorSignal.FromCurrentContext().Raise(ex);
                    }
                    return uocode;
                }
                else
                {
                    var addetto = uocodematricola;
                    try
                    {
                        if (!String.IsNullOrWhiteSpace(addetto))
                        {
                            addetto = new ResourcesRepository().GetNomeUtente(addetto);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                        ErrorSignal.FromCurrentContext().Raise(ex);
                    }
                    return addetto;
                }

            }
            set { }
        }
    }

    public class StatiPossibili
    {
        [Key]
        public int Id { get; set; }
        public int Stato { get; set; }
    }


    public class Stati
    {
        public Stati()
        {
            StatiPossibili = new HashSet<StatiPossibili>();
        }

        [Key]
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Descrizione { get; set; }
        public bool IsActive { get; set; } = true;
        public bool Pubblica { get; set; } = true;
        public bool Modificabile { get; set; } = true;
        public bool Cancellabile { get; set; } = false;
        public bool ObbligoMotivazione { get; set; } = false;
        public ICollection<StatiPossibili> StatiPossibili { get; set; }
    }

    public class LogStati
    {
        [Key]
        public long Id { get; set; }
        public Stati stato { get; set; }
        [Display(Name = "Motivazione")]
        public string Motivazione { get; set; }
        [ScaffoldColumn(false)]
        [HiddenInput(DisplayValue = true)]
        [Display(Name = "istID ultima modifica")]
        public int? istIDUltimaModifica { get; set; }
        [ScaffoldColumn(false)]
        [HiddenInput(DisplayValue = true)]
        [Display(Name = "UOCode ultima modifica")]
        public string UOCodeUltimaModifica { get; set; }
        [ScaffoldColumn(false)]
        [HiddenInput(DisplayValue = true)]
        [Display(Name = "Matricola Addetto ultima modifica")]
        public string MatricolaUltimaModifica { get; set; }
        [ScaffoldColumn(false)]
        [Display(Name = "Ultima Modifica")]
        [HiddenInput(DisplayValue = true)]
        [DisplayFormatAttribute(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        public DateTime? UltimaModifica { get; set; }
        [HiddenInput(DisplayValue = true)]
        [Display(Name = "Modificato da:")]
        public string AddettoUltimaModifica
        {
            get
            {
                var addetto = MatricolaUltimaModifica;
                try
                {
                    if (!String.IsNullOrWhiteSpace(addetto))
                    {
                        addetto = new ResourcesRepository().GetNomeUtente(addetto);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    ErrorSignal.FromCurrentContext().Raise(ex);
                }
                return addetto;
            }
            set { }
        }
        [HiddenInput(DisplayValue = true)]
        [Display(Name = "Modificato da:")]
        public string StrutturaUltimaModifica
        {
            get
            {
                var uocode = UOCodeUltimaModifica;
                try
                {
                    if (!String.IsNullOrWhiteSpace(uocode))
                    {
                        var infoUocode = new ResourcesRepository().GetDimUoByUocode(uocode).FirstOrDefault();
                        //var infoUocode = new CommonBusiness().getUoByText(uocode).FirstOrDefault();
                        if (infoUocode != null)
                        {
                            uocode = infoUocode.DescrizioneUO;
                        }
                        //var descUOcode = new ResourcesRepository().GetUOCodeDesc(uocode, Convert.ToString(istIDUltimaModifica));
                        //if (!String.IsNullOrWhiteSpace(descUOcode)) uocode = descUOcode;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    ErrorSignal.FromCurrentContext().Raise(ex);
                }
                return uocode;
            }
            set { }
        }

    }

    public class column
    {
        public string title { get; set; }
        public string type { get; set; } = "text";
        public string source { get; set; }
        public string width { get; set; }
        public bool readOnly { get; set; } = false;
    }

    public class Colonne
    {
        [Key]
        public Guid Guid { get; set; }
        public int numColonna { get; set; }
        public column column { get; set; } = new column();
    }

    public class Fogli : Comune
    {
        public Fogli()
        {
            Righe = new HashSet<Righe>();
            ProfiliWL = new HashSet<ProfiliWL>();
            LogStati = new HashSet<LogStati>();
            ProprietaColonne = new HashSet<Colonne>();

        }

        public string NomeLista { get; set; }
        public string NomeFile { get; set; }
        public string StileFoglio { get; set; }
        public ICollection<Colonne> ProprietaColonne { get; set; }
        public Stati Stato { get; set; }
        //public string json { get; set; }
        public virtual ICollection<Righe> Righe { get; set; }
        public virtual ICollection<ProfiliWL> ProfiliWL { get; set; }
        public virtual ICollection<LogStati> LogStati { get; set; }



        [ScaffoldColumn(false)]
        [HiddenInput(DisplayValue = true)]
        [Display(Name = "istID proprietario")]
        public int? istID { get; set; }
        [ScaffoldColumn(false)]
        [HiddenInput(DisplayValue = true)]
        [Display(Name = "UOCode creatore")]
        public string UOCode { get; set; }
        [ScaffoldColumn(false)]
        [HiddenInput(DisplayValue = true)]
        [Display(Name = "Matricola creatore")]
        public string Matricola { get; set; }
        [HiddenInput(DisplayValue = true)]
        [Display(Name = "Creato da:")]
        public string Addetto
        {
            get
            {
                var addetto = Matricola;
                try
                {
                    if (!String.IsNullOrWhiteSpace(addetto))
                    {
                        addetto = new ResourcesRepository().GetNomeUtente(addetto);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    ErrorSignal.FromCurrentContext().Raise(ex);
                }
                return addetto;
            }
            set { }
        }
        [HiddenInput(DisplayValue = true)]
        [Display(Name = "Creato da:")]
        public string Struttura
        {
            get
            {
                var uocode = UOCode;
                try
                {
                    if (!String.IsNullOrWhiteSpace(uocode))
                    {
                        var infoUocode = new ResourcesRepository().GetDimUoByUocode(uocode).FirstOrDefault();
                        //var infoUocode = new CommonBusiness().getUoByText(uocode).FirstOrDefault();
                        if (infoUocode != null)
                        {
                            uocode = infoUocode.DescrizioneUO;
                        }
                        //var descUOcode = new ResourcesRepository().GetUOCodeDesc(uocode, Convert.ToString(istID));
                        //if (!String.IsNullOrWhiteSpace(descUOcode)) uocode = descUOcode;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    ErrorSignal.FromCurrentContext().Raise(ex);
                }
                return uocode;
            }
            set { }
        }

    }
    public class Righe : Comune
    {
        public int numRiga { get; set; }
        public string riga { get; set; } = "";
    }
}
