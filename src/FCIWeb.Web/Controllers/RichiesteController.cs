using System;
using System.Collections.Generic;
using FCIWeb.Web.Data;
using FCIWeb.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StatoRichiesta = FCIWeb.Core.Models.StatoRichiesta;

namespace FCIWeb.Web.Controllers
{
    [Authorize]
    public class RichiesteController : Controller
    {
        private readonly IRichiestaStore _store;

        public RichiesteController(IRichiestaStore store)
        {
            _store = store;
        }

        [HttpGet]
        public IActionResult Index(string q, int page = 1, int pageSize = 10)
        {
            var result = _store.GetPage(q, page, pageSize);
            ViewBag.Query = q;
            return View(result);
        }

        [HttpGet]
        public IActionResult Details(Guid id)
        {
            var richiesta = _store.GetById(id);
            if (richiesta == null) return NotFound();
            ViewBag.Transizioni = Richiesta.TransizioniAmmesse(richiesta.Stato);
            return View(richiesta);
        }

        [HttpGet]
        public IActionResult ExportCsv(string q)
        {
            var records = new List<FCIWeb.Core.Models.Richiesta>();
            var firstPage = _store.GetPage(q, 1, 50);
            records.AddRange(firstPage.Items);
            for (var current = 2; current <= firstPage.TotalPages; current++)
                records.AddRange(_store.GetPage(q, current, 50).Items);

            var bytes = CsvExport.Serialize(records);
            return File(bytes, "text/csv; charset=utf-8", $"fciweb-richieste-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv");
        }

        [HttpGet]
        public IActionResult Create() => View(new RichiestaCreateModel());

        [HttpPost]
        public IActionResult Create(RichiestaCreateModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var richiesta = new Richiesta
            {
                NDG = model.NDG.Trim(),
                Intestazione = model.Intestazione.Trim(),
                UOCode = model.UOCode?.Trim(),
                Matricola = model.Matricola?.Trim() ?? User.Identity?.Name,
                Note = model.Note,
                Stato = StatoRichiesta.Bozza,
                DataCreazione = DateTime.UtcNow
            };

            _store.Add(richiesta);
            return RedirectToAction(nameof(Details), new { id = richiesta.Id });
        }

        [HttpPost]
        public IActionResult CambiaStato(Guid id, StatoRichiesta nuovo, string motivazione, Guid version)
        {
            if (!_store.TryChangeState(id, nuovo, motivazione, out var error, version))
            {
                TempData["Errore"] = error;
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public IActionResult Delete(Guid id)
        {
            _store.Delete(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
