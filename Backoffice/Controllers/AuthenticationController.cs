using Backoffice.Services;
using Backoffice.ViewModels;
using Common;
using Common.Api.ExigoAdminWebService;
using ExigoService;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Backoffice.Controllers
{
    [AllowAnonymous]
    public class AuthenticationController : Controller
    {
        #region Signing in
        [Route("~/login")]
        public ActionResult Login()
        {
            var model = new LoginViewModel();

            if (GlobalSettings.Exigo.Api.CompanyKey == "exigodemo")
            {
                model.LoginName = "www";
                model.Password = "testpsswd";
            }

            return View(model);
        }

        [HttpPost]
        [Route("~/login")]
        public JsonNetResult Login(LoginViewModel model)
        {
            var service = new IdentityService();
            var response = service.SignIn(model.LoginName, model.Password);

            return new JsonNetResult(response);
        }

        [Route("~/silentlogin")]
        public ActionResult SilentLogin(string token)
        {
            var service = new IdentityService();
            var response = service.SignIn(token);

            if (response.Status)
            {
                var cookie = HttpContext.Request.Cookies[GlobalSettings.Backoffices.MonthlySubscriptionCookieName];
                if (cookie != null)
                {
                    cookie = new HttpCookie(GlobalSettings.Backoffices.MonthlySubscriptionCookieName);
                    cookie.Value = "true";
                    cookie.Expires = DateTime.Now.AddDays(-1);
                    HttpContext.Response.Cookies.Add(cookie);

                }
                //T.W. 6/1/2016 77187 This change ensures the correct landing page (when silent login to the back office) uses the filter 'BackofficeSubscriptionRequired'
                return Redirect(response.LandingUrl);
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        [Route("~/adminlogin")]
        public ActionResult AdminLogin(string token)
        {
            var service = new IdentityService();
            var response = service.AdminSilentLogin(token);

            if (response.Status)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            else
            {
                return RedirectToAction("Login");
            }
        }
        #endregion

        #region Signing Out
        [Route("~/logout")]
        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }
        #endregion
    }
}
