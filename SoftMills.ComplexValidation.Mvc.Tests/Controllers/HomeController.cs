using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SoftMills.ComplexValidation.Mvc.Tests.Models;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SoftMills.ComplexValidation.Mvc.Tests.Controllers {

	public class HomeController : Controller {
		public const string Name = "home";

		public JsonResult DeliveryDateCheck(object[] args) {
			return Json(DeliveryDateCheckFunc(new object[] {DateTime.Parse((string) args[0])}));
		}

		public static object DeliveryDateCheckFunc(object[] args) {
			return !args[0].Equals(DateTime.Today.AddDays(1));
		}

		public ActionResult Index() {
			var model = HomeIndexModel.Default;
			return View(model);
		}

		[HttpPost, ValidateAntiForgeryToken]
		public ActionResult Index(FormCollection form) {
			var model = new HomeIndexModel();
			TryUpdateModel(model, form);
			return View(model);
		}

		public ActionResult Remote() {
			return View(new HomeRemoteModel(Url));
		}

		[HttpPost, ValidateAntiForgeryToken]
		public ActionResult Remote(FormCollection form) {
			var model = new HomeRemoteModel(Url);
			TryUpdateModel(model, form);
			return View(model);
		}

		public JsonResult ToppingCountCheck(object[] args) {
			return Json(ToppingCountCheckFunc(args));
		}

		public static object ToppingCountCheckFunc(object[] args) {
			return !args[0].Equals(1);
		}

		public JsonResult ToppingsCheck() {
			Request.InputStream.Seek(0, SeekOrigin.Begin);
			var args = JObject.Parse(new StreamReader(Request.InputStream).ReadToEnd())["args"] as JArray;
			return Json(args[0].All(t => t.Value<int>() != 1));
		}

		public static object ToppingsCheckFunc(object[] args) {
			var list = (object[]) args[0];
			return list.All(obj => !obj.Equals(1));
		}
	}

	public static class HomeControllerExtensions {
		private const string Name = HomeController.Name;

		public static string HomeDeliveryDateCheck(this UrlHelper url) {
			return url.ControllerAction(Name, "deliverydatecheck");
		}

		public static string HomeRemote(this UrlHelper url) {
			return url.ControllerAction(Name, "remote");
		}

		public static string HomeToppingCountCheck(this UrlHelper url) {
			return url.ControllerAction(Name, "toppingcountcheck");
		}

		public static string HomeToppingsCheck(this UrlHelper url) {
			return url.ControllerAction(Name, "toppingscheck");
		}
	}
}