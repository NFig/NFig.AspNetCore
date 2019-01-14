using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NFig.AspNetCore.Sample.Views.Home;
using static NFig.AspNetCore.Sample.Configuration.Settings;

namespace NFig.AspNetCore.Sample.Controllers
{
    public class HomeController : Controller
    {
        private readonly SiteSettings _siteSettings;
        private readonly SiteSettings.NestedSettings _nestedSettings;
        private readonly SecretSettings _secretSettings;

        public HomeController(
            IOptions<SiteSettings> siteSettings,
            IOptions<SiteSettings.NestedSettings> nestedSettings,
            IOptions<SecretSettings> secretSettings
        )
        {
            _siteSettings = siteSettings.Value;
            _nestedSettings = nestedSettings.Value;
            _secretSettings = secretSettings.Value;
        }

        [Route("")]
        public IActionResult Index()
        {
            return View(new IndexModel(_siteSettings, _nestedSettings, _secretSettings));
        }
    }
}
