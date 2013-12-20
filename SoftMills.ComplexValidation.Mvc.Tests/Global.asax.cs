using SoftMills.ComplexValidation.Mvc.Tests.Controllers;
using SoftMills.ComplexValidation.Mvc.Tests.Properties;
using System;
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

			// Initialize traditional validation attributes
			DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(StringLengthAttribute), typeof(CustomStringLengthAttributeAdapter));
		}

		protected void Application_BeginRequest(object source, EventArgs e) {
			ValidIfAttribute.AddRemoteValidatorsOnce(((HttpApplication) source).Context, url => {
				ValidIfAttribute.AddRemoteValidator(url.HomeDeliveryDateCheck(), HomeController.DeliveryDateCheckFunc);
				ValidIfAttribute.AddRemoteValidator(url.HomeToppingCountCheck(), HomeController.ToppingCountCheckFunc);
				ValidIfAttribute.AddRemoteValidator(url.HomeToppingsCheck(), HomeController.ToppingsCheckFunc);
			});
		}

		// Example of a traditional validation attribute adapter
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