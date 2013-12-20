using System.Web.Mvc;
using System.Web.Routing;

namespace SoftMills.ComplexValidation.Mvc.Tests {
	public class RouteConfig {
		public const string DefaultRoute = "Default";

		public static void RegisterRoutes(RouteCollection routes) {
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute(DefaultRoute, "{controller}/{action}/{id}",
				new { controller = "Home", action = "Index", id = UrlParameter.Optional });
		}
	}

	public static class RouteConfigExtensions {
		public static string ControllerAction(this UrlHelper url, string controller, string action) {
			return url.RouteUrl(RouteConfig.DefaultRoute, new { controller, action });
		}
	}
}