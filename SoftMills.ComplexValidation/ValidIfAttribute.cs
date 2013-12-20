/*                                                    ______
 *                                       ___.--------'------`---------.____
 *                                 _.---'----------------------------------`---.__
 *                               .'___=]============================================
 *  ,-----------------------..__/.'         >--.______        _______.----'
 *  ]====================<==||(__)        .'          `------'
 *  `-----------------------`' ----.___--/
 *       /       /---'                 `/         Anthony J. Mills
 *      /_______(______________________/            ASP.NET MVC
 *      `-------------.--------------.'     Complex Validation Attribute
 *                      \________|_.-'              MIT License
 *
 * NuGet package: https://www.nuget.org/packages/SoftMills.ComplexValidation/
 * Project home: https://github.com/softwaremills/complexvalidation
 */

using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace SoftMills.ComplexValidation {

	public class RemoteArgs {
		public object[] args;
	}

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]  // AllowMultiple = false because of an MVC limitation. Thus, ValidIf2 etc.
	public class ValidIfAttribute : ValidationAttribute, IClientValidatable {
		private const string AdditionalValuesKey = "ValidIf";
		private const string ClientValidationPrefix = "v";

		private readonly string rule;
		private readonly string[] errorMessageFormatNames;     // Property names
		protected readonly string[] ErrorMessageDisplayNames;  // Label names

		private static IDictionary<string, Func<object[], object>> remoteValidators;
		private static bool initializedAlready = false;
		private static Object lockObject = new Object();

		/// <summary>
		/// One validation attribute to rule them all.
		/// </summary>
		/// <param name="rule">
		/// The rule specifying one facet of what is valid, in JSON format with single quotes. For example, "['gte','MinValue']".
		/// </param>
		/// <param name="errorMessageFormatNames">
		/// Names of additional referenced properties; the first will be {1}, the second {2}, etc., in the error message string ({0}
		/// is always the current field name).
		/// </param>
		public ValidIfAttribute(string rule, params string[] errorMessageFormatNames) {
			this.rule = rule;
			this.errorMessageFormatNames = errorMessageFormatNames;
			ErrorMessageDisplayNames = new string[errorMessageFormatNames.Length];
		}

		/// <summary>
		/// Adds a remote validator reference. This allows server-side validation of something like
		/// "['remote','SomeUrl','Parameter']". On the server side, the server can look up the value of SomeUrl and then call the
		/// validator that has been referenced.
		/// </summary>
		/// <param name="url">The URL of the validator.</param>
		/// <param name="validator">The validator function.</param>
		public static void AddRemoteValidator(string url, Func<object[], object> validator) {
			(remoteValidators ?? (remoteValidators = new Dictionary<string, Func<object[], object>>())).Add(url, validator);
		}

		/// <summary>
		/// Helper function to allow initializing remote validators on the first request. Should call this from Application_BeginRequest.
		/// </summary>
		/// <param name="context">If called from Application_BeginRequest, this should be the Context property of the source, which is an HttpApplication.</param>
		/// <param name="initializer">An initializer method, passed a UrlHelper. This should call AddRemoteValidator multiple times.</param>
		public static void AddRemoteValidatorsOnce(HttpContext context, Action<UrlHelper> initializer) {
			if (initializedAlready) return;
			lock (lockObject) {
				if (initializedAlready) return;
				initializer(new UrlHelper(context.Request.RequestContext));
				initializedAlready = true;
			}
		}

		/// <summary>
		/// Formats the error message for a given field name. The default implementation does a string.Format, supplying the name of
		/// the current field as {0} and the names of the other referenced fields (supplied as subsequent parameters to the
		/// attribute) as {1}, {2}, etc.
		/// </summary>
		/// <param name="name">The label name of the current field.</param>
		/// <returns>The final validation message.</returns>
		public override string FormatErrorMessage(string name) {
			return string.Format(ErrorMessageString, new object[] {name}.Concat(ErrorMessageDisplayNames).ToArray());
		}

		/// <summary>
		/// Evaluates the rule for the current object and judges the value valid if the rule evaluates to a "present" value (e.g.
		/// true, any non-empty string, any number, any date). Protected to allow descendants to override if necessary.
		/// </summary>
		/// <param name="value">The value to evaluate.</param>
		/// <param name="validationContext">The context, which includes the value to evaluate.</param>
		/// <returns></returns>
		protected override ValidationResult IsValid(object value, ValidationContext validationContext) {
			var viewData = new ViewDataDictionary(validationContext.ObjectInstance);
			for (var i = 0; i < errorMessageFormatNames.Length; i++) {
				ErrorMessageDisplayNames[i] = ErrorMessageDisplayNames[i] ?? GetDisplayName(errorMessageFormatNames[i], viewData);
			}
			var resolved = ResolveToken(JToken.Parse(rule), value, validationContext.ObjectInstance);
			return IsPresent(resolved) ? ValidationResult.Success : new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
		}

		/// <summary>
		/// Gets the client validation rules for the attribute. Automatically assigns the next client validation identifier (e.g.
		/// "v", "va", "vb", etc.).
		/// </summary>
		public virtual IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context) {
			for (var i = 0; i < errorMessageFormatNames.Length; i++) {
				ErrorMessageDisplayNames[i] = ErrorMessageDisplayNames[i] ??
					GetDisplayName(errorMessageFormatNames[i], context.Controller.ViewData);
			}
			var index = metadata.AdditionalValues.ContainsKey(AdditionalValuesKey)
				? (int) (metadata.AdditionalValues[AdditionalValuesKey] = (int) metadata.AdditionalValues[AdditionalValuesKey] + 1)
				: (int) (metadata.AdditionalValues[AdditionalValuesKey] = -1);
			var clientValidationRule = new ModelClientValidationRule {
				ErrorMessage = FormatErrorMessage(metadata.DisplayName),
				ValidationType = ClientValidationPrefix + (index == -1 ? "" : ((char) (index + 97)).ToString())
			};
			clientValidationRule.ValidationParameters.Add("rule", rule.Replace('\'', '\"'));
			return new[] {clientValidationRule};
		}

		private static object ResolveString(string str, object value, object container) {
			return str == string.Empty ? value : GetDependentPropertyValue(container, str);
		}

		private object ResolveToken(JToken token, object value, object container) {
			switch (token.Type) {
				case JTokenType.Array:
					return ResolveFunction((JArray) token, value, container);

				case JTokenType.String:
					return ResolveString(token.Value<string>(), value, container);

				case JTokenType.Boolean:
					return token.Value<bool>();

				case JTokenType.Float:
				case JTokenType.Integer:
					return token.Value<double>();

				default:
					return null;
			}
		}

		protected virtual object ResolveFunction(JArray args, object value, object container) {
			var func = args[0].ToString();
			switch (func) {
				case "remote": {
						var url = Convert.ToString(ResolveToken(args[1], value, container));
						var remoteArgs = args.Count < 3 ? new object[] { value } : args.Skip(2).Select(v => ResolveToken(v, value, container)).ToArray();
						Func<object[], object> validator;
						return remoteValidators != null && remoteValidators.TryGetValue(url, out validator) ? validator(remoteArgs) : null;
					}

				case "delay": {
						return args[2];
					}

				// true if all are present, false if any are absent
				case "all":
				case "and":
				case "present": {
						if (args.Count < 2)
							return IsPresent(value);
						for (var index = 1; index < args.Count; index++)
							if (!IsPresent(ResolveToken(args[index], value, container)))
								return false;
						return true;
					}

				// true if any are present, false if all are absent
				case "any":
				case "or": {
						if (args.Count < 2)
							return IsPresent(value);
						for (var index = 1; index < args.Count; index++)
							if (IsPresent(ResolveToken(args[index], value, container)))
								return true;
						return false;
					}

				// true if all are absent, false if any are present
				case "absent":
				case "not": {
						if (args.Count < 2)
							return !IsPresent(value);
						for (var index = 1; index < args.Count; index++)
							if (!IsPresent(ResolveToken(args[index], value, container)))
								return true;
						return false;
					}

				// true if any are absent, false if all are present
				case "anyabsent":
				case "nor": {
						if (args.Count < 2)
							return !IsPresent(value);
						for (var index = 1; index < args.Count; index++)
							if (IsPresent(ResolveToken(args[index], value, container)))
								return false;
						return true;
					}

				// 'if', (case, result)..., else result
				case "if": {
						for (var index = 1; index < args.Count - 2; index += 2)
							if (IsPresent(ResolveToken(args[index], value, container)))
								return ResolveToken(args[index + 1], value, container);
						return ResolveToken(args[args.Count - 1], value, container);
					}

				// returns the first 'present' value in the list
				case "coalesce": {
						for (var index = 1; index < args.Count; index++) {
							var current = ResolveToken(args[index], value, container);
							if (IsPresent(current))
								return current;
						}
						return null;
					}

				// returns the number of 'present' values in the list
				case "count": {
						var counter = 0;
						for (var index = 1; index < args.Count; index++) {
							if (IsPresent(ResolveToken(args[index], value, container)))
								counter++;
						}
						return counter;
					}

				case "contains": {
						var test = ResolveToken(args[args.Count - 1], value, container);
						var haystack = args.Count < 3 ? new[] {value} : args.Skip(1).Take(args.Count - 2).Select(t => ResolveToken(t, value, container)).ToArray();
						return haystack.Any(val => ArrayContains(val, test));
					}

				case "in": {
						var test = args.Count < 3 ? value : ResolveToken(args[1], value, container);
						var haystack = args.Skip(args.Count < 3 ? 1 : 2).Select(t => ResolveToken(t, value, container)).ToArray();
						return haystack.Any(val => ArrayContains(val, test));
					}

				// 'regex', regular expression - returns whether the value matches the regular expression 'regex', value, regular
				// expression - returns whether the specified value matches the regular expression
				case "regex": {
						var test = Convert.ToString(args.Count < 3 ? value : ResolveToken(args[1], value, container));
						var pattern = Convert.ToString(ResolveToken(args[args.Count < 3 ? 1 : 2], value, container));
						return Regex.IsMatch(test, "^" + pattern + "$", RegexOptions.ECMAScript);
					}

				// returns the first parameter as a string (doesn't try to treat it as a field name and resolve the value)
				case "str": {
						return Convert.ToString(args[1]);
					}

				// returns the array length or string length of the value or first parameter; null returns zero
				case "len": {
						var token = args.Count < 2 ? value : ResolveToken(args[1], value, container);
						if (token is IList) return ((IList) token).Count;
						var str = Convert.ToString(token) ?? string.Empty;
						return str.Length;
					}

				// resolves the given string, treating it as a field name and returning the value of that field
				case "value": {
						return ResolveString(Convert.ToString(args[1]), value, container);
					}

				// concatenates the given parameters into a string
				case "concat": {
						return string.Join(string.Empty, args.Skip(1).Select(a => ResolveToken(a, value, container)).Select(Convert.ToString));
					}

				// resolves the value of the first parameter, using it as the context for resolving the second parameter
				case "with": {
						return ResolveToken(args[2], value, GetDependentPropertyValue(container, Convert.ToString(args[1])));
					}

				// 'eq', other - is the inspected value equal to other? 'eq', first, second - are the two given values equal?
				// succeeds if either is null
				case "eq": {
						dynamic first = args.Count < 3 ? value : ResolveToken(args[1], value, container);
						if (first == null)
							return true;
						dynamic second = ResolveToken(args[args.Count < 3 ? 1 : 2], value, container);
						if (second == null)
							return true;
						return first == second;
					}

				// 'neq', other - is the inspected value not equal to other? 'neq', first, second - are the two given values not
				// equal? succeeds if either is null
				case "neq": {
						dynamic first = args.Count < 3 ? value : ResolveToken(args[1], value, container);
						if (first == null)
							return true;
						dynamic second = ResolveToken(args[args.Count < 3 ? 1 : 2], value, container);
						if (second == null)
							return true;
						return first != second;
					}

				// 'lt', other - is the inspected value less than other? 'lt', first, second - is first less than second? succeeds
				// if either is null
				case "lt": {
						dynamic first = args.Count < 3 ? value : ResolveToken(args[1], value, container);
						if (first == null)
							return true;
						dynamic second = ResolveToken(args[args.Count < 3 ? 1 : 2], value, container);
						if (second == null)
							return true;
						return first < second;
					}

				// 'lte', other - is the inspected value less than or equal to other? 'lte', first, second - is first less than or
				// equal to second? succeeds if either is null
				case "lte": {
						dynamic first = args.Count < 3 ? value : ResolveToken(args[1], value, container);
						if (first == null)
							return true;
						dynamic second = ResolveToken(args[args.Count < 3 ? 1 : 2], value, container);
						if (second == null)
							return true;
						return first <= second;
					}

				// 'gt', other - is the inspected value greater than other? 'gt', first, second - is first greater than second?
				// succeeds if either is null
				case "gt": {
						dynamic first = args.Count < 3 ? value : ResolveToken(args[1], value, container);
						if (first == null)
							return true;
						dynamic second = ResolveToken(args[args.Count < 3 ? 1 : 2], value, container);
						if (second == null)
							return true;
						return first > second;
					}

				// 'gte', other - is the inspected value greater than or equal to other? 'gte', first, second - is first greater
				// than or equal to second?
				case "gte": {
						dynamic first = args.Count < 3 ? value : ResolveToken(args[1], value, container);
						if (first == null)
							return true;
						dynamic second = ResolveToken(args[args.Count < 3 ? 1 : 2], value, container);
						if (second == null)
							return true;
						return first >= second;
					}

				default:
					throw new ArgumentException("Unknown function '" + args[0] + "' encountered trying to evaluate " + args + ".");
			}
		}

		protected virtual bool IsPresent(object value) {
			return value != null && ((value is string) ? !string.IsNullOrEmpty(value.ToString()) : !(value is bool) || (bool) value);
		}

		private static string GetDisplayName(string name, ViewDataDictionary viewData) {
			var metadata = ModelMetadata.FromStringExpression(name, viewData);
			return metadata != null ? metadata.DisplayName : null;
		}

		private static object GetDependentPropertyValue(object container, string dependentProperty) {
			var currentType = container.GetType();
			var value = container;

			var propertyPath = (dependentProperty ?? string.Empty).Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries);
			foreach (var propertyName in propertyPath) {
				var property = currentType.GetProperty(propertyName);
				if (property == null)
					return null;
				value = property.GetValue(value, null);
				currentType = property.PropertyType;
			}

			return value;
		}

		private static bool ArrayContains(object haystack, dynamic value) {
			if (haystack == null) return false;

			var enumerable = haystack as IEnumerable;
			if (enumerable != null) {
				foreach (dynamic item in enumerable) {
					try {
						if (item == value) return true;
					} catch (RuntimeBinderException) {
					}
				}
			}

			dynamic hay = haystack;
			try {
				if (hay == value) return true;
			} catch (RuntimeBinderException) {
			}
			return false;
		}

		// Registration of Default Error Messages Call ValidIfAttribute.RegisterAdapter<DescendantAttribute>() after setting the
		// defaults below.

		public static Type DefaultErrorMessageResourceType = null;
		public static string DefaultErrorMessageResourceNamePrefix = null;

		public static void RegisterAdapter<T>() where T : ValidationAttribute {
			DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(T), typeof(ClientValidatableAttributeAdapter<T>));
		}

		public class ClientValidatableAttributeAdapter<T> : DataAnnotationsModelValidator<T> where T : ValidationAttribute {
			public ClientValidatableAttributeAdapter(ModelMetadata metadata, ControllerContext context, T attribute)
				: base(metadata, context, attribute)
			{
				if (!string.IsNullOrEmpty(attribute.ErrorMessage) ||
					!string.IsNullOrEmpty(attribute.ErrorMessageResourceName) ||
					attribute.ErrorMessageResourceType != null ||
					DefaultErrorMessageResourceType == null ||
					DefaultErrorMessageResourceNamePrefix == null) return;

				attribute.ErrorMessageResourceType = DefaultErrorMessageResourceType;
				var typeName = attribute.GetType().Name;
				attribute.ErrorMessageResourceName = DefaultErrorMessageResourceNamePrefix +
					(typeName.EndsWith("Attribute") ? typeName.Substring(0, typeName.Length - 9) : typeName);
			}
		}
	}

	public class ValidIf2Attribute : ValidIfAttribute {
		public ValidIf2Attribute(string rule, params string[] errorMessageFormatNames) : base(rule, errorMessageFormatNames) { }
	}

	public class ValidIf3Attribute : ValidIfAttribute {
		public ValidIf3Attribute(string rule, params string[] errorMessageFormatNames) : base(rule, errorMessageFormatNames) { }
	}

	public class ValidIf4Attribute : ValidIfAttribute {
		public ValidIf4Attribute(string rule, params string[] errorMessageFormatNames) : base(rule, errorMessageFormatNames) { }
	}

	public class ValidIf5Attribute : ValidIfAttribute {
		public ValidIf5Attribute(string rule, params string[] errorMessageFormatNames) : base(rule, errorMessageFormatNames) { }
	}

	// Convenience attributes. Because it's generally a bit more readable to say [LessThan("Today")].
	public class RequiredIfAttribute : ValidIfAttribute {
		public RequiredIfAttribute(string other) : base("['if','" + HttpUtility.JavaScriptStringEncode(other) + "','',true]", other) { }
	}

	public class RequiredIfAbsentAttribute : ValidIfAttribute {
		public RequiredIfAbsentAttribute(string other) : base("['or','','" + HttpUtility.JavaScriptStringEncode(other) + "']", other) { }
	}

	public class EqualToAttribute : ValidIfAttribute {
		public EqualToAttribute(string other) : base("['eq','" + HttpUtility.JavaScriptStringEncode(other) + "']", other) { }
		public EqualToAttribute(int constant) : base("['eq'," + constant + "]", null) { ErrorMessageDisplayNames[0] = constant.ToString(); }
		public EqualToAttribute(double constant) : base("['eq'," + constant + "]", null) { ErrorMessageDisplayNames[0] = constant.ToString(); }
	}

	public class LessThanAttribute : ValidIfAttribute {
		public LessThanAttribute(string other) : base("['lt','" + HttpUtility.JavaScriptStringEncode(other) + "']", other) { }
		public LessThanAttribute(int constant) : base("['lt'," + constant + "]", null) { ErrorMessageDisplayNames[0] = constant.ToString(); }
		public LessThanAttribute(double constant) : base("['lt'," + constant + "]", null) { ErrorMessageDisplayNames[0] = constant.ToString(); }
	}

	public class LessThanOrEqualToAttribute : ValidIfAttribute {
		public LessThanOrEqualToAttribute(string other) : base("['lte','" + HttpUtility.JavaScriptStringEncode(other) + "']", other) { }
		public LessThanOrEqualToAttribute(int constant) : base("['lte'," + constant + "]", null) { ErrorMessageDisplayNames[0] = constant.ToString(); }
		public LessThanOrEqualToAttribute(double constant) : base("['lte'," + constant + "]", null) { ErrorMessageDisplayNames[0] = constant.ToString(); }
	}

	public class GreaterThanAttribute : ValidIfAttribute {
		public GreaterThanAttribute(string other) : base("['gt','" + HttpUtility.JavaScriptStringEncode(other) + "']", other) { }
		public GreaterThanAttribute(int constant) : base("['gt'," + constant + "]", null) { ErrorMessageDisplayNames[0] = constant.ToString(); }
		public GreaterThanAttribute(double constant) : base("['gt'," + constant + "]", null) { ErrorMessageDisplayNames[0] = constant.ToString(); }
	}

	public class GreaterThanOrEqualToAttribute : ValidIfAttribute {
		public GreaterThanOrEqualToAttribute(string other) : base("['gte','" + HttpUtility.JavaScriptStringEncode(other) + "']", other) { }
		public GreaterThanOrEqualToAttribute(int constant) : base("['gte'," + constant + "]", null) { ErrorMessageDisplayNames[0] = constant.ToString(); }
		public GreaterThanOrEqualToAttribute(double constant) : base("['gte'," + constant + "]", null) { ErrorMessageDisplayNames[0] = constant.ToString(); }
	}
}