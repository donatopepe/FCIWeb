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
using DAL.AreaCommonWeb;
using AreaCommonWeb.ModelView.Security;
using AreaCommonWeb.Models;
using AreaCommonWeb.Business;
using AreaCommonWeb.DAL;
using Elmah;
using System.Configuration;

namespace FCIWeb.Areas.FCI.Repository
{
    public partial class FCIRepository : IDisposable
    {
        private ResourcesRepository resourcesRepository = new ResourcesRepository();
        private FCIContext db = new FCIContext();
        private bool disposed = false;
        public char separator = '|';
        private IEnumerable<EmailSettings> emailSettings;
        private IEnumerable<EmailTesti> EmailTesti;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    db.Dispose();
                }
            }
            this.disposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public string myUocode = "";
        public string flagUffRuolo = ""; //////non credo serva
        public string filtroIst = "";
        public int? myIstID = null;
        public string myMatricola = "";
        private string codRuolo;

        public FCIRepository()
        {
            UserProfile myProfile = new UserProfileManager().GetCurrent();

            if (myProfile != null)
            {
                codRuolo = myProfile.Utenza.CodRuolo;
                myUocode = UserProfileManager.GetFiltroSicurezza(myProfile, "codUnitRuolo");
                flagUffRuolo = UserProfileManager.GetFiltroSicurezza(myProfile, "flagUffRuolo"); //////non credo serva
                filtroIst = UserProfileManager.GetFiltroSicurezza(myProfile, "IST");
                myIstID = !String.IsNullOrEmpty(filtroIst) ? Convert.ToInt32(filtroIst) : Convert.ToInt32(myProfile.UserInfo.CodIstituto);
                myMatricola = myProfile.UserInfo.Matricola;
            }
            //codRuolo = "pbl";
            //myUocode = "12345";
            //myMatricola = "12345";
            this.emailSettings = db.EmailSettings.Where(a => a.isActive).ToList();
            this.EmailTesti = db.EmailTesti.Where(a => a.IsActive).ToList();
        }

        public Guid Elimina(Guid guid)
        {
            if (guid == Guid.Empty) return guid;
            var foglio = getFoglio(guid);
            if (foglio == null) return guid;
            db.Fogli.Remove(foglio);
            db.ChangeTracker.DetectChanges();
            db.SaveChanges();
            return Guid.Empty;
        }

        public IEnumerable<LogStati> GetLogStati(Guid Id)
        {


            var result = db.Fogli
                 .Include(l => l.LogStati)
                 .Include(d => d.LogStati.Select(e => e.stato))
                 .FirstOrDefault(b => b.Guid == Id).LogStati
                 .OrderBy(h => h.UltimaModifica);
            return result.ToList();
        }

        public IEnumerable<Fogli> getFogli()
        {
            var result = db.Fogli.ToList();
            return result;
        }

        private CommonBusiness commonBusiness = new CommonBusiness();

        private bool Autorizzato(Fogli foglio)
        {
            if (!string.IsNullOrWhiteSpace(codRuolo))
            {
                var profilo = GetProfilo(codRuolo);
                if (profilo != null && (!profilo.SoloPubbliche || profilo.SoloPubbliche == foglio.Stato.Pubblica))
                {
                    if (profilo.TutteVisibili)
                    {
                        return true;
                    }
                    else
                    {
                        //foreach (var wl in foglio.ProfiliWL)
                        //{
                        //    if (wl.IsUocode)
                        //    {
                        //        if (EqualsNormalized(wl.uocodematricola, myUocode)) return true;
                        //    }
                        //    else
                        //    {
                        //        if (EqualsNormalized(wl.uocodematricola, myMatricola)) return true;
                        //    }
                        //}
                        if (EqualsNormalized(foglio.Matricola, myMatricola) || EqualsNormalized(foglio.UOCode, myUocode)) return true;
                    }
                }
            }
            foreach (var wl in foglio.ProfiliWL)
            {
                if (wl.profilo!=null && (!wl.profilo.SoloPubbliche || wl.profilo.SoloPubbliche == foglio.Stato.Pubblica))
                {
                    if (wl.IsUocode)
                    {
                        if (EqualsNormalized(wl.uocodematricola, myUocode)) return true;
                    }
                    else
                    {
                        if (EqualsNormalized(wl.uocodematricola, myMatricola)) return true;

                    }
                }
            }
            return false;
        }
        public bool EqualsNormalized(string a, string b)
        {
            if (!String.IsNullOrWhiteSpace(a)) a = a.Replace(" ", "").ToLower();
            if (!String.IsNullOrWhiteSpace(b)) b = b.Replace(" ", "").ToLower();
            return a == b;
        }
        public bool ContainNormalized(string a, string b)
        {
            if (!String.IsNullOrWhiteSpace(a)) a = a.Replace(" ", "").ToLower();
            if (!String.IsNullOrWhiteSpace(b)) b = b.Replace(" ", "").ToLower();
            return a.Contains(b);
        }
        public IEnumerable<Fogli> getFogli(FiltriRichiesteModel filtri, bool filtraperUocode)
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
            //var richieste = db.Richieste
            //    .Include(a => a.Stato)
            //    .Include(b => b.Stati)
            //    .Where(d => ((d.Stato.IsActive) && (d.Istituto == istituto) && ((d.UOCode == uocodedaricercare) || (d.UOCodeNext == uocodedaricercare))));

            var richieste = db.Fogli
                .Include(a => a.Stato)
                .Include(b => b.ProfiliWL)
                .Include(d => d.ProfiliWL.Select(e => e.profilo))
                .Where(d => ((d.Stato.IsActive) && (d.istID == istituto))).ToList();

            if (filtraperUocode)
                richieste = richieste.Where(d => Autorizzato(d)).ToList();

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
            var datada = new DateUtil().TextToDate(filtri.dataOrdineDA);
            var dataa = new DateUtil().TextToDate(filtri.dataOrdineA);
            if ((richieste != null) && (datada != null) && (dataa != null))
            {
                var da = (DateTime)datada;
                var a = ((DateTime)dataa).AddDays(1);
                richieste = richieste.Where(f => (f.UltimaModifica != null) && (f.UltimaModifica >= da) && (f.UltimaModifica <= a)).ToList();
            }

            return richieste.OrderBy(a => a.UltimaModifica).ToList();
        }
        public IEnumerable<Stati> GetStati()
        {
            return db.Stati.Include(a => a.StatiPossibili).ToList();
        }
        public Stati GetStato(int id)
        {
            return db.Stati.Include(a => a.StatiPossibili).Where(a=>a.Id==id).FirstOrDefault();
        }
        public IEnumerable<Stati> GetStati(Guid guid)
        {
            var foglio = getFoglio((Guid)guid);
            var statoattuale = foglio.Stato;
            var result = new List<Stati>();
            result.Add(statoattuale);
            foreach (var item in statoattuale.StatiPossibili)
            {
                result.Add(GetStato(item.Stato));
            }
            return result;
        }

        public class ExcelCellWithType
        {
            public string Value { get; set; }
            public string CellReference { get; set; }
            public string CellFormula { get; set; }
            public int? ColumnIndex { get; set; }
            public int? RowIndex { get; set; }
            public UInt32Value ExcelCellFormat { get; set; }
            public bool IsDateTimeType { get; set; }
        }
        private static int? GetColumnIndex(string cellReference)
        {
            if (string.IsNullOrEmpty(cellReference))
            {
                return null;
            }

            //remove digits
            string columnReference = Regex.Replace(cellReference.ToUpper(), @"[\d]", string.Empty);
            if (string.IsNullOrEmpty(columnReference))
            {
                return null;
            }

            int columnNumber = -1;
            int mulitplier = 1;

            //working from the end of the letters take the ASCII code less 64 (so A = 1, B =2...etc)
            //then multiply that number by our multiplier (which starts at 1)
            //multiply our multiplier by 26 as there are 26 letters
            foreach (char c in columnReference.ToCharArray().Reverse())
            {
                columnNumber += mulitplier * ((int)c - 64);

                mulitplier = mulitplier * 26;
            }

            //the result is zero based so return columnnumber + 1 for a 1 based answer
            //this will match Excel's COLUMN function
            return columnNumber + 1;
        }
        private static int? GetRowIndex(string cellReference)
        {
            if (string.IsNullOrEmpty(cellReference))
            {
                return null;
            }


            string rowReference = Regex.Replace(cellReference, @"[^\d]", string.Empty);

            if (String.IsNullOrWhiteSpace(rowReference)) return null;

            return Convert.ToInt32(rowReference);
        }
        private ExcelCellWithType ReadExcelCell(Cell cell, WorkbookPart workbookPart)
        {
            var cellValue = cell.CellValue;
            var text = (cellValue == null) ? cell.InnerText : cellValue.Text;
            if (cell.DataType?.Value == CellValues.SharedString)
            {
                text = workbookPart.SharedStringTablePart.SharedStringTable
                    .Elements<SharedStringItem>().ElementAt(
                        Convert.ToInt32(cell.CellValue.Text)).InnerText;
            }

            var cellText = (text ?? string.Empty).Trim();

            var cellWithType = new ExcelCellWithType();
            cellWithType.CellReference = cell.CellReference;
            cellWithType.ColumnIndex = GetColumnIndex(cell.CellReference);
            cellWithType.RowIndex = GetRowIndex(cell.CellReference);
            cellWithType.CellFormula = (cell.CellFormula == null) ? string.Empty : cell.CellFormula.InnerText;


            if (cell.StyleIndex != null)
            {
                var cellFormat = workbookPart.WorkbookStylesPart.Stylesheet.CellFormats.ChildElements[
                    int.Parse(cell.StyleIndex.InnerText)] as CellFormat;

                if (cellFormat != null)
                {
                    cellWithType.ExcelCellFormat = cellFormat.NumberFormatId;

                    var dateFormat = GetDateTimeFormat(cellFormat.NumberFormatId);
                    if (!string.IsNullOrEmpty(dateFormat))
                    {
                        cellWithType.IsDateTimeType = true;

                        if (!string.IsNullOrEmpty(cellText))
                        {
                            if (double.TryParse(cellText, out var cellDouble))
                            {
                                var theDate = DateTime.FromOADate(cellDouble);
                                cellText = theDate.ToString("yyyy-MM-dd"); //theDate.ToString(dateFormat);
                            }
                        }
                    }
                }


            }

            cellWithType.Value = cellText;

            return cellWithType;
        }
        //// https://msdn.microsoft.com/en-GB/library/documentformat.openxml.spreadsheet.numberingformat(v=office.14).aspx
        private readonly Dictionary<uint, string> DateFormatDictionary = new Dictionary<uint, string>()
        {
            [14] = "dd/MM/yyyy",
            [15] = "d-MMM-yy",
            [16] = "d-MMM",
            [17] = "MMM-yy",
            [18] = "h:mm AM/PM",
            [19] = "h:mm:ss AM/PM",
            [20] = "h:mm",
            [21] = "h:mm:ss",
            [22] = "M/d/yy h:mm",
            [30] = "M/d/yy",
            [34] = "yyyy-MM-dd",
            [45] = "mm:ss",
            [46] = "[h]:mm:ss",
            [47] = "mmss.0",
            [51] = "MM-dd",
            [52] = "yyyy-MM-dd",
            [53] = "yyyy-MM-dd",
            [55] = "yyyy-MM-dd",
            [56] = "yyyy-MM-dd",
            [58] = "MM-dd",
            [165] = "M/d/yy",
            [166] = "dd MMMM yyyy",
            [167] = "dd/MM/yyyy",
            [168] = "dd/MM/yy",
            [169] = "d.M.yy",
            [170] = "yyyy-MM-dd",
            [171] = "dd MMMM yyyy",
            [172] = "d MMMM yyyy",
            [173] = "M/d",
            [174] = "M/d/yy",
            [175] = "MM/dd/yy",
            [176] = "d-MMM",
            [177] = "d-MMM-yy",
            [178] = "dd-MMM-yy",
            [179] = "MMM-yy",
            [180] = "MMMM-yy",
            [181] = "MMMM d, yyyy",
            [182] = "M/d/yy hh:mm t",
            [183] = "M/d/y HH:mm",
            [184] = "MMM",
            [185] = "MMM-dd",
            [186] = "M/d/yyyy",
            [187] = "d-MMM-yyyy"
        };

        public object getProfiliWL(Guid guid)
        {
            var foglio = db.Fogli.Include(a => a.ProfiliWL).Include(d => d.ProfiliWL.Select(e => e.profilo)).Where(a => a.Guid == guid).FirstOrDefault();

            var ProfiliWL = foglio.ProfiliWL.ToList();
            return new
            {
                ProfiliWL = ProfiliWL,
                RowVersion = Convert.ToString(foglio.RowVersion),
            };
        }

        public object getprofiliAvviabili()
        {
            return db.Profili.Where(a => a.IsActive).ToList();
        }

        private string GetDateTimeFormat(UInt32Value numberFormatId)
        {
            return DateFormatDictionary.ContainsKey(numberFormatId) ? DateFormatDictionary[numberFormatId] : string.Empty;
        }

        //private class cell
        //{
        //    public string text { get; set; }
        //}

        //private class row
        //{
        //    public Dictionary<string, cell> cells { get; set; } = new Dictionary<string, cell>();
        //}

        //private class sheet
        //{
        //    public string name { get; set; }
        //    public Dictionary<string, row> rows { get; set; } = new Dictionary<string, row>();
        //}


        //private int Write(ModelFileForm fileForm)
        //{
        //    try
        //    {
        //        Stream fileExcel = new MemoryStream(fileForm.FileContent);



        //        //Lets open the existing excel file and read through its content . Open the excel using openxml sdk
        //        using (SpreadsheetDocument doc = SpreadsheetDocument.Open(fileExcel, false))
        //        {
        //            //create the object for workbook part  
        //            WorkbookPart workbookPart = doc.WorkbookPart;
        //            Sheets thesheetcollection = workbookPart.Workbook.GetFirstChild<Sheets>();

        //            var cartella = new Cartelle { name = fileForm.FileName };

        //            //using for each loop to get the sheet from the sheetcollection  
        //            foreach (Sheet thesheet in thesheetcollection.OfType<Sheet>())
        //            {
        //                DataTable dtTable = new DataTable();
        //                var foglio = new Fogli { name = thesheet.Name };

        //                var foglioC = new sheet { name = thesheet.Name };

        //                //statement to get the worksheet object by using the sheet id  
        //                Worksheet theWorksheet = ((WorksheetPart)workbookPart.GetPartById(thesheet.Id)).Worksheet;

        //                SheetData thesheetdata = theWorksheet.GetFirstChild<SheetData>();

        //                List<Row> rows = thesheetdata.Elements<Row>().Where(row => row.Elements<Cell>().Any(cell => cell.DataType != null)).ToList();   ///codice da implementare se pulire righe vuote

        //                for (int rigaCnt = 0; rigaCnt < thesheetdata.ChildElements.Count(); rigaCnt++)
        //                {
        //                    StringBuilder _StringBuilder = new StringBuilder();
        //                    List<string> rowList = new List<string>();
        //                    var rigaC = new row();
        //                    for (int colonnaCnt = 0; colonnaCnt < thesheetdata.ElementAt(rigaCnt).ChildElements.Count(); colonnaCnt++)
        //                    {
        //                        string Value = string.Empty;
        //                        Cell thecurrentcell = (Cell)thesheetdata.ElementAt(rigaCnt).ChildElements.ElementAt(colonnaCnt);
        //                        //statement to take the integer value  
        //                        string currentcellvalue = string.Empty;

        //                        var cellWithType = ReadExcelCell(thecurrentcell, workbookPart);

        //                        //if (thecurrentcell.DataType != null)
        //                        //{
        //                        //    //if (thecurrentcell.DataType == CellValues.String)
        //                        //    //{
        //                        //    //    Value = "String:" + thecurrentcell.InnerText;
        //                        //    //}
        //                        //    //else
        //                        //    //if (thecurrentcell.DataType == CellValues.Boolean)
        //                        //    //{
        //                        //    //    Value = "Boolean:" + thecurrentcell.InnerText;
        //                        //    //}
        //                        //    //else
        //                        //    //if (thecurrentcell.DataType == CellValues.Date)
        //                        //    //{
        //                        //    //    Value = "Date:" + thecurrentcell.InnerText;
        //                        //    //}
        //                        //    //else
        //                        //    if (thecurrentcell.DataType == CellValues.SharedString)
        //                        //    {


        //                        //        int id;
        //                        //        if (Int32.TryParse(thecurrentcell.InnerText, out id))
        //                        //        {
        //                        //            SharedStringItem item = workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ElementAt(id);
        //                        //            if (item.Text != null)
        //                        //            {
        //                        //                //first row will provide the column name.
        //                        //                if (rigaCnt == 0)
        //                        //                {
        //                        //                    dtTable.Columns.Add(item.Text.Text);
        //                        //                }
        //                        //                else
        //                        //                {
        //                        //                    rowList.Add(item.Text.Text);
        //                        //                }
        //                        //                Value = item.Text.Text;
        //                        //            }
        //                        //            else if (item.InnerText != null)
        //                        //            {
        //                        //                Value = currentcellvalue = item.InnerText;

        //                        //            }
        //                        //            else if (item.InnerXml != null)
        //                        //            {
        //                        //                Value =  currentcellvalue = "Xml:" + item.InnerXml;
        //                        //            }
        //                        //        }
        //                        //    }
        //                        //}
        //                        //else
        //                        //{
        //                        //    if (rigaCnt != 0)//reserved for column values
        //                        //    {
        //                        //        Value = thecurrentcell.InnerText;
        //                        //        rowList.Add(Value);
        //                        //    }
        //                        //}

        //                        Value = cellWithType.Value;
        //                        //if (rigaCnt == 0)
        //                        //{
        //                        //    dtTable.Columns.Add(Value);
        //                        //}
        //                        //else
        //                        //{
        //                        //    rowList.Add(Value);
        //                        //}
        //                        rowList.Add(Value);
        //                        if (rowList.Count > dtTable.Columns.Count) dtTable.Columns.Add(Convert.ToString(colonnaCnt));
        //                        _StringBuilder.Append(string.Format("{0},", Value.Trim()));

        //                        if (!String.IsNullOrWhiteSpace(Value))
        //                        {
        //                            var cellaC = new cell { text = Value };
        //                            rigaC.cells.Add(Convert.ToString(colonnaCnt), cellaC);
        //                        }

        //                    }

        //                    foglioC.rows.Add(Convert.ToString(rigaCnt), rigaC);

        //                    //if (rigaCnt != 0)//reserved for column values
        //                        dtTable.Rows.Add(rowList.ToArray());

        //                    var riga = new Righe { riga = _StringBuilder.ToString().TrimEnd(',') };

        //                    foglio.righe.Add(riga);
        //                }


        //                //foglio.json = JsonConvert.SerializeObject(foglioC);//JsonConvert.SerializeObject(dtTable);

        //                cartella.fogli.Add(foglio);


        //            }

        //            db.Cartella.Add(cartella);
        //            return db.SaveChanges();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //        throw;
        //    }
        //}

        public Guid Write(ModelFileForm fileForm, string nome)
        {
            int result = 1;
            Guid guid = Guid.Empty;
            try
            {
                Stream fileExcel = new MemoryStream(fileForm.FileContent);

                using (SpreadsheetDocument doc = SpreadsheetDocument.Open(fileExcel, false))
                {

                    WorkbookPart workbookPart = doc.WorkbookPart;
                    Sheets thesheetcollection = workbookPart.Workbook.GetFirstChild<Sheets>();

                    Sheet thesheet = thesheetcollection.OfType<Sheet>().FirstOrDefault();

                    var foglio = getFoglio();
                    foglio.NomeFile = fileForm.FileName;
                    foglio.NomeLista = nome;

                    Worksheet theWorksheet = ((WorksheetPart)workbookPart.GetPartById(thesheet.Id)).Worksheet;

                    SheetData thesheetdata = theWorksheet.GetFirstChild<SheetData>();

                    List<Row> rows = thesheetdata.Elements<Row>().Where(row => row.Elements<Cell>().Any(cell => cell.DataType != null)).ToList();

                    for (int rigaCnt = 0; rigaCnt < rows.Count(); rigaCnt++)
                    {

                        var valori = new Dictionary<int, string>();
                        for (int colonnaCnt = 0; colonnaCnt < rows.ElementAt(rigaCnt).ChildElements.Count(); colonnaCnt++)
                        {
                            string Value = string.Empty;
                            Cell thecurrentcell = (Cell)rows.ElementAt(rigaCnt).ChildElements.ElementAt(colonnaCnt);
                            string currentcellvalue = string.Empty;
                            var cellWithType = ReadExcelCell(thecurrentcell, workbookPart);
                            Value = cellWithType.Value.Replace(separator.ToString(), "");
                            if (cellWithType.ColumnIndex != null) valori.Add(cellWithType.ColumnIndex.Value, Value);
                        }

                        StringBuilder _StringBuilder = new StringBuilder();
                        for (int colonnaCnt = 0; colonnaCnt < valori.Keys.Max(); colonnaCnt++)
                        {
                            string Value = string.Empty;
                            if (valori.Keys.Contains(colonnaCnt + 1)) Value = valori[colonnaCnt + 1];

                            if (rigaCnt > 0)
                            {
                                _StringBuilder.Append(string.Format("{0}" + separator, Value.Trim()));
                            }
                            else
                            {
                                foglio.ProprietaColonne.Add(new Colonne
                                {
                                    numColonna = colonnaCnt+1,
                                    column = new column
                                    {
                                        title = Value.Trim(),
                                        type = "text",
                                        readOnly = false,
                                        source = string.Empty,
                                        width = string.Empty,
                                    },
                                });
                            }
                            
                        }

                        if (rigaCnt > 0)
                        {
                            var riga = new Righe { numRiga = rigaCnt, riga = _StringBuilder.ToString().TrimEnd(separator) };
                            foglio.Righe.Add(riga);
                        }
                    }

                    db.Fogli.Add(foglio);
                    db.ChangeTracker.DetectChanges();
                    result = db.SaveChanges();
                    guid = foglio.Guid;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            return guid;
        }
        public void Notificare(Fogli foglio)
        {
            if (EqualsNormalized(foglio.Stato.Nome, "Pubblicata"))
            {
                var emailtesto = db.EmailTesti.ToList().Where(a => a.IsActive && EqualsNormalized(foglio.Stato.Nome, a.Nome)).FirstOrDefault();
                SendEmail(foglio, emailtesto);
            }
            else
            {
                foreach (var item in foglio.ProfiliWL)
                {
                    item.Notificata = false;
                }
            }
        }
        public void UpdateLogStati(Fogli foglio, string NomeStato, string motivazione)
        {
            if (foglio != null)
            {
                if (foglio.Stato == null || foglio.Stato.Nome.ToLower() != NomeStato.Trim().ToLower())
                {
                    foglio.Stato = db.Stati.Include(d => d.StatiPossibili).Where(a => a.Nome.ToLower() == NomeStato.Trim().ToLower()).FirstOrDefault();
                    var newlog = new LogStati
                    {
                        stato = foglio.Stato,
                        Motivazione=motivazione,
                        istIDUltimaModifica = myIstID,
                        UOCodeUltimaModifica = myUocode,
                        MatricolaUltimaModifica = myMatricola,
                        UltimaModifica = DateTime.Now,
                    };
                    foglio.LogStati.Add(newlog);
                    db.ChangeTracker.DetectChanges();
                    db.SaveChanges();
                    Notificare(foglio);
                }
            }
        }
        public Fogli getFoglio(Guid? guid = null)
        {
            Fogli foglio = null;
            if (guid != null && guid != Guid.Empty)
            {
                foglio = db.Fogli.Include(a => a.ProfiliWL).Include(d => d.ProfiliWL.Select(e => e.profilo)).Include(a => a.Stato).Include(d => d.Stato.StatiPossibili).Include(a => a.LogStati).Include(a => a.Righe).Include(a => a.ProprietaColonne).Where(a => a.Guid == guid).FirstOrDefault();
            }
            if (foglio == null || guid == null || guid == Guid.Empty)
            {
                foglio = new Fogli();
                foglio.istID = myIstID;
                foglio.Matricola = myMatricola;
                foglio.UOCode = myUocode;
                var profilo = GetProfilo(codRuolo);
                if (profilo != null && profilo.IsActive) foglio.ProfiliWL.Add(new ProfiliWL { istID = myIstID, uocodematricola = myMatricola, IsActive = true, IsUocode = false, profilo = profilo });
            }
            if (foglio != null && foglio.Stato == null) UpdateLogStati(foglio, "bozze", "Primo inserimento");
            return foglio;
        }

        public class optionsJExcel
        {
            //options : {
            //    // Date format
            //    format:'DD/MM/YYYY',
            //    // Allow keyboard date entry
            //    readonly:0,
            //    // Today is default
            //    today:0,
            //    // Show timepicker
            //    time:0,
            //    // Show the reset button
            //    resetButton:true,
            //    // Placeholder
            //    placeholder:'',
            //    // Translations can be done here
            //    months:['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
            //    weekdays:['Sunday','Monday','Tuesday','Wednesday','Thursday','Friday','Saturday'],
            //    weekdays_short:['S', 'M', 'T', 'W', 'T', 'F', 'S'],
            //    // Value
            //    value:null,
            //    // Events
            //    onclose:null,
            //    onchange:null,
            //    // Fullscreen (this is automatic set for screensize < 800)
            //    fullscreen:false,
            //};
            public string format { get; set; }
        }

        public class columnJExcel
        {
            public optionsJExcel options { get; set; }
            public string mask { get; set; }
            public string sep_deci { get; set; }
            public string title { get; set; }
            public string type { get; set; } = "text";
            public List<string> source { get; set; } = new List<string>();
            public string width { get; set; }
            public bool readOnly { get; set; } = false;
        }

        public class tipoJExcel
        {
            public Guid guid { get; set; }
            public string NomeLista { get; set; }
            public List<List<object>> data { get; set; } = new List<List<object>>();
            public List<columnJExcel> columns { get; set; } = new List<columnJExcel>();
            public List<string> rowversions { get; set; } = new List<string>();
            public string style { get; set; }
            public int maxcol { get; set; }
        }

        public tipoJExcel getFoglioJExcel(Guid? guid)
        {

            
            var foglio = getFoglio((Guid)guid);

            if (foglio.Righe.Count <= 0) foglio.Righe.Add(new Righe { numRiga = 1 });
            if (foglio == null)
            {
                return null;
            }
            var righe = foglio.Righe.OrderBy(a => a.numRiga).ToList();

            var jexcel = new tipoJExcel();

            var maxColonne = 0;
            if (righe != null && righe.Count > 0)
            {
                for (var i = 1; i <= righe.Max(a => a.numRiga); i++)
                {

                    var riga = righe.Where(a => a.numRiga == i).FirstOrDefault();
                    var elem = String.Empty;
                    if (riga != null && !String.IsNullOrWhiteSpace(riga.riga)) elem = riga.riga;
                    var rigaSplit = elem.Split(separator).ToList();
                    if (rigaSplit.Count > maxColonne) maxColonne = rigaSplit.Count;
                    //jexcel.data.Add(rigaSplit);
                    var dati = new List<object>();
                    foreach (var value in rigaSplit)
                    {
                        double num;
                        if (double.TryParse(value.Replace(".", ","), out num))
                        {
                            dati.Add(value.Replace(".", ","));
                        }
                        else
                        {
                            dati.Add(value);
                        }
                    }
                    jexcel.data.Add(dati);
                }
            }

            var colonne = foglio.ProprietaColonne.ToList();
            if (colonne != null && colonne.Count > 0)
            {
                var maxcol = colonne.Max(a => a.numColonna);
                for (var i = 1; i <= maxcol; i++)
                {
                    var colonna = colonne.Where(a => a.numColonna == i).FirstOrDefault();
                    if (colonna != null)
                    {
                        var source = new List<string>();
                        if (colonna.column.source != null)
                            source = JsonConvert.DeserializeObject<List<string>>(colonna.column.source);
                        var mask = string.Empty;
                        var decimale = string.Empty;
                        optionsJExcel options = new optionsJExcel();
                        if (colonna.column.type == "number")
                        {
                            mask = "#.##0,00";
                            decimale = ",";
                        }
                        if (colonna.column.type == "calendar")
                        {
                            mask = "dd/mm/yyyy";
                            options.format = "DD/MM/YYYY";
                        }
                        jexcel.columns.Add(new columnJExcel()
                        {
                            title = colonna.column.title,
                            type = colonna.column.type,
                            source = source,
                            mask = mask,
                            sep_deci = decimale,
                            options = options,
                            width = colonna.column.width,
                            readOnly = colonna.column.readOnly,
                        });
                    }
                    else
                    {
                        jexcel.columns.Add(new columnJExcel());
                    }
                }
            }

            jexcel.style = foglio.StileFoglio;
            jexcel.rowversions = righe.Select(a => Convert.ToString(a.RowVersion)).ToList();
            jexcel.guid = foglio.Guid;
            jexcel.maxcol = maxColonne;
            jexcel.NomeLista = foglio.NomeLista;
            return jexcel;
        }

        public object getColonne(Guid guid)
        {
            var foglio = getFoglio((Guid)guid);
            var colonne = foglio.ProprietaColonne.ToList();
            List<colonneJson> columns = new List<colonneJson>();
            if (colonne != null && colonne.Count > 0)
            {
                var maxcol = colonne.Max(a => a.numColonna);
                for (var i = 1; i <= maxcol; i++)
                {
                    var colonna = colonne.Where(a => a.numColonna == i).FirstOrDefault();
                    if (colonna != null)
                    {
                        var sourceString = String.Empty;
                        if (!String.IsNullOrWhiteSpace(colonna.column.source))
                        {
                            var source = JsonConvert.DeserializeObject<List<string>>(colonna.column.source);
                            sourceString = String.Join(",", source.ToArray());
                        }

                        var tipo = String.Empty;
                        if (colonna.column.readOnly)
                        {
                            tipo = "readOnly";
                        }
                        else
                        {
                            tipo = colonna.column.type;
                        }
                        var tipocolonna = new colonnaJson
                        {
                            type = tipo,
                            source = sourceString,
                        };
                        columns.Add(new colonneJson()
                        {
                            name = colonna.column.title,
                            width = colonna.column.width,
                            tipo = tipocolonna,
                        });
                    }
                    else
                    {
                        columns.Add(new colonneJson { tipo = new colonnaJson { type = "text" } });
                    }
                }
            }
            return new
            {
                columns = columns,
                RowVersion = Convert.ToString(foglio.RowVersion),
            };
        }
        public object UpdateCellaJExcel(Guid guid, string cellName, string value, string rowversion)
        {
            int result = 1;
            var colonna = GetColumnIndex(cellName);
            var riga = GetRowIndex(cellName);
            if (colonna != null && riga != null)
            {
                var foglioDb = getFoglio((Guid)guid);
                if (foglioDb.Righe.Count <= 0) foglioDb.Righe.Add(new Righe { numRiga = 1 });
                if (foglioDb != null)
                {
                    var rigaDb = foglioDb.Righe.Where(b => b.numRiga == (int)riga).FirstOrDefault();
                    if (rigaDb != null)
                    {
                        if (String.IsNullOrWhiteSpace(rigaDb.riga)) rigaDb.riga = String.Empty;
                        var rigaSplit = rigaDb.riga.Split(separator).ToList();
                        var count = colonna - rigaSplit.Count;
                        if (count > 0)
                        {
                            for (var i = 1; i <= count; i++) rigaSplit.Add(string.Empty);
                        }
                        if (colonna <= rigaSplit.Count)
                        {
                            var elemento = rigaSplit.ElementAtOrDefault((int)colonna - 1);
                            var proprietacolonna = foglioDb.ProprietaColonne.Where(a => a.numColonna == colonna).FirstOrDefault();
                            value = value.Replace(separator.ToString(), "");
                            if (proprietacolonna != null && proprietacolonna.column.type == "number")
                            {
                                value = value.Replace(",", ".");
                                value = Regex.Replace(value.ToUpper(), @"[^\d*\.?\d*$]", string.Empty);
                            }
                            rigaSplit[(int)colonna - 1] = value;
                            var elementoAgg = rigaSplit.ElementAtOrDefault((int)colonna - 1);
                            rigaDb.riga = String.Join(separator.ToString(), rigaSplit.ToArray());
                            rigaDb.RowVersion = Convert.ToInt64(rowversion);
                            if (foglioDb.Guid == Guid.Empty)
                            {
                                db.Fogli.Add(foglioDb);
                            }
                            db.ChangeTracker.DetectChanges();
                            result = db.SaveChanges();
                            guid = foglioDb.Guid;
                        }

                    }
                }

            }
            else
            {
                ////riga non trovata
            }


            return new { result, guid };
        }
        public object InsertRowJExcel(Guid guid, int y, int before, List<long> RowVersions)
        {
            int result = 1;
            var riga = y + 1;
            if (before == 1)
            {
                riga = riga - 1;
            }
            var foglioDb = getFoglio((Guid)guid);
            if (foglioDb.Righe.Count <= 0) foglioDb.Righe.Add(new Righe { numRiga = 1 });
            if (foglioDb != null)
            {
                var righeDb = foglioDb.Righe.OrderByDescending(a => a.numRiga).ToList();
                foreach (var item in righeDb)
                {
                    if (item.numRiga > riga)
                    {
                        item.RowVersion = RowVersions.ElementAtOrDefault(item.numRiga - 1);
                        item.numRiga++;
                    }

                }

                for (var i = 1; i <= riga; i++)
                {
                    if (righeDb.Where(a => a.numRiga == i).FirstOrDefault() == null)
                    {
                        foglioDb.Righe.Add(new Righe { numRiga = i });
                    }
                }
                foglioDb.Righe.Add(new Righe { numRiga = riga + 1 });
                if (foglioDb.Guid == Guid.Empty)
                {
                    db.Fogli.Add(foglioDb);
                }
                db.ChangeTracker.DetectChanges();
                result = db.SaveChanges();
                guid = foglioDb.Guid;
            }
            return new { result, guid };
        }
        public object DeleteRowJExcel(Guid guid, int y, List<long> RowVersions)
        {
            int result = 1;
            var riga = y + 1;
            var foglioDb = getFoglio((Guid)guid);
            if (foglioDb.Righe.Count <= 0) foglioDb.Righe.Add(new Righe { numRiga = 1 });
            if (foglioDb != null)
            {
                var righeDb = foglioDb.Righe.OrderByDescending(a => a.numRiga).ToList();
                foreach (var item in righeDb)
                {
                    if (item.numRiga == riga)
                    {
                        foglioDb.Righe.Remove(item);
                    }
                    else
                    {
                        if (item.numRiga > riga)
                        {
                            item.RowVersion = RowVersions.ElementAtOrDefault(item.numRiga - 1);
                            item.numRiga--;
                        }
                    }
                }
                if (foglioDb.Righe.Count <= 0) foglioDb.Righe.Add(new Righe { numRiga = 1 });
                if (foglioDb.Guid == Guid.Empty)
                {
                    db.Fogli.Add(foglioDb);
                }
                db.ChangeTracker.DetectChanges();
                result = db.SaveChanges();
                guid = foglioDb.Guid;
            }
            return new { result, guid };
        }
        public object InsertColJExcel(Guid guid, int x, int before, List<long> RowVersions)
        {
            int result = 1;
            var col = x + 1;
            if (before == 1)
            {
                col = col - 1;
            }
            var foglioDb = getFoglio((Guid)guid);
            if (foglioDb.Righe.Count <= 0) foglioDb.Righe.Add(new Righe { numRiga = 1 });
            if (foglioDb != null)
            {
                var righeDb = foglioDb.Righe.ToList();
                var colonna = foglioDb.ProprietaColonne.OrderBy(a => a.numColonna == col);
                if (colonna != null)
                {
                    foreach (var item in colonna)
                    {
                        if (item.numColonna > col) item.numColonna++;
                    }
                }
                foreach (var item in righeDb)
                {
                    var rigaSplit = item.riga.Split(separator).ToList();
                    if (col <= rigaSplit.Count)
                    {
                        rigaSplit.Insert(col, string.Empty);
                    }
                    else
                    {
                        rigaSplit.Add(string.Empty);
                    }
                    item.riga = String.Join(separator.ToString(), rigaSplit.ToArray());
                    item.RowVersion = RowVersions.ElementAtOrDefault(item.numRiga - 1);
                }
                if (foglioDb.Guid == Guid.Empty)
                {
                    db.Fogli.Add(foglioDb);
                }
                db.ChangeTracker.DetectChanges();
                result = db.SaveChanges();
                guid = foglioDb.Guid;
                //if (result <= 0)
                //{
                //    var colonna = foglioDb.ProprietaColonne.OrderBy(a => a.numColonna == col);
                //    if (colonna != null)
                //    {
                //        foreach (var item in colonna)
                //        {
                //            if (item.numColonna > col) item.numColonna++;
                //        }
                //    }
                //    result = db.SaveChanges();
                //}
            }
            return new { result, guid };


        }
        public object DeleteColJExcel(Guid guid, int x, List<long> RowVersions)
        {
            int result = 1;
            var col = x + 1;
            var foglioDb = getFoglio((Guid)guid);
            if (foglioDb.Righe.Count <= 0) foglioDb.Righe.Add(new Righe { numRiga = 1 });
            if (foglioDb != null)
            {
                var righeDb = foglioDb.Righe.ToList();
                foreach (var item in righeDb)
                {
                    var rigaSplit = item.riga.Split(separator).ToList();
                    if (col <= rigaSplit.Count)
                    {
                        rigaSplit.RemoveAt(col - 1);
                        item.riga = String.Join(separator.ToString(), rigaSplit.ToArray());
                        item.RowVersion = RowVersions.ElementAtOrDefault(item.numRiga - 1);
                    }

                }
                if (foglioDb.Guid == Guid.Empty)
                {
                    db.Fogli.Add(foglioDb);
                }
                db.ChangeTracker.DetectChanges();
                result = db.SaveChanges();
                guid = foglioDb.Guid;
                if (result <= 0)
                {
                    var colonna = foglioDb.ProprietaColonne.Where(a => a.numColonna == col).FirstOrDefault();
                    if (colonna != null)
                    {
                        db.Colonne.Remove(colonna);
                        foglioDb.ProprietaColonne.Remove(colonna);
                    }
                    var colonne = foglioDb.ProprietaColonne.OrderBy(a => a.numColonna == col);
                    if (colonne != null)
                    {
                        foreach (var item in colonne)
                        {
                            if (item.numColonna > col) item.numColonna--;
                        }
                    }
                    db.ChangeTracker.DetectChanges();
                    result = db.SaveChanges();
                }
            }
            return new { result, guid };
        }
        public object RenameColJExcel(Guid guid, int x, string title)
        {
            int result = 1;
            var col = x + 1;
            var foglioDb = getFoglio((Guid)guid);
            if (foglioDb.Righe.Count <= 0) foglioDb.Righe.Add(new Righe { numRiga = 1 });
            if (foglioDb != null)
            {
                var colonna = foglioDb.ProprietaColonne.Where(a => a.numColonna == col).FirstOrDefault();
                if (colonna != null)
                {
                    colonna.column.title = title;

                }
                else
                {
                    foglioDb.ProprietaColonne.Add(new Colonne { numColonna = col, column = new column { title = title } });
                }
                if (foglioDb.Guid == Guid.Empty)
                {
                    db.Fogli.Add(foglioDb);
                }
                db.ChangeTracker.DetectChanges();
                result = db.SaveChanges();
                guid = foglioDb.Guid;
            }
            return new { result, guid };
        }
        public object ResizeColJExcel(Guid guid, int x, int width)
        {
            int result = 1;
            var col = x + 1;
            var foglioDb = getFoglio((Guid)guid);
            if (foglioDb.Righe.Count <= 0) foglioDb.Righe.Add(new Righe { numRiga = 1 });
            if (foglioDb != null)
            {
                var colonna = foglioDb.ProprietaColonne.Where(a => a.numColonna == col).FirstOrDefault();
                if (colonna != null)
                {
                    colonna.column.width = Convert.ToString(width);

                }
                else
                {
                    foglioDb.ProprietaColonne.Add(new Colonne { numColonna = col, column = new column { width = Convert.ToString(width) } });
                }
                if (foglioDb.Guid == Guid.Empty)
                {
                    db.Fogli.Add(foglioDb);
                }
                db.ChangeTracker.DetectChanges();
                result = db.SaveChanges();
                guid = foglioDb.Guid;
            }
            return new { result, guid };
        }
        public object SaveStyle(Guid guid, string stileFoglio)
        {
            int result = 1;
            var foglioDb = getFoglio((Guid)guid);
            foglioDb.StileFoglio = stileFoglio;
            if (foglioDb.Guid == Guid.Empty)
            {
                db.Fogli.Add(foglioDb);
            }
            db.ChangeTracker.DetectChanges();
            result = db.SaveChanges();
            guid = foglioDb.Guid;
            return new { result, guid };
        }
        public object UpdateFoglio(Guid guid, string nomeLista, string rowversion)
        {
            int result = 1;
            var foglioDb = getFoglio((Guid)guid);
            if (!String.IsNullOrWhiteSpace(rowversion)) foglioDb.RowVersion = Convert.ToInt64(rowversion);
            foglioDb.NomeLista = nomeLista;

            if (foglioDb.Guid == Guid.Empty)
            {
                db.Fogli.Add(foglioDb);
            }
            db.ChangeTracker.DetectChanges();
            result = db.SaveChanges();
            guid = foglioDb.Guid;
            return new { result, guid };
        }
        private class colonnaJson
        {
            public string nomeTipo { get; set; }
            public string type { get; set; }
            public string source { get; set; }
        }
        private class colonneJson
        {
            public string name { get; set; }
            public string width { get; set; }
            public colonnaJson tipo { get; set; }
        }
        public object UpdateColonna(Guid guid, string colonneJson, string rowversion)
        {
            int result = 1;
            if (!String.IsNullOrWhiteSpace(colonneJson))
            {
                var foglioDb = getFoglio((Guid)guid);
                if (!String.IsNullOrWhiteSpace(rowversion)) foglioDb.RowVersion = Convert.ToInt64(rowversion);
                var objectJson = JsonConvert.DeserializeObject<List<colonneJson>>(colonneJson);

                int i = 1;
                foreach (var oldCol in foglioDb.ProprietaColonne.ToList())
                {
                    db.Colonne.Remove(oldCol);
                }

                foreach (var item in objectJson)
                {
                    var nomecolonna = item.name;
                    var nometipo = item.tipo.nomeTipo;
                    var type = item.tipo.type;
                    bool readOnly = false;
                    if (type == "readOnly")
                    {
                        readOnly = true;
                        type = string.Empty;
                    }

                    var source = item.tipo.source;
                    var sourceSplitJson = String.Empty;
                    if (!String.IsNullOrWhiteSpace(source))
                    {
                        var sourceSplit = source.Split(',').ToList();
                        sourceSplitJson = JsonConvert.SerializeObject(sourceSplit);
                    }

                    foglioDb.ProprietaColonne.Add(new Colonne
                    {
                        numColonna = i++,
                        column = new column
                        {
                            title = nomecolonna,
                            type = type,
                            readOnly = readOnly,
                            source = sourceSplitJson,
                            width = item.width,
                        },
                    });
                }




                if (foglioDb.Guid == Guid.Empty)
                {
                    db.Fogli.Add(foglioDb);
                }
                db.ChangeTracker.DetectChanges();
                result = db.SaveChanges();
                guid = foglioDb.Guid;
            }

            return new { result, guid };
        }

        public object SetStato(Guid guid, string NomeStato, string Motivazione, string rowversion)
        {
            int result = 1;
            if (!String.IsNullOrWhiteSpace(NomeStato))
            {
                var foglioDb = getFoglio((Guid)guid);
                if (!String.IsNullOrWhiteSpace(rowversion)) foglioDb.RowVersion = Convert.ToInt64(rowversion);

                if (foglioDb.Stato.Nome.ToLower() != NomeStato.Trim().ToLower())
                {
                    UpdateLogStati(foglioDb, NomeStato.Trim().ToLower(), Motivazione);
                }
                else
                {
                    return new { result = 0, guid };
                }

                if (foglioDb.Guid == Guid.Empty)
                {
                    db.Fogli.Add(foglioDb);
                }
                db.ChangeTracker.DetectChanges();
                result = db.SaveChanges();
                guid = foglioDb.Guid;
            }

            return new { result, guid };
        }


        public object SetProfilo(Guid guid, string istituto, string uocodematricola, string profilo)
        {
            int result = 1;
            if (!String.IsNullOrWhiteSpace(istituto) && !String.IsNullOrWhiteSpace(uocodematricola))
            {
                var istID = Convert.ToInt32(istituto);
                var foglioDb = db.Fogli.Include(a => a.ProfiliWL).Include(d => d.ProfiliWL.Select(e => e.profilo)).Where(a => a.Guid == guid).FirstOrDefault();


                var profiloWL = foglioDb.ProfiliWL.Where(a => a.istID == istID && a.uocodematricola == uocodematricola).FirstOrDefault();
                var profiloNew = GetProfilo(profilo);
                if (profiloWL != null)
                {
                    profiloWL.profilo = profiloNew;
                    db.ChangeTracker.DetectChanges();
                    result = db.SaveChanges();
                }
            }

            return new { result };
        }
        public object AddProfilo(Guid guid, string istituto, string uocodematricola, bool IsUocode)
        {
            int result = 1;
            if (!String.IsNullOrWhiteSpace(istituto) && !String.IsNullOrWhiteSpace(uocodematricola))
            {
                var istID = Convert.ToInt32(istituto);
                var foglioDb = db.Fogli.Include(a => a.Stato).Include(a => a.ProfiliWL).Include(d => d.ProfiliWL.Select(e => e.profilo)).Where(a => a.Guid == guid).FirstOrDefault();
                var profiloWL = foglioDb.ProfiliWL.Where(a => a.istID == istID && a.uocodematricola == uocodematricola).FirstOrDefault();
                if (profiloWL == null)
                {
                    var profiloNew = db.Profili.Where(a => a.Nome.ToLower() == "visualizzatore").FirstOrDefault();
                    profiloWL = new ProfiliWL()
                    {
                        profilo = profiloNew,
                        istID = istID,
                        uocodematricola = uocodematricola,
                        IsActive = true,
                        IsUocode = IsUocode
                    };
                    foglioDb.ProfiliWL.Add(profiloWL);
                    db.ChangeTracker.DetectChanges();
                    result = db.SaveChanges();
                    Notificare(foglioDb);
                }
            }

            return new { result };
        }
        public object RemoveProfilo(Guid guid, string istituto, string uocodematricola, bool IsUocode)
        {
            int result = 1;
            if (!String.IsNullOrWhiteSpace(istituto) && !String.IsNullOrWhiteSpace(uocodematricola))
            {
                var istID = Convert.ToInt32(istituto);
                var foglioDb = db.Fogli.Include(a => a.ProfiliWL).Include(d => d.ProfiliWL.Select(e => e.profilo)).Where(a => a.Guid == guid).FirstOrDefault();
                var profiloWL = foglioDb.ProfiliWL.Where(a => a.istID == istID && a.uocodematricola == uocodematricola).FirstOrDefault();
                if (profiloWL != null)
                {
                    foglioDb.ProfiliWL.Remove(profiloWL);
                    db.ChangeTracker.DetectChanges();
                    result = db.SaveChanges();
                }
            }

            return new { result };
        }

        public Profili GetProfilo(string profilo)
        {
            return db.Profili.Where(a => a.Nome.ToLower() == profilo.ToLower()).FirstOrDefault();
        }

        public ViewModel ApriRichiesta(Guid guid)
        {
            if (guid == null) guid = Guid.Empty;
            var foglio = getFoglio((Guid)guid);
            Profili profilo = null;
            foreach (var wl in foglio.ProfiliWL)
            {
                if (myIstID == wl.istID)
                {
                    if (wl.IsUocode)
                    {
                        if (myUocode == wl.uocodematricola)
                        {
                            profilo = wl.profilo;
                            break;
                        }
                    }
                    else
                    {
                        if (myMatricola == wl.uocodematricola)
                        {
                            profilo = wl.profilo;
                            break;
                        }
                    }
                }
            }
            if (!string.IsNullOrWhiteSpace(codRuolo))
            {
                var profiloRuolo = GetProfilo(codRuolo);
                if (profiloRuolo != null)
                {
                    profilo = profiloRuolo;
                }
            }
            var result = new ViewModel { guid = (Guid)guid, profilo = profilo, stato = foglio.Stato, NomeLista = foglio.NomeLista };
            return result;
        }
        public EmailSettings EmailSettings()
        {
            var result = emailSettings;
            return result.FirstOrDefault();
        }
        public bool SendEmail(Fogli foglio, EmailTesti emailtesto)
        {
            var result = false;
            if (emailtesto != null)
            {
                result = true;
                //string fullUrl = "";
                string host = HttpContext.Current.Request.Url.Host;
                string nomePortale = ConfigurationManager.AppSettings["NomePortale"];
                string hostName = @"http://gruppo.example.it/Webapps/Portal/WKS_SSO.aspx?url=http://" + host + nomePortale;
                string fullUrl = hostName + @"/FCI/Richiesta/GestioneRichieste/" + foglio.Guid;
                //fullUrl = this.Url.Action("GestioneRichieste", "Richiesta", new { }, this.Request.Url.Scheme);
                var oggetto = emailtesto.Oggetto;
                var Corpo = emailtesto.Corpo;

                Corpo = Corpo.Replace("{nomelista}", foglio.NomeLista);
                var uocode = myUocode;
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
                Corpo = Corpo.Replace("{struttura}", uocode);

                Corpo = Corpo.Replace("{link}", fullUrl);

                var emailset = EmailSettings();
                var sendmail = new EmailSender(emailset);

                var email = "";
                foreach (var item in foglio.ProfiliWL)
                    if (!item.Notificata)
                    {
                        try
                        {

                            if (item.IsUocode)
                            {
                                if (!String.IsNullOrWhiteSpace(email)) email += ", ";
                                email += resourcesRepository.GetEmailByUoCode(item.uocodematricola, Convert.ToString(item.istID));
                            }
                            else
                            {
                                var resource = commonBusiness.getResourcesByUoCodeMatricola(String.Empty, item.uocodematricola).FirstOrDefault();
                                if (resource != null)
                                {
                                    if (!String.IsNullOrWhiteSpace(email)) email += ", ";
                                    email += resource.Email;
                                }

                            }

                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex.Message);
                            ErrorSignal.FromCurrentContext().Raise(ex);
                        }

                    }
                var logemail = new LogEmail() { Note = foglio.Guid.ToString(), ErrorMessage = "NonOk", Result = false, Subject = oggetto, Message = Corpo, IsBodyHtml = emailtesto.Html };
                try
                {
                    if (!String.IsNullOrWhiteSpace(email))
                    {
                        logemail.Email = email;
                        var resultEmail = sendmail.SendEmail(email, oggetto, Corpo, emailtesto.Html);
                        if (resultEmail != null)
                        {
                            if (!resultEmail.Result) result = false;
                            logemail.Result = resultEmail.Result;
                            logemail.ErrorMessage = resultEmail.ErrorMessage;
                            if (logemail.Result)
                            {
                                foreach (var item in foglio.ProfiliWL)
                                {
                                    item.Notificata = true;
                                }
                                db.ChangeTracker.DetectChanges();
                                db.SaveChanges();
                            }
                        }
                        else
                        {
                            result = false;
                            logemail.Result = false;
                            logemail.ErrorMessage = "Invio email anomalia";
                        }
                    }
                    else
                    {
                        result = false;
                        logemail.Result = false;
                        logemail.ErrorMessage = "Email destinatario assente";
                    }
                }
                catch (Exception ex)
                {
                    result = false;
                    logemail.ErrorMessage = ex.Message;
                    logemail.Result = false;
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    ErrorSignal.FromCurrentContext().Raise(ex);
                }
                UpdateLogEmail(logemail);
            }

            return result;
        }

        public void UpdateLogEmail(LogEmail logemail)
        {
            logemail.Data = DateTime.Now;
            db.LogEmail.Add(logemail);
            db.ChangeTracker.DetectChanges();
            db.SaveChanges();
        }

    }
}