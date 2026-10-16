using Newtonsoft.Json;
using FCIWeb.Areas.FCI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Data;
using FCIWeb.Areas.FCI.Uility;

namespace FCIWeb.Areas.FCI.DAL
{
    public class InitialCreate
    {
        public void On(FCIContext context)
        {
            //var Rowsheet = new List<Righe>
            //{
            //    new Righe{ riga = "xxxxxxxxxxxx" },

            //};

            //var Worksheet = new List<Fogli>
            //{
            //    new Fogli{ nome = "pippo", righe = Rowsheet },

            //};

            //var Workbook = new List<Cartelle>
            //{
            //    new Cartelle{ nome = "pippo", fogli = Worksheet },

            //};
            //Workbook.ForEach(s => context.Cartella.Add(s));
            //var pippo = context.SaveChanges();

            //var wb = context.Cartella.FirstOrDefault();
            //wb.nome = "pluto";
            ////wb.RowVersion = DateTime.UtcNow.Ticks;
            ////context.Entry(wb).State = EntityState.Modified;

            //var pluto = context.SaveChanges();
            var bozza = new Stati { Nome = "Bozze", Descrizione = "Bozze", Pubblica = false, Modificabile = true, Cancellabile = true };
            var annullata = new Stati { Nome = "Annullata", Descrizione = "Annullata", Pubblica = false, Modificabile = false, ObbligoMotivazione=true };
            var pubblica = new Stati { Nome = "Pubblicata", Descrizione = "Pubblicata", Pubblica = true, Modificabile = true };
            var archiviata = new Stati { Nome = "Archiviata", Descrizione = "Archiviata", Pubblica = true, Modificabile = false };
            var nascosta = new Stati { Nome = "Nascosta", Descrizione = "Nascosta", Pubblica = false, Modificabile = true };

            var stati = new List<Stati>
            {
                bozza,             //non pubblica e modificabile
                annullata,    //non pubblica e non modificabile
                pubblica,    //pubblica e modificabile
                archiviata,   //pubblica e non modificabile
                nascosta,       //non pubblica e modificabile 
            };
            stati.ForEach(s => context.Stati.Add(s));
            context.SaveChanges();

            bozza.StatiPossibili.Add(new StatiPossibili { Stato = pubblica.Id });

            pubblica.StatiPossibili.Add(new StatiPossibili { Stato = annullata.Id });
            pubblica.StatiPossibili.Add(new StatiPossibili { Stato = archiviata.Id });
            pubblica.StatiPossibili.Add(new StatiPossibili { Stato = nascosta.Id });

            annullata.StatiPossibili.Add(new StatiPossibili { Stato = pubblica.Id });
            annullata.StatiPossibili.Add(new StatiPossibili { Stato = archiviata.Id });
            annullata.StatiPossibili.Add(new StatiPossibili { Stato = nascosta.Id });

            archiviata.StatiPossibili.Add(new StatiPossibili { Stato = pubblica.Id });
            archiviata.StatiPossibili.Add(new StatiPossibili { Stato = annullata.Id });
            archiviata.StatiPossibili.Add(new StatiPossibili { Stato = nascosta.Id });

            nascosta.StatiPossibili.Add(new StatiPossibili { Stato = pubblica.Id });
            nascosta.StatiPossibili.Add(new StatiPossibili { Stato = archiviata.Id });
            nascosta.StatiPossibili.Add(new StatiPossibili { Stato = annullata.Id });

            context.SaveChanges();




            var ruoli = new List<Profili>
            {
                new Profili{ Nome="adm",Descrizione="Amministratore", Compilazione=true, Esportazione=true, ModificaLista=true, IsActive=false, SoloPubbliche=false, TutteVisibili=true},
                new Profili{ Nome="pbl",Descrizione="Pubblicatore", Compilazione=false, Esportazione=true, ModificaLista=true, IsActive=false, SoloPubbliche=false},
                new Profili{ Nome="Compilatore",Descrizione="Compilatore", Compilazione=true, Esportazione=true, ModificaLista=false, IsActive=true, SoloPubbliche=true},
                new Profili{ Nome="Visualizzatore",Descrizione="Visualizzatore", Compilazione=false, Esportazione=true, ModificaLista=false, IsActive = true , SoloPubbliche=true},
                new Profili{ Nome="Monitoring",Descrizione="Monitoring", Compilazione=false, Esportazione=false, ModificaLista=false, IsActive=true, SoloPubbliche=true},
            };
            ruoli.ForEach(s => context.Profili.Add(s));
            context.SaveChanges();


            var emailcorpo = @"
    <html>
	<link href='{css}' rel='stylesheet' type='text/css'>
		
		<table border=0 cellspacing=1 cellpadding=0 align=center width='700'><tr><td><tbody>
		<table border=1 cellspacing=0 cellpadding=10 width='100%' bgcolor=white>
		<tr>
		<td class='desc_cell_c'>
	      		<TABLE BORDER=0 WIDTH='600' CELLSPACING=5 CELLPADDING=0 align=center> 
				<tr>
				<td align='right' colspan='2'><img src='http://portal.example.com/img/logo.gif' width=106 height=57><br><br></td>
				</tr>
                <tr>
                <td>
                Si comunica che è stata publicata la seguente Lista:
                </td>
                </tr>
				<tr>
				<td style='background-color:#dce5ea;font-size:11px;font-family:verdana;'>Nome Lista:</td>
				<td style='background-color:#dce5ea;font-size:11px;font-family:verdana;'><b>{nomelista}</b> </td>
				</tr>
				
	       		<tr>
				<td style='background-color:#dce5ea;font-size:11px;font-family:verdana;'> Struttura: </td>
				<td style='background-color:#F4F4F4;font-size:11px;font-family:verdana;'><b>{struttura}</b></td>
				</tr>
				<tr><td style='font-size:10px;font-family:verdana;' colspan=2>
                <a title='Link alla Lista' href = '{link}' target = '_blank' >Lista</a>
                </td></tr>
				<tr><td style='font-size:10px;font-family:verdana;' colspan=2>L'occasione &egrave; gradita per porgere distinti saluti.<br><br></td></tr>
				<tr><td style='font-size:10px;font-family:verdana;' colspan=2><i>Per favore, non rispondere a questa mail.</i></td></tr>
				
				</table>
		</td></tr></table>

	</tbody></td></tr></table>
	</html>
";

            var emailTesti = new EmailTesti
            {
                Nome = "Pubblicata",
                Oggetto = "Publicazione nuova Lista",
                Corpo = emailcorpo,
                Html = true,
                IsActive = true,
            };

            context.EmailTesti.Add(emailTesti);
            context.SaveChanges();



        }
    }

}
