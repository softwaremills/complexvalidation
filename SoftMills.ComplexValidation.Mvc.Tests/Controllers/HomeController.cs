using SoftMills.ComplexValidation.Mvc.Tests.Models;
using System.Web.Mvc;

namespace SoftMills.ComplexValidation.Mvc.Tests.Controllers
{
	public class HomeController : Controller
	{
		public ActionResult Index()
		{
			var model = HomeIndexModel.Default;
			return View(model);
		}

		[HttpPost, ValidateAntiForgeryToken]
		public ActionResult Index(FormCollection form)
		{
			var model = new HomeIndexModel();
			TryUpdateModel(model, form);
			return View(model);
		}
	}
}