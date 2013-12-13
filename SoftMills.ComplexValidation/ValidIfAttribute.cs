/*                                                   ______
|                                       ___.--------'------`---------.____
|                                 _.---'----------------------------------`---.__
|                               .'___=]============================================
|  ,-----------------------..__/.'         >--.______        _______.----'
|  ]====================<==||(__)        .'          `------'
|  `-----------------------`' ----.___--/
|       /       /---'                 `/         Anthony J. Mills
|      /_______(______________________/            ASP.NET MVC
|      `-------------.--------------.'     Complex Validation Attribute
|                      \________|_.-'              BSD License

So, here's the basic idea. Your current validation rules might look like this:

[RegularExpression("[0-9]{5}")]
public string ZipCode { get; set; }

But what if, instead, they looked like this?

[ValidIf("['regex','[0-9]{5}']")]
public string ZipCode { get; set; }

Big deal, you might say. But what if we make it a little ... fancier? How much code do you currently have to write to implement
this?

[ValidIf("['if'," +
	"['eq','CountryId','UsaId'],['regex','[0-9]{5}']," +
	"['eq','CountryId','CanadaId'],['regex','[A-Za-z][0-9][A-Za-z] ?[0-9][A-Za-z][0-9]']," +
	"true]", ErrorMessage = "{0} must be a valid zip code or postal code.")]
Or, perhaps, this:

[ValidIf("['or',['not',['eq','CountryId','UsaId']],['regex','[0-9]{5}']]",
	ErrorMessage = "{0} must be a valid zip code.")]
[ValidIf2("['if',['eq','CountryId','CanadaId'],['regex','[A-Za-z][0-9][A-Za-z] ?[0-9][A-Za-z][0-9]'],true]",
	ErrorMessage = "{0} must be a valid postal code.")]
(In other words, if the value of the CountryId field is equal to the value of the UsaId field, then it's valid if it matches a US
post office regular expression, but if the value of the CountryId field is equal to the value of the CanadaId field, then it's valid
if it matches a Canadian post office regular expression. If the CountryId field is something else, it's valid, period.)

It's basically JSON. Arrays are function calls. The first array element is the name of the function, and the rest of the elements
are parameters to the function. Strings are usually interpreted as form element names, and they evaluate to the given element's
value. This depends on the function, though; the regex function above expects its pattern parameter to be a string, not an element
reference. Though you could put a regular expression string in a form element and use ['regex',['value','RegexPattern']]. To go the
other way, you do something like ['eq',['str','Joe']]. This is valid when the current field is equal to "Joe", not whatever is
currently in the field named "Joe".

The language geeks among you might think, "Hey, that looks like Lisp!" Well, yeah, it pretty much is. It's a really simple Lisp
dedicated to doing validations. It's got lots of functions relating to validating, and you can even define your own pretty easily.

Since rules are JSON strings. This allows really easy parsing on the client side, meaning every single validation method is
supported client-side as well!

Ok, you say, but what if I want multiple validations? I can only use a given attribute once. It's an ASP.NET MVC limitation.

No problem:
[ValidIf("['gte','MinValue']", ErrorMessage = "{0} must be at least the minimum.")]
[ValidIf2("['lte','MaxValue']", ErrorMessage = "{0} must be at most the maximum.")]
public int ChosenValue { get; set; }

Basically, ValidIf2Attribute is a simple descendent of ValidIfAttribute.

Now, if you're paying attention, you're wondering: if you can create any descendant, does the client-side validation still work? Yes!
ValidIfAttribute keeps track itself. The first ValidIfAttribute will get a "v" client-side identifier, the second will get a "va", the
third gets "vb", the fourth gets "vc", etc.

As shipped, you can use [ValidIf()] through [ValidIf5()]. If you need more than five validations on any particular property, just
define another descendant and have the client-side validation register the next few client-side identifiers (it's easy).

Oh, also, some convenience attributes have been defined that make common use cases a little easier. Also, this makes it easier to
specify a default error message string:

public class MatchZipCodeAttribute : ValidIfAttribute
{
	public MatchZipCodeAttribute(string countryIdName, string usaIdName, string canadaIdName)
		: base("['if'," +
			"['eq','" + countryIdName + "','" + usaIdName + "'],['regex','[0-9]{5}']," +
			"['eq','" + countryIdName + "','" + canadaIdName + "'],['regex','[A-Za-z][0-9][A-Za-z] ?[0-9][A-Za-z][0-9]']," +
			"true]")
	{
		ErrorMessage = Resources.ValidationMatchZipCodeAttribute;  // {0}
*/

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace SoftMills.ComplexValidation {

	public class ValidIf2Attribute : ValidIfAttribute {
		public ValidIf2Attribute(string rule, params string[] errorMessageFormatNames) : base(rule, errorMessageFormatNames) {} }

	public class ValidIf3Attribute : ValidIfAttribute {
		public ValidIf3Attribute(string rule, params string[] errorMessageFormatNames) : base(rule, errorMessageFormatNames) {} }

	public class ValidIf4Attribute : ValidIfAttribute {
		public ValidIf4Attribute(string rule, params string[] errorMessageFormatNames) : base(rule, errorMessageFormatNames) {} }

	public class ValidIf5Attribute : ValidIfAttribute {
		public ValidIf5Attribute(string rule, params string[] errorMessageFormatNames) : base(rule, errorMessageFormatNames) {} }

	// Convenience attributes. Because it's generally a bit more readable to say [LessThan("Today")].
	//public class RequiredAttribute : ValidIfAttribute { public RequiredAttribute() : base("''") {} }  // No point in redefining the classic RequiredAttribute.
	public class RequiredIfAttribute : ValidIfAttribute {
		public RequiredIfAttribute(string other) : base("['if','" + HttpUtility.JavaScriptStringEncode(other) + "','',true]", other) {} }

	public class RequiredIfAbsentAttribute : ValidIfAttribute {
		public RequiredIfAbsentAttribute(string other) : base("['or','" + HttpUtility.JavaScriptStringEncode(other) + "','']", other) {} }

	public class EqualToAttribute : ValidIfAttribute {
		public EqualToAttribute(string other) : base("['eq','" + HttpUtility.JavaScriptStringEncode(other) + "']", other) {}
		public EqualToAttribute(int constant) : base("['eq'," + constant + "]", null) { ErrorMessageDisplayNames[0] = constant.ToString(); }
		public EqualToAttribute(double constant) : base("['eq'," + constant + "]", null) { ErrorMessageDisplayNames[0] = constant.ToString(); } }

	public class LessThanAttribute : ValidIfAttribute {
		public LessThanAttribute(string other) : base("['lt','" + HttpUtility.JavaScriptStringEncode(other) + "']", other) {}
		public LessThanAttribute(int constant) : base("['lt'," + constant + "]", null) { ErrorMessageDisplayNames[0] = constant.ToString(); }
		public LessThanAttribute(double constant) : base("['lt'," + constant + "]", null) { ErrorMessageDisplayNames[0] = constant.ToString(); } }

	public class LessThanOrEqualToAttribute : ValidIfAttribute {
		public LessThanOrEqualToAttribute(string other) : base("['lte','" + HttpUtility.JavaScriptStringEncode(other) + "']", other) {}
		public LessThanOrEqualToAttribute(int constant) : base("['lte'," + constant + "]", null) { ErrorMessageDisplayNames[0] = constant.ToString(); }
		public LessThanOrEqualToAttribute(double constant) : base("['lte'," + constant + "]", null) { ErrorMessageDisplayNames[0] = constant.ToString(); } }

	public class GreaterThanAttribute : ValidIfAttribute {
		public GreaterThanAttribute(string other) : base("['gt','" + HttpUtility.JavaScriptStringEncode(other) + "']", other) {}
		public GreaterThanAttribute(int constant) : base("['gt'," + constant + "]", null) { ErrorMessageDisplayNames[0] = constant.ToString(); }
		public GreaterThanAttribute(double constant) : base("['gt'," + constant + "]", null) { ErrorMessageDisplayNames[0] = constant.ToString(); } }

	public class GreaterThanOrEqualToAttribute : ValidIfAttribute {
		public GreaterThanOrEqualToAttribute(string other) : base("['gte','" + HttpUtility.JavaScriptStringEncode(other) + "']", other) {}
		public GreaterThanOrEqualToAttribute(int constant) : base("['gte'," + constant + "]", null) { ErrorMessageDisplayNames[0] = constant.ToString(); }
		public GreaterThanOrEqualToAttribute(double constant) : base("['gte'," + constant + "]", null) { ErrorMessageDisplayNames[0] = constant.ToString(); } }

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class ValidIfAttribute : ValidationAttribute, IClientValidatable {
		private const string AdditionalValuesKey = "ValidIf";
		private const string ClientValidationPrefix = "v";

		private readonly string rule;
		private readonly string[] errorMessageFormatNames;
		protected readonly string[] ErrorMessageDisplayNames;

		private static IDictionary<string, Func<object[], bool>> remoteValidators;

		public ValidIfAttribute(string rule, params string[] errorMessageFormatNames) {
			this.rule = rule;
			this.errorMessageFormatNames = errorMessageFormatNames;
			ErrorMessageDisplayNames = new string[errorMessageFormatNames.Length];
		}

		public static void AddRemoteValidator(string url, Func<object[], bool> validator) {
			(remoteValidators ?? (remoteValidators = new Dictionary<string, Func<object[], bool>>())).Add(url, validator);
		}

		public override string FormatErrorMessage(string name) {
			return string.Format(ErrorMessageString, new object[] {name}.Concat(ErrorMessageDisplayNames).ToArray());
		}

		private static string GetDisplayName(string name, ViewDataDictionary viewData) {
			var metadata = ModelMetadata.FromStringExpression(name, viewData);
			return metadata != null ? metadata.DisplayName : null;
		}

		protected override ValidationResult IsValid(object value, ValidationContext validationContext) {
			var viewData = new ViewDataDictionary(validationContext.ObjectInstance);
			for (var i = 0; i < errorMessageFormatNames.Length; i++) {
				ErrorMessageDisplayNames[i] = ErrorMessageDisplayNames[i] ?? GetDisplayName(errorMessageFormatNames[i], viewData);
			}
			var resolved = ResolveToken(JToken.Parse(rule), value, validationContext.ObjectInstance);
			return IsPresent(resolved) ? ValidationResult.Success : new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
		}

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
					var remoteArgs = args.Skip(2).Select(v => ResolveToken(v, value, container)).ToArray();
					Func<object[], bool> validator;
					return remoteValidators != null && remoteValidators.TryGetValue(url, out validator) && validator(remoteArgs);
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

					// 'regex', regular expression - returns whether the value matches the regular expression
					// 'regex', value, regular expression - returns whether the specified value matches the regular expression
				case "regex": {
					var test = Convert.ToString(args.Count < 3 ? value : ResolveToken(args[1], value, container));
					var pattern = Convert.ToString(ResolveToken(args[args.Count < 3 ? 1 : 2], value, container));
					return Regex.IsMatch(test, "^" + pattern + "$", RegexOptions.ECMAScript);
				}

					// returns the first parameter as a string (doesn't try to treat it as a field name and resolve the value)
				case "str": {
					return Convert.ToString(args[1]);
				}

					// returns the string length of the first parameter; null returns zero
				case "len": {
					var str = Convert.ToString(ResolveToken(args[1], value, container)) ?? string.Empty;
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

					// 'eq', other - is the inspected value equal to other?
					// 'eq', first, second - are the two given values equal?
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

					// 'neq', other - is the inspected value not equal to other?
					// 'neq', first, second - are the two given values not equal?
					// succeeds if either is null
				case "neq": {
					dynamic first = args.Count < 3 ? value : ResolveToken(args[1], value, container);
					if (first == null)
						return true;
					dynamic second = ResolveToken(args[args.Count < 3 ? 1 : 2], value, container);
					if (second == null)
						return true;
					return first != second;
				}

					// 'lt', other - is the inspected value less than other?
					// 'lt', first, second - is first less than second?
					// succeeds if either is null
				case "lt": {
					dynamic first = args.Count < 3 ? value : ResolveToken(args[1], value, container);
					if (first == null)
						return true;
					dynamic second = ResolveToken(args[args.Count < 3 ? 1 : 2], value, container);
					if (second == null)
						return true;
					return first < second;
				}

					// 'lte', other - is the inspected value less than or equal to other?
					// 'lte', first, second - is first less than or equal to second?
					// succeeds if either is null
				case "lte": {
					dynamic first = args.Count < 3 ? value : ResolveToken(args[1], value, container);
					if (first == null)
						return true;
					dynamic second = ResolveToken(args[args.Count < 3 ? 1 : 2], value, container);
					if (second == null)
						return true;
					return first <= second;
				}

					// 'gt', other - is the inspected value greater than other?
					// 'gt', first, second - is first greater than second?
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

					// 'gte', other - is the inspected value greater than or equal to other?
					// 'gte', first, second - is first greater than or equal to second?
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

		private static object ResolveString(string str, object value, object container) {
			return str == string.Empty ? value : GetDependentPropertyValue(container, str);
		}

		private static object GetDependentPropertyValue(object container, string dependentProperty) {
			var currentType = container.GetType();
			var value = container;

			foreach (var propertyName in (dependentProperty ?? string.Empty).Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries)) {
				var property = currentType.GetProperty(propertyName);
				if (property == null)
					return null;
				value = property.GetValue(value, null);
				currentType = property.PropertyType;
			}

			return value;
		}
	}
}