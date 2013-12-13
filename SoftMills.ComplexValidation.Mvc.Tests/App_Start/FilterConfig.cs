using System.Web.Mvc;

namespace SoftMills.ComplexValidation.Mvc.Tests.App_Start
{
	public class FilterConfig
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
		}
	}
}