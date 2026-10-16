using FCIWeb.Areas.FCI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using Elmah;
using Newtonsoft.Json;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text.RegularExpressions;
using Table = DocumentFormat.OpenXml.Wordprocessing.Table;
using AreaCommonWeb.DAL;
using AreaCommonWeb.Business;

namespace FCIWeb.Areas.FCI.Uility
{
    //public enum Modello 
    //{
    //    Richiesta,
    //    Consegna,
    //    Comunicazione_Mod1,
    //    Comunicazione_Mod2,
    //    Disposizione_add_spese
    //}


    public class StampaDocumenti
    {

        private CommonBusiness commonBusiness = new CommonBusiness();
        private MemoryStream file_memoria;
        private Richieste richesta;
        //private string fullpath = @"C:\\Sviluppo\\";
        private string fullpath = @"//FILESERVER/ApplicazioniMvc/WebPortal/Doc_Files/FCI/";
        private string filetemp = @"//FILESERVER/ApplicazioniMvc/WebPortal/Doc_Files/FCI/temp/";

        public  void OpenAndAddTextToWordDocument(string filepath, string txt)
        {
            Directory.CreateDirectory(filetemp);
            filetemp += DateTime.UtcNow.Ticks + @".docx";
            File.Copy(fullpath+@"Modulo_Richiesta_Documentazione_REV_new.docx", filetemp);
               
            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(filetemp, true))
            {
                string docText = null;
                using (StreamReader sr = new StreamReader(wordDoc.MainDocumentPart.GetStream()))
                {
                    docText = sr.ReadToEnd();
                }

                string filiale = this.richesta.Filiale == null ? "" : this.richesta.Filiale;
                Regex regexText = new Regex("FILIALE_PROG");
                docText = regexText.Replace(docText, filiale);

                using (StreamWriter sw = new StreamWriter(wordDoc.MainDocumentPart.GetStream(FileMode.Create)))
                {
                    sw.Write(docText);
                }
                
                Body bod = wordDoc.MainDocumentPart.Document.Body;
                foreach (Table t in bod.Descendants<Table>().Where(tbl => tbl.InnerText.Contains("myTable")))
                {
                    string titolo = t.InnerText;
                    //t.Append(new TableRow(new TableCell(new Paragraph(new Run(new Text("test"))))));
                }

                //aggiunge una riga
                TableRow row;
                TableRow newriga;
                TableCell cell;
                Run run;
                Text testo;
                string des_rap, cartone;
                DocumentFormat.OpenXml.Wordprocessing.Paragraph parag;
                foreach (Table t in bod.Descendants<Table>().Where(t=> t.InnerXml.Contains("TABELLA_ALLEGATI")))
                {
                    foreach (DocumentiRichiesti doc_ric in this.richesta.DocumentiRichiesti)
                    {
                        newriga = (TableRow)t.Elements<TableRow>().ElementAt(t.Elements<TableRow>().Count()-1).Clone();

                        row = t.Elements<TableRow>().ElementAt(t.Elements<TableRow>().Count() - 1);//prima riga vuota
                        row.ElementAt(1).Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>().First().Elements<Run>().First().Elements<Text>()
                            .First().Text = doc_ric.NumCopie.ToString();

                        des_rap = string.IsNullOrEmpty(doc_ric.Rapporto) ? "" : " " + doc_ric.Rapporto;
                        row.ElementAt(2).Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>().First().Elements<Run>().First().Elements<Text>()
                            .First().Text = doc_ric.DescrizioneDocumento + des_rap;

                        row.ElementAt(3).Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>().First().Elements<Run>().First().Elements<Text>()
                                .First().Text = doc_ric.DocOrig ? "X" : "";

                        row.ElementAt(4).Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>().First().Elements<Run>().First().Elements<Text>()
                                .First().Text = !doc_ric.DocOrig ? "X" : "";

                        cartone = doc_ric.NumCartone == null ? "" : doc_ric.NumCartone.ToString();
                        row.ElementAt(6).Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>().First().Elements<Run>().First().Elements<Text>()
                            .First().Text = cartone;

                        //table.Rows.Add();
                        t.Append(newriga);
                    }
                    

                    



                    //cell = row.Elements<TableCell>().ElementAt(2);
                    //// Find the first paragraph in the table cell.  
                    //parag = cell.Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>().First();
                    //// Find the first run in the paragraph.  
                    //run = parag.Elements<Run>().First();
                    //// Set the text for the run.  
                    //testo = run.Elements<Text>().First();
                    //testo.Text = "yyy3";



                    //nuovariga
                    t.Append(new TableRow(new TableCell(new DocumentFormat.OpenXml.Wordprocessing. Paragraph(new Run(new Text("test"))))));
                }

                wordDoc.Save();
                wordDoc.Close();
            }
            
            

