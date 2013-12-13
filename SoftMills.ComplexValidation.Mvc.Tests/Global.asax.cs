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

			//RegisterClientValidatableAttribute<RequiredAttribute>();
			RegisterClientValidatableAttribute<RequiredIfAttribute>();
			RegisterClientValidatableAttribute<RequiredIfAbsentAttribute>();
			RegisterClientValidatableAttribute<EqualToAttribute>();
			RegisterClientValidatableAttribute<LessThanAttribute>();
			RegisterClientValidatableAttribute<LessThanOrEqualToAttribute>();
			RegisterClientValidatableAttribute<GreaterThanAttribute>();
			RegisterClientValidatableAttribute<GreaterThanOrEqualToAttribute>();
			DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(StringLengthAttribute), typeof(CustomStringLengthAttributeAdapter));
		}

		private static void RegisterClientValidatableAttribute<T>()
			where T : ValidationAttribute
		{
			DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(T), typeof(ClientValidatableAttributeAdapter<T>));
		}

		private class ClientValidatableAttributeAdapter<T> : DataAnnotationsModelValidator<T>
			where T : ValidationAttribute
		{
			public ClientValidatableAttributeAdapter(ModelMetadata metadata, ControllerContext context, T attribute)
				: base(metadata, context, attribute)
			{
				if (string.IsNullOrEmpty(attribute.ErrorMessage) &&
					string.IsNullOrEmpty(attribute.ErrorMessageResourceName) &&
					attribute.ErrorMessageResourceType == null)
				{
					attribute.ErrorMessageResourceType = typeof(Resources);
					var typeName = attribute.GetType().Name;
					attribute.ErrorMessageResourceName = "Validation_" +
						(typeName.EndsWith("Attribute") ? typeName.Substring(0, typeName.Length - 9) : typeName);
				}
			}
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