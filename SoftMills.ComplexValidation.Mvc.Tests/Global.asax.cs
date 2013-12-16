using SoftMills.ComplexValidation.Mvc.Tests.App_Start;
using SoftMills.ComplexValidation.Mvc.Tests.Properties;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SoftMills.ComplexValidation.Mvc.Tests
{
	public class MvcApplication : HttpApplication
	{
		protected void Application_Start()
		{
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);

			ValidIfAttribute.DefaultErrorMessageResourceType = typeof(Resources);
			ValidIfAttribute.DefaultErrorMessageResourceNamePrefix = "Validation_";

			//ValidIfAttribute.RegisterAdapter<RequiredAttribute>();
			ValidIfAttribute.RegisterAdapter<RequiredIfAttribute>();
			ValidIfAttribute.RegisterAdapter<RequiredIfAbsentAttribute>();
			ValidIfAttribute.RegisterAdapter<EqualToAttribute>();
			ValidIfAttribute.RegisterAdapter<LessThanAttribute>();
			ValidIfAttribute.RegisterAdapter<LessThanOrEqualToAttribute>();
			ValidIfAttribute.RegisterAdapter<GreaterThanAttribute>();
			ValidIfAttribute.RegisterAdapter<GreaterThanOrEqualToAttribute>();

			DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(StringLengthAttribute), typeof(CustomStringLengthAttributeAdapter));
		}

		private class CustomStringLengthAttributeAdapter : StringLengthAttributeAdapter
		{
			public CustomStringLengthAttributeAdapter(ModelMetadata metadata, ControllerContext context, StringLengthAttribute attribute)
				: base(metadata, context, attribute)
			{
				if (string.IsNullOrEmpty(attribute.ErrorMessage) &&
					string.IsNullOrEmpty(attribute.ErrorMessageResourceName) &&
					attribute.ErrorMessageResourceType == null)
				{
					attribute.ErrorMessageResourceType = typeof(Resources);
					attribute.ErrorMessageResourceName = "Validation_StringLength";
				}
			}
		}
	}
}