            Stream stream = File.Open(filetemp, FileMode.Open);
            var file_memoria = new MemoryStream();
            stream.CopyTo(file_memoria);
            stream.Close();

            //deve restituire 
            //return file_memoria.ToArray();

            
        }

        public StampaDocumenti(Richieste richiesta)
        {
            this.richesta = richiesta;
                        
            //OpenAndAddTextToWordDocument(strDoc, strTxt);                      
        }

        private string ValidaCampo(string campodb)
        {
    //        '"':"&quot;"
    //'&':"&amp;"
    //"'":"&apos;"
    //"<":"&lt;"
    //">":"&gt;"
            string valore = string.IsNullOrEmpty(campodb) ? " " : campodb;
            valore = valore.Replace("\"","&quot;");
            valore = valore.Replace("&", "&amp;");
            valore = valore.Replace("'","&apos;");
            valore = valore.Replace("<", "&lt;");
            valore = valore.Replace(">", "&gt;");

            return valore;
        }

        private string AssegnaCampi(string docText, WordprocessingDocument wordDoc)
        {
            try
            {
 
                Regex regexText;
                int ex = 0;

                //ErrorSignal.FromCurrentContext().Raise(new Exception("Richiesta: " + JsonConvert.SerializeObject(this.richesta)));

                //string filiale = string.IsNullOrEmpty( this.richesta.Filiale )? "" : this.richesta.Filiale;
                

                string canaleconsegna = " ";
                if (this.richesta.CanaleConsegna != null)
                    canaleconsegna = this.richesta.CanaleConsegna.Nome.ToUpper();
                string descrizione_filiale = ValidaCampo(this.richesta.Filiale);
                string uocode = this.richesta.UOCode;
                string luogo = this.richesta.Luogo;
                if (canaleconsegna == "FILIALE")
                {
                    regexText = new Regex("FILIALE_PROG");

                    if (this.richesta.Filiale == this.richesta.DescrizioneFilialeConsegna)
                        docText = regexText.Replace(docText, descrizione_filiale);
                    else
                    {
                        descrizione_filiale = ValidaCampo(this.richesta.DescrizioneFilialeConsegna);
                        uocode = this.richesta.FilialeConsegna;
                        docText = regexText.Replace(docText, descrizione_filiale);
                    }


                    try
                    {

                        IQueryable<DimUO> filiali = commonBusiness.GetDimUoByUocode(uocode);

                        DimComuni comune = commonBusiness.GetComuneByUoCode(uocode, this.richesta.Istituto);

                        //IQueryable<DimComuni> comune = commonBusiness.GetComune(provincia,comune);
                        //comune.FirstOrDefault().CAP;
                        //comune.FirstOrDefault().Provincia;

                        //var indirizzo = filiali.FirstOrDefault().Indirizzo;
                        regexText = new Regex("INDIRIZZO_FILIALE");
                        docText = regexText.Replace(docText, ValidaCampo(filiali.FirstOrDefault().Indirizzo));

                        regexText = new Regex("CAP_CITTA_PROG");
                        docText = regexText.Replace(docText, ValidaCampo(comune.CAP + "-" + comune.Provincia));
                        luogo = comune.Comune;


                    }
                    catch (Exception ex2)
                    {
                        //System.Diagnostics.Debug.WriteLine(ex.Message);
                        ErrorSignal.FromCurrentContext().Raise(ex2);
                    }
                }
                else
                {
                    //pulisco campi per eventuale compilazione manuale
                    regexText = new Regex("FILIALE_PROG");
                    docText = regexText.Replace(docText, descrizione_filiale);

                    regexText = new Regex("INDIRIZZO_FILIALE");
                    docText = regexText.Replace(docText, "");

                    regexText = new Regex("CAP_CITTA_PROG");
                    docText = regexText.Replace(docText, "");                    
                }


                regexText = new Regex("DATA_PROG");
                docText = regexText.Replace(docText, 
                    ValidaCampo( Convert.ToDateTime( this.richesta.DataRichiesta).ToShortDateString()));

                regexText = new Regex("NDG_PROG");
                docText = regexText.Replace(docText,ValidaCampo( this.richesta.NDG.ToString()));

                regexText = new Regex("COGNOME_NOME_PROG");
                docText = regexText.Replace(docText, ValidaCampo( this.richesta.IntestazioneNDG));

                string cf = ValidaCampo(this.richesta.CodiceFiscale);
                string piva = ValidaCampo(this.richesta.PIva);
                cf = cf + (piva.Trim() != "" && cf.Trim()!="" ? "/" + piva : piva);
                regexText = new Regex("CF_PI_PROG");
                docText = regexText.Replace(docText, cf);

                regexText = new Regex("LUOGO_NASCITA_PROG");
                docText = regexText.Replace(docText, ValidaCampo( this.richesta.LuogoNascita));

                regexText = new Regex("DATA_NASCITA_PROG");
                docText = regexText.Replace(docText,
                    this.richesta.DataNascita == null ? "" : Convert.ToDateTime(this.richesta.DataNascita).ToShortDateString()
                    );

                regexText = new Regex("INDIRIZZO_PROG");
                if (string.IsNullOrEmpty( this.richesta.IndirizzoRichiedente))
                    docText = regexText.Replace(docText, ValidaCampo( this.richesta.Indirizzo));
                else
                    docText = regexText.Replace(docText, ValidaCampo(this.richesta.IndirizzoRichiedente));

                regexText = new Regex("TOTALE_DOCUMENTI");
                docText = regexText.Replace(docText,
                    ValidaCampo( String.Format("{0:N2}", this.richesta.ImportoTotale)));

                string pagamento = (this.richesta.ModalitaPagamento == null) ? "" : this.richesta.ModalitaPagamento.Nome.ToUpper();
                regexText = new Regex("CC_PROG");
                docText = regexText.Replace(docText, pagamento == "CONTO" ? "X" : "");

                regexText = new Regex("CONTOCORRENTE_PROG");
                docText = regexText.Replace(docText, ValidaCampo( this.richesta.NumeroConto));

                regexText = new Regex("CAS_PROG");
                docText = regexText.Replace(docText, pagamento == "CASSA" ? "X" : "");

                
                regexText = new Regex("M_01");
                docText = regexText.Replace(docText, this.richesta.CanaleConsegna==null ? " ": this.richesta.CanaleConsegna.Nome.ToUpper() == "EMAIL" ? "X" : "");

                regexText = new Regex("MAIL_CLIENTE_PROG");
                docText = regexText.Replace(docText, this.richesta.email);

                regexText = new Regex("M_02");
                docText = regexText.Replace(docText, this.richesta.CanaleConsegna == null ? " " : this.richesta.CanaleConsegna.Nome.ToUpper() == "PEC" ? "X" : "");

                regexText = new Regex("PEC_CLIENTE_PROG");
                string pecmail = ValidaCampo(this.richesta.pec).Trim();
                docText = regexText.Replace(docText, pecmail != "" ? "PEC " + pecmail : " ");

                regexText = new Regex("M_03");
                docText = regexText.Replace(docText, this.richesta.CanaleConsegna == null ? " " :  this.richesta.CanaleConsegna.Nome.ToUpper() == "FILIALE" ? "X" : "");

                //string luogo = string.IsNullOrEmpty(this.richesta.Luogo) ? "" : this.richesta.Luogo;
                regexText = new Regex("LUOGO_PIEPAGINA");
                docText = regexText.Replace(docText, ValidaCampo(luogo) );

                regexText = new Regex("DATAOGGI");
                docText = regexText.Replace(docText, DateTime.Now.ToShortDateString());

                string canalerichiesta = this.richesta.CanaleRicezione==null ? "" : this.richesta.CanaleRicezione.Nome.ToUpper();
                string m_04 = "";
                string m_05 = "";
                string m_06 = "";
                string data1 = "";
                string data2 = "";
                string data3="";
                switch (canalerichiesta)
                {
                    case "FILIALE":
                        {
                            m_04 = "X";
                            m_05 = "";
                            m_06 = "";
                            data1 = Convert.ToDateTime(this.richesta.DataRichiesta).ToShortDateString();
                            data2 = "";
                            data3 = "";                           

                            break;
                        }

                    case "PEC":
                        {
                            m_04 = "";
                            m_05 = "X";
                            m_06 = "";
                            data1 = "";
                            data2 = Convert.ToDateTime(this.richesta.DataRichiesta).ToShortDateString();
                            data3 = "";

                            break;
                        }
                    case "RACCOMANDATA":
                        {
                            m_04 = "";
                            m_05 = "";
                            m_06 = "X";
                            data1 = "";
                            data2 = "";
                            data3 = Convert.ToDateTime(this.richesta.DataRichiesta).ToShortDateString();

                            break;
                        }
                }

                regexText = new Regex("M_04");
                docText = regexText.Replace(docText, m_04);
                regexText = new Regex("M_05");
                docText = regexText.Replace(docText, m_05);
                regexText = new Regex("M_06");
                docText = regexText.Replace(docText, m_06);
                regexText = new Regex("DATA_RICHIESTA_1");
                docText = regexText.Replace(docText, data1);
                regexText = new Regex("DATA_RICHIESTA_2");
                docText = regexText.Replace(docText, data2);
                regexText = new Regex("DATA_RICHIESTA_3");
                docText = regexText.Replace(docText, data3);

                regexText = new Regex("DATA_RICEVUTA_DOC");
                docText = regexText.Replace(docText, this.richesta.DataConsegna == null ? " ": Convert.ToDateTime(this.richesta.DataConsegna).ToShortDateString());

                regexText = new Regex("Data_Avviso");
                docText = regexText.Replace(docText, this.richesta.DataAvviso == null ? " " : Convert.ToDateTime(this.richesta.DataAvviso).ToShortDateString());


                //string canaleconsegna = this.richesta.CanaleConsegna.Nome.ToUpper();
                if (canaleconsegna == "FILIALE" &&
                    this.richesta.Filiale == this.richesta.DescrizioneFilialeConsegna)
                    canaleconsegna = "in FILIALE";
                if (canaleconsegna == "FILIALE" &&
                    this.richesta.Filiale != this.richesta.DescrizioneFilialeConsegna)
                    canaleconsegna = "in " + this.richesta.DescrizioneFilialeConsegna;

                if (canaleconsegna == "PEC" || canaleconsegna=="EMAIL")
                    canaleconsegna = "VIA "+ canaleconsegna;
                if (canaleconsegna == "RACCOMANDATA")
                    canaleconsegna = "per RACCOMANDATA";
                regexText = new Regex("CANALE_CONSEGNA");
                docText = regexText.Replace(docText, canaleconsegna);               

                using (StreamWriter sw = new StreamWriter(wordDoc.MainDocumentPart.GetStream(FileMode.Create)))
                {
                    sw.Write(docText);
                }
            }
            catch (Exception ex)
            {
                //System.Diagnostics.Debug.WriteLine(ex.Message);
                ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return docText;
        }


        public byte[] Compila_Richiesta()
        {
            //return new byte[0];
            var file_memoria = new MemoryStream();

            try
            {
                string nomefile = "Modulo_Richiesta_Documentazione_REV_new.docx";
                object m = System.Reflection.Missing.Value;
                object oldFileName;


                oldFileName = this.fullpath + nomefile;
                Directory.CreateDirectory(filetemp);
                filetemp += DateTime.UtcNow.Ticks+@".docx";
                File.Copy(oldFileName.ToString(),filetemp);

                using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(filetemp, true))
                {
                    string docText = null;
                    using (StreamReader sr = new StreamReader(wordDoc.MainDocumentPart.GetStream()))
                    {
                        docText = sr.ReadToEnd();
                    }

                    //string canaleconsegna = " ";
                    //if (this.richesta.CanaleConsegna != null)
                    //    canaleconsegna = this.richesta.CanaleConsegna.Nome.ToUpper();
                    //string descrizione_filiale = ValidaCampo(this.richesta.Filiale);
                    //string uocode = this.richesta.UOCode;
                    //string luogo = this.richesta.Luogo;

                    //Regex regexText = new Regex("FILIALE_PROG");
                    //docText = regexText.Replace(docText, descrizione_filiale);

                    //try
                    //{

                    //    IQueryable<DimUO> filiali = commonBusiness.GetDimUoByUocode(uocode);

                    //    DimComuni comune = commonBusiness.GetComuneByUoCode(uocode, this.richesta.Istituto);

                    //    regexText = new Regex("INDIRIZZO_FILIALE");
                    //    docText = regexText.Replace(docText, ValidaCampo(filiali.FirstOrDefault().Indirizzo));

                    //    regexText = new Regex("CAP_CITTA_PROG");
                    //    docText = regexText.Replace(docText, ValidaCampo(comune.CAP + "-" + comune.Provincia));
                    //    luogo = comune.Comune;
                    //}
                    //catch (Exception ex2)
                    //{
                    //    //System.Diagnostics.Debug.WriteLine(ex.Message);
                    //    ErrorSignal.FromCurrentContext().Raise(ex2);
                    //}
                    ////string luogo = string.IsNullOrEmpty(this.richesta.Luogo) ? "" : this.richesta.Luogo;
                    //regexText = new Regex("LUOGO_PIEPAGINA");
                    //docText = regexText.Replace(docText, ValidaCampo(luogo));



                    docText = this.AssegnaCampi(docText, wordDoc);
                    
                    //aggiunge una riga
                    TableRow row;
                    TableRow newriga;                    
                    string des_rap, cartone;
                    Body bod = wordDoc.MainDocumentPart.Document.Body;                    
                    foreach (Table t in bod.Descendants<Table>().Where(t => t.InnerXml.Contains("TABELLA_ALLEGATI")))
                    {
                        foreach (DocumentiRichiesti doc_ric in this.richesta.DocumentiRichiesti)
                        {
                            newriga = (TableRow)t.Elements<TableRow>().ElementAt(t.Elements<TableRow>().Count() - 1).Clone();

                            row = t.Elements<TableRow>().ElementAt(t.Elements<TableRow>().Count() - 1);//prima riga vuota
                            row.ElementAt(1).Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>().First().Elements<Run>().First().Elements<Text>()
                                .First().Text = doc_ric.NumCopie.ToString();
                            
                            row.ElementAt(2).Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>().First().Elements<Run>().First().Elements<Text>()
                                .First().Text = doc_ric.DescrizioneDocumento;

                            des_rap = string.IsNullOrEmpty(doc_ric.Rapporto) ? "" : " " + doc_ric.Rapporto;
                            row.ElementAt(2).Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>().First().Elements<Run>().ElementAt(1).Elements<Text>()
                                .First().Text = "Rapporto n. " +des_rap;

                            row.ElementAt(3).Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>().First().Elements<Run>().First().Elements<Text>()
                                    .First().Text = doc_ric.DocOrig ? "X" : "";

                            row.ElementAt(4).Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>().First().Elements<Run>().First().Elements<Text>()
                                    .First().Text = !doc_ric.DocOrig ? "X" : "";

                            cartone = doc_ric.NumCartone == null ? "" : doc_ric.NumCartone.ToString();
                            row.ElementAt(6).Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>().First().Elements<Run>().First().Elements<Text>()
                                .First().Text = cartone;

                            //table.Rows.Add();
                            t.Append(newriga);
                        }
                                              
                    }

                    wordDoc.Save();
                    wordDoc.Close();
                }

                Stream stream = File.Open(filetemp, FileMode.Open);
                file_memoria = new MemoryStream();
                stream.CopyTo(file_memoria);
                stream.Close();
                File.Delete(filetemp);
            }
            catch(Exception ex)
            {
                //System.Diagnostics.Debug.WriteLine(ex.Message);
                ErrorSignal.FromCurrentContext().Raise(ex);
            }
            
            return file_memoria.ToArray();
        }

