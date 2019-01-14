using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NFig.AspNetCore.Sample.Configuration;
using NFig.AspNetCore.Sample.Views.Home;
using static NFig.AspNetCore.Sample.Configuration.Settings;

namespace NFig.AspNetCore.Sample.Controllers
{
    public class SettingsController : Controller
    {
        [Route("settings/{*pathInfo}")]
        public Task Settings() => NFigMiddleware<Settings, Tier, DataCenter>.HandleRequestAsync(HttpContext);
    }
}
