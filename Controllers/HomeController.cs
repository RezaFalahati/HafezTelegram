using System.Diagnostics;
using System.Threading.Tasks;
using HafezTelegram.DataSource;
using HafezTelegram.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TdLib;

namespace HafezTelegram.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly string PhoneNumber = "989301291048";

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index(string authenticationCode)
        {
            if (!string.IsNullOrEmpty(authenticationCode)) CheckAuthCode(authenticationCode);
            ViewData["ErrorMessage"] =
                $"Loop Count = {DataReceiveHelper.LoopCount} , Information = {DataReceiveHelper.Information}";
            return View();
        }

        public async Task<IActionResult> SendAuthCode()
        {
            try
            {
                using var client = await DataReceiveHelper.NewClientAsync();
                await client.ExecuteAsync(new TdApi.SetAuthenticationPhoneNumber
                {
                    PhoneNumber = PhoneNumber
                });
            }
            catch (TdException e)
            {
                var error = e.Error;
                ViewData["ErrorMessage"] = error.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult CheckLoopCount()
        {
            return RedirectToAction(nameof(Index));
        }

        public IActionResult StartThread()
        {
            try
            {
                DataReceiveHelper.IsAuthorised = true;
            }
            catch (TdException e)
            {
                var error = e.Error;
                ViewData["ErrorMessage"] = error.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult StopThread()
        {
            try
            {
                DataReceiveHelper.IsAuthorised = false;
            }
            catch (TdException e)
            {
                var error = e.Error;
                ViewData["ErrorMessage"] = error.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private async void CheckAuthCode(string authenticationCode)
        {
            try
            {
                using var client = await DataReceiveHelper.NewClientAsync();
                await client.ExecuteAsync(new TdApi.CheckAuthenticationCode
                {
                    Code = authenticationCode
                });
            }
            catch (TdException e)
            {
                var error = e.Error;
                ViewData["ErrorMessage"] = error.Message;
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}