        public byte[] Compila_Consegna()
        {
            var file_memoria = new MemoryStream();
            try
            {
                string nomefile = "Ricevuta_consegna_copia_documentazione_new.docx";
                object m = System.Reflection.Missing.Value;
                object oldFileName;
                oldFileName = this.fullpath + nomefile;

                
                Directory.CreateDirectory(filetemp);
                filetemp += DateTime.UtcNow.Ticks + @".docx";
                File.Copy(oldFileName.ToString(), filetemp);

                using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(filetemp, true))
                {
                    string docText = null;
                    using (StreamReader sr = new StreamReader(wordDoc.MainDocumentPart.GetStream()))
                    {
                        docText = sr.ReadToEnd();
                    }


                    docText = this.AssegnaCampi(docText, wordDoc);

                    //aggiunge una riga
                    TableRow row;
                    TableRow newriga;
                    string des_rap;
                    Body bod = wordDoc.MainDocumentPart.Document.Body;
                    foreach (Table t in bod.Descendants<Table>().Where(t => t.InnerXml.Contains("TABELLA_ALLEGATI")))
                    {
                        foreach (DocumentiRichiesti doc_ric in this.richesta.DocumentiRichiesti)
                        {
                            newriga = (TableRow)t.Elements<TableRow>().ElementAt(t.Elements<TableRow>().Count() - 1).Clone();

                            row = t.Elements<TableRow>().ElementAt(t.Elements<TableRow>().Count() - 1);//prima riga vuota
                            row.ElementAt(1).Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>().First().Elements<Run>().First().Elements<Text>()
                                .First().Text = doc_ric.NumCopie.ToString();

                            des_rap = string.IsNullOrEmpty(doc_ric.Rapporto) ? "" : " " + doc_ric.Rapporto;
                            row.ElementAt(2).Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>().First().Elements<Run>().First().Elements<Text>()
                                .First().Text = doc_ric.DescrizioneDocumento + des_rap;

                            row.ElementAt(3).Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>().First().Elements<Run>().First().Elements<Text>()
                                    .First().Text = doc_ric.DocOrig ? "X" : "";

                            row.ElementAt(4).Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>().First().Elements<Run>().First().Elements<Text>()
                                    .First().Text = !doc_ric.DocOrig ? "X" : "";
                             
  
                            //table.Rows.Add();
                            t.Append(newriga);
                        }

                    }

                    wordDoc.Save();
                    wordDoc.Close();
                }


                Stream stream = File.Open(filetemp, FileMode.Open);
                file_memoria = new MemoryStream();
                stream.CopyTo(file_memoria);
                stream.Close();
                File.Delete(filetemp);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                ErrorSignal.FromCurrentContext().Raise(ex);
            }
  

