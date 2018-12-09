using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Globalization.Models;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Localization;

namespace Globalization.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHtmlLocalizer<HomeController> _localizer;
        public IActionResult Index()
        {
            return View();
        }

        public HomeController(IHtmlLocalizer<HomeController> localizer)
        {
            this._localizer = localizer;
        }

        public IActionResult About()
        {
            ViewData["Message"] = _localizer["DescriptionPage"];

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = _localizer["ContactPage"];

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private string _currentLanguage;
        private string CurrentLanguage
        { get
            {
            if (!string.IsNullOrEmpty(_currentLanguage)) return _currentLanguage;
            if (string.IsNullOrEmpty(_currentLanguage))
            {
                var feature = HttpContext.Features.Get<IRequestCultureFeature>();
                _currentLanguage = feature.RequestCulture.Culture.TwoLetterISOLanguageName.ToLower();
            }
            return _currentLanguage; } }
        public ActionResult RedirectToDefaultCulture() { var culture = CurrentLanguage; if (culture != "en") culture = "en"; return RedirectToAction("Index", new { culture }); }
    }
}
