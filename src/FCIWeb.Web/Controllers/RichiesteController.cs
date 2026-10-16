using System;
using FCIWeb.Web.Data;
using FCIWeb.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        public IActionResult Index(string q)
        {
            ViewBag.Query = q;
            return View(_store.GetAll(q));
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
        public IActionResult CambiaStato(Guid id, StatoRichiesta nuovo, string motivazione)
        {
            if (!_store.TryChangeState(id, nuovo, motivazione, out var error))
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