            return file_memoria.ToArray();
        }

        public byte[] Compila_Addspese()
        {
            var file_memoria = new MemoryStream();
            try
            {
                object nomefile = "Lettera_Disposizione_Addebito_Spese_Copie_new.docx";
                object m = System.Reflection.Missing.Value;
                object oldFileName;
                oldFileName = this.fullpath + nomefile;


                Directory.CreateDirectory(filetemp);
                filetemp += DateTime.UtcNow.Ticks + @".docx";
                File.Copy(oldFileName.ToString(), filetemp);

                using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(filetemp, true))
                {
                    string docText = null;
                    using (StreamReader sr = new StreamReader(wordDoc.MainDocumentPart.GetStream()))
                    {
                        docText = sr.ReadToEnd();
                    }


                    docText = this.AssegnaCampi(docText, wordDoc);

                    //aggiunge una riga
                    TableRow row;
                    TableRow newriga;
                    string des_rap;
                    Body bod = wordDoc.MainDocumentPart.Document.Body;
                    foreach (Table t in bod.Descendants<Table>().Where(t => t.InnerXml.Contains("TABELLA_ALLEGATI")))
                    {
                        foreach (DocumentiRichiesti doc_ric in this.richesta.DocumentiRichiesti)
                        {
                            newriga = (TableRow)t.Elements<TableRow>().ElementAt(t.Elements<TableRow>().Count() - 1).Clone();

                            row = t.Elements<TableRow>().ElementAt(t.Elements<TableRow>().Count() - 1);//prima riga vuota
                            row.ElementAt(1).Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>().First().Elements<Run>().First().Elements<Text>()
                                .First().Text = doc_ric.NumCopie.ToString();

                            des_rap = string.IsNullOrEmpty(doc_ric.Rapporto) ? "" : " " + doc_ric.Rapporto;
                            row.ElementAt(2).Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>().First().Elements<Run>().First().Elements<Text>()
                                .First().Text = doc_ric.DescrizioneDocumento + des_rap;

                            row.ElementAt(3).Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>().First().Elements<Run>().First().Elements<Text>()
                                    .First().Text ="€ "+ String.Format("{0:#.00}", doc_ric.Costo);

                            t.Append(newriga);
                        }

                    }

                    wordDoc.Save();
                    wordDoc.Close();
                }

                Stream stream = File.Open(filetemp, FileMode.Open);
                file_memoria = new MemoryStream();
                stream.CopyTo(file_memoria);
                stream.Close();
                File.Delete(filetemp);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                ErrorSignal.FromCurrentContext().Raise(ex);
            }
            

            return file_memoria.ToArray();
        }


        public byte[] Compila_ModDocumentiGiacenza()
        {
            var file_memoria = new MemoryStream();

            try
            {
                object nomefile = "FAC-simile_Comunic_Mod1_new.docx";
                object m = System.Reflection.Missing.Value;
                object oldFileName;
                oldFileName = this.fullpath + nomefile;


                Directory.CreateDirectory(filetemp);
                filetemp += DateTime.UtcNow.Ticks + @".docx";
                File.Copy(oldFileName.ToString(), filetemp);

                using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(filetemp, true))
                {
                    string docText = null;
                    using (StreamReader sr = new StreamReader(wordDoc.MainDocumentPart.GetStream()))
                    {
                        docText = sr.ReadToEnd();
                    }


                    docText = this.AssegnaCampi(docText, wordDoc);

                    //aggiunge una riga
                    TableRow row;
                    TableRow newriga;
                    string des_rap;
                    Body bod = wordDoc.MainDocumentPart.Document.Body;
                    foreach (Table t in bod.Descendants<Table>().Where(t => t.InnerXml.Contains("TABELLA_ALLEGATI")))
                    {
                        foreach (DocumentiRichiesti doc_ric in this.richesta.DocumentiRichiesti)
                        {
                            newriga = (TableRow)t.Elements<TableRow>().ElementAt(t.Elements<TableRow>().Count() - 1).Clone();

                            row = t.Elements<TableRow>().ElementAt(t.Elements<TableRow>().Count() - 1);//prima riga vuota
                            row.ElementAt(1).Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>().First().Elements<Run>().First().Elements<Text>()
                                .First().Text = doc_ric.NumCopie.ToString();

                            des_rap = string.IsNullOrEmpty(doc_ric.Rapporto) ? "" : " " + doc_ric.Rapporto;
                            row.ElementAt(2).Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>().First().Elements<Run>().First().Elements<Text>()
                                .First().Text = doc_ric.DescrizioneDocumento + des_rap;

                            row.ElementAt(3).Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>().First().Elements<Run>().First().Elements<Text>()
                                    .First().Text = "€ " + String.Format("{0:#.00}", doc_ric.Costo);

                            t.Append(newriga);
                        }

                    }

                    wordDoc.Save();
                    wordDoc.Close();
                }

                Stream stream = File.Open(filetemp, FileMode.Open);
                file_memoria = new MemoryStream();
                stream.CopyTo(file_memoria);
                stream.Close();
                File.Delete(filetemp);
              
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                ErrorSignal.FromCurrentContext().Raise(ex);
            }

            

            return file_memoria.ToArray();
        }

        
        public byte[] Compila_ModInvioDocumentazione()
        {
            var file_memoria = new MemoryStream();

            try
            {
                object nomefile = "FAC-simile Comunic_Mod2_new.docx";
                object m = System.Reflection.Missing.Value;
                object oldFileName;
                oldFileName = this.fullpath + nomefile;


                Directory.CreateDirectory(filetemp);
                filetemp += DateTime.UtcNow.Ticks + @".docx";
                File.Copy(oldFileName.ToString(), filetemp);

                using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(filetemp, true))
                {
                    string docText = null;
                    using (StreamReader sr = new StreamReader(wordDoc.MainDocumentPart.GetStream()))
                    {
                        docText = sr.ReadToEnd();
                    }


                    docText = this.AssegnaCampi(docText, wordDoc);

                    //aggiunge una riga
                    TableRow row;
                    TableRow newriga;
                    string des_rap;
                    Body bod = wordDoc.MainDocumentPart.Document.Body;
                    foreach (Table t in bod.Descendants<Table>().Where(t => t.InnerXml.Contains("TABELLA_ALLEGATI")))
                    {
                        foreach (DocumentiRichiesti doc_ric in this.richesta.DocumentiRichiesti)
                        {
                            newriga = (TableRow)t.Elements<TableRow>().ElementAt(t.Elements<TableRow>().Count() - 1).Clone();

                            row = t.Elements<TableRow>().ElementAt(t.Elements<TableRow>().Count() - 1);//prima riga vuota
                            row.ElementAt(1).Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>().First().Elements<Run>().First().Elements<Text>()
                                .First().Text = doc_ric.NumCopie.ToString();

                            des_rap = string.IsNullOrEmpty(doc_ric.Rapporto) ? "" : " " + doc_ric.Rapporto;
                            row.ElementAt(2).Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>().First().Elements<Run>().First().Elements<Text>()
                                .First().Text = doc_ric.DescrizioneDocumento + des_rap;

                            row.ElementAt(3).Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>().First().Elements<Run>().First().Elements<Text>()
                                    .First().Text = "€ " + String.Format("{0:#.00}", doc_ric.Costo);

                            t.Append(newriga);
                        }

                    }

                    wordDoc.Save();
                    wordDoc.Close();
                }

                Stream stream = File.Open(filetemp, FileMode.Open);
                file_memoria = new MemoryStream();
                stream.CopyTo(file_memoria);
                stream.Close();
                File.Delete(filetemp);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                ErrorSignal.FromCurrentContext().Raise(ex);
            }

            

            return file_memoria.ToArray();
        }
    }
}