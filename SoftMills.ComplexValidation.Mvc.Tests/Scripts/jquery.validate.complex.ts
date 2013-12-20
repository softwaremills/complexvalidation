/*! SoftMills.ComplexValidation v0.1.1 | (c) 2013 Anthony Mills | MIT license */

/*                                                   ______
|                                       ___.--------'------`---------.____
|                                 _.---'----------------------------------`---.__
|                               .'___=]============================================
|  ,-----------------------..__/.'         >--.______        _______.----'
|  ]====================<==||(__)        .'          `------'
|  `-----------------------`' ----.___--/
|       /       /---'                 `/        The Software Mills
|      /_______(______________________/            ASP.NET MVC
|      `-------------.--------------.'     Complex Validation Attribute
|                      \________|_.-'              BSD License

See documentation in the C# code for usage.

Here's how the JavaScript code works. The init() function at the bottom registers handlers with MVC's unobtrusive validation (and
jQuery Validation as a result). By default, five handlers are registered, allowing five validations per input field. If you need
more, just add more validation names to validatorNamesToRegister.

When the validator method is called, it basically attempts to evaluate the rule given the value of the current element. It does this
using some pretty normal recursive techniques, and it tries to short-circuit where possible. If there are no asynchronous elements
(generally AJAX calls are the only ones you expect) then it's not too difficult to figure out.

Note: jQuery Validate does not seem to be programmed with the ideas of taking the sort of abuse necessary to implement complex async
validations of the sort that are possible here. I would recommend that you restrict yourself to a single "remote" at the root.
*/

declare var jQuery;

// The result of a function call can be either a value or a list of deferred calls to make necessary to determine the value. Once
// the evaluator does its evaluation, it will have a list of deferred calls to make. It initiates those calls, then waits for
// responses. When each response comes back, it reevaluates the expression, possibly finding further calls to make, or possibly
// figuring out the answer. Once it knows the answer(e.g. if an expression is X AND Y, and you know that Y is false, you know the
// result is false and you can cancel evaluating X), it can abort the remaining calls and return the value.
interface JqvcDeferredCalls {
	isDeferred: boolean;
	calls: any[][];
}

// When doing async resolution, when we get values back, we keep a list of substitutions to make (i.e. "when you see this array, use
// this value instead of evaluating the call") so we don't make those calls again.
interface JqvcSubstitution {
	insteadOf: any[];
	use: any;
}

// In our async resolution, you can have async queries in flight when you get a result. At that point, the unnecessary bits need to
// be aborted, because we don't care about the results.
interface JqvcAbortableObject {
	abort: () => void;
}

// This packages the rule and a list of abortable objects involved in the async resolution of the current query. If a new query
// comes in, we need to abort the calls from the previous query before proceeding.
interface JqvcValidationParams {
	rule: any;
	abortable: JqvcAbortableObject[];
}

!function ($) {
	// Add subsequent elements in the chain if you need more validators
	var validatorNamesToRegister = ["v", "va", "vb", "vc", "vd"];

	var pending: JqvcDeferredCalls = { isDeferred: true, calls: [] };  // just used as a token

	var getDeferred = function (...args: any[]): JqvcDeferredCalls {
		var async: JqvcDeferredCalls = null;
		for (var i = 0, len = args.length; i < len; i++) {
			var item = args[i];
			if (isDeferred(item)) {
				if (async) {
					async.calls = async.calls.concat(item.calls);
				} else {
					async = item;
				}
			}
		}
		return async;
	};

	var resolveArray = function (array: any[], value: any, element: HTMLInputElement, prefix: string, substitutions: JqvcSubstitution[]) {
		for (var i = 0, len = substitutions.length; i < len; i++)
			if (array === substitutions[i].insteadOf)
				return substitutions[i].use;

		switch (array[0]) {
			// Async calls
			case "remote":
			case "delay":
				var syncCall: any[] = [array], asyncCalls: any[][] = null;
				for (var i = 1, len = array.length; i < len; i++) {
					var val = resolveToken(array[i], value, element, prefix, substitutions);
					if (isDeferred(val)) {
						asyncCalls = (asyncCalls || []).concat(val.calls);
					} else {
						syncCall.push(val);
					}
				}
				return <JqvcDeferredCalls> { isDeferred: true, calls: async || [syncCall] };

			// Presence checks
			case "all":
			case "and":
			case "present":
				if (array.length < 2) return isPresent(value);
				var async: JqvcDeferredCalls = null;
				for (var i = 1, len = array.length; i < len; i++) {
					var val = resolveToken(array[i], value, element, prefix, substitutions);
					async = getDeferred(async, val);
					if (!isDeferred(val) && !isPresent(val))
						return false;
				}
				return async || true;
			case "any":
			case "or":
				if (array.length < 2) return isPresent(value);
				var async: JqvcDeferredCalls = null;
				for (var i = 1, len = array.length; i < len; i++) {
					var val = resolveToken(array[i], value, element, prefix, substitutions);
					async = getDeferred(async, val);
					if (!isDeferred(val) && isPresent(val))
						return true;
				}
				return async || false;
			case "absent":
			case "not":
				if (array.length < 2) return !isPresent(value);
				var async: JqvcDeferredCalls = null;
				for (var i = 1, len = array.length; i < len; i++) {
					var val = resolveToken(array[i], value, element, prefix, substitutions);
					async = getDeferred(async, val);
					if (!isDeferred(val) && !isPresent(val))
						return true;
				}
				return async || false;
			case "anyabsent":
			case "nor":
				if (array.length < 2) return !isPresent(value);
				var async: JqvcDeferredCalls = null;
				for (var i = 1, len = array.length; i < len; i++) {
					var val = resolveToken(array[i], value, element, prefix, substitutions);
					async = getDeferred(async, val);
					if (!isDeferred(val) && isPresent(val))
						return false;
				}
				return async || true;

			// Utilities
			case "if":
				var async: JqvcDeferredCalls = null;
				for (var i = 1, len = array.length - 2; i < len; i += 2) {
					var checkVal = resolveToken(array[i], value, element, prefix, substitutions);
					if (isDeferred(checkVal)) {
						async = getDeferred(async, checkVal, resolveToken(array[i + 1], value, element, prefix, substitutions));
					} else {
						if (isPresent(checkVal)) {
							var result = resolveToken(array[i + 1], value, element, prefix, substitutions);
							async = getDeferred(async, result);
							return async || result;
						}
					}
				}
				var result = resolveToken(array[len + 1], value, element, prefix, substitutions);
				return getDeferred(async, result) || result;
			case "coalesce":
				var async: JqvcDeferredCalls = null;
				for (var i = 1, len = array.length; i < len; i++) {
					var val = resolveToken(array[i], value, element, prefix, substitutions);
					async = getDeferred(async, val);
					if (!isDeferred(val) && isPresent(val))
						return async || value;
				}
				return async || null;
			case "count":
				var async: JqvcDeferredCalls = null;
				for (var i = 1, counter = 0, len = array.length; i < len; i++) {
					var token = resolveToken(array[i], value, element, prefix, substitutions);
					async = getDeferred(async, token);
					if (!isDeferred(token) && isPresent(token))
						counter++;
				}
				return async || counter;
			case "contains":
				var test = resolveToken(array[array.length < 3 ? 1 : 2], value, element, prefix, substitutions);
				var haystack: any[] = array.length < 3 ? value : resolveToken(array[1], value, element, prefix, substitutions);
				return haystack.some(t => t == test);
			case "in":
				var test = array.length < 3 ? value : resolveToken(array[1], value, element, prefix, substitutions);
				var haystack: any[] = array.slice(array.length < 3 ? 1 : 2).map(t => resolveToken(t, value, element, prefix, substitutions));
				return haystack.some(t => t == test);
			case "regex":
				var test = array.length < 3 ? value : resolveToken(array[1], value, element, prefix, substitutions);
				var pattern = resolveToken(array[array.length < 3 ? 1 : 2], value, element, prefix, substitutions);
				return getDeferred(test, pattern) || new RegExp(makeString(pattern)).test(makeString(test));
			case "str":
				if (typeof array[1] === "string")
					return array[1];
				var token = resolveToken(array[1], value, element, prefix, substitutions);
				return getDeferred(token) || makeString(token);
			case "len":
				var token = array.length < 2 ? value : resolveToken(array[1], value, element, prefix, substitutions);
				return getDeferred(token) || $.isArray(token) ? token.length : (makeString(token) || "").length;
			case "value":
				var token = resolveToken(array[1], value, element, prefix, substitutions);
				return getDeferred(token) || resolveString(makeString(token), value, element, prefix);
			case "concat":
				var str = "", async: JqvcDeferredCalls = null;
				for (var i = 1, len = array.length; i < len; i++) {
					var token = resolveToken(array[i], value, element, prefix, substitutions);
					async = getDeferred(async, token);
					if (!isDeferred(token))
						str += makeString(token);
				}
				return async || str;
			case "with":
				var prefixToken = resolveToken(array[1], value, element, prefix, substitutions);
				return getDeferred(prefixToken) || resolveToken(array[2], value, element, makeString(prefixToken) + ".", substitutions);

			// Equivalence
			case "eq":
				var first = array.length < 3 ? value : resolveToken(array[1], value, element, prefix, substitutions);
				if (first == null) return true;
				var second = resolveToken(array[array.length < 3 ? 1 : 2], value, element, prefix, substitutions);
				if (second == null) return true;
				return getDeferred(first, second) || first === second;
			case "neq":
				first = array.length < 3 ? value : resolveToken(array[1], value, element, prefix, substitutions);
				if (first == null) return true;
				second = resolveToken(array[array.length < 3 ? 1 : 2], value, element, prefix, substitutions);
				if (second == null) return true;
				return getDeferred(first, second) || first !== second;
			case "lt":
				first = array.length < 3 ? value : resolveToken(array[1], value, element, prefix, substitutions);
				if (first == null) return true;
				second = resolveToken(array[array.length < 3 ? 1 : 2], value, element, prefix, substitutions);
				if (second == null) return true;
				return getDeferred(first, second) || first < second;
			case "lte":
				first = array.length < 3 ? value : resolveToken(array[1], value, element, prefix, substitutions);
				if (first == null) return true;
				second = resolveToken(array[array.length < 3 ? 1 : 2], value, element, prefix, substitutions);
				if (second == null) return true;
				return getDeferred(first, second) || first <= second;
			case "gt":
				first = array.length < 3 ? value : resolveToken(array[1], value, element, prefix, substitutions);
				if (first == null) return true;
				second = resolveToken(array[array.length < 3 ? 1 : 2], value, element, prefix, substitutions);
				if (second == null) return true;
				return getDeferred(first, second) || first > second;
			case "gte":
				first = array.length < 3 ? value : resolveToken(array[1], value, element, prefix, substitutions);
				if (first == null) return true;
				second = resolveToken(array[array.length < 3 ? 1 : 2], value, element, prefix, substitutions);
				if (second == null) return true;
				return getDeferred(first, second) || first >= second;

			// Error handling
			default:
				throw new Error("Unknown function '" + array[0] + "' in function call '" + JSON.stringify(array) + "'.");
		}
	};

	var resolveDataType = function (value) {
		if (typeof value !== "string") {
			return $.isArray(value) ? value.map(v => resolveDataType(v)) : value;
		} else if (value === "true" || value === "True") {
			return true;
		} else if (value === "false" || value === "False") {
			return false;
		} else if ((value - 0) == value && value.length > 0) {
			return parseFloat(value);
		} else {
			var dateValue = new Date(value);
			if (!isNaN(<any> dateValue)) return dateValue;
		}
		return value;
	};

	var resolveString = function (str: string, value: any, element: HTMLInputElement, prefix: string) {
		if (str === "") return value;
		var i, len;
		var pos = element.name.lastIndexOf(".") + 1;
		var name = element.name.substr(0, pos) + prefix + str;
		var elements = document.getElementsByName(name);
		if (elements.length === 0) throw new Error("Cannot resolve referenced element named '" + name + "'.");
		for (i = 0, len = elements.length; i < len; i++) {
			var el = <HTMLInputElement> elements[i];
			if (el.type === "checkbox") return el.checked;
			else if (el.type === "radio") { if (el.checked) return resolveDataType(el.value); }
			else return resolveDataType($(el).val());
		}
		return null;
	};

	var resolveToken = function (token: any, value: any, element: HTMLInputElement, prefix: string, substitutions: JqvcSubstitution[]) {
		if (token instanceof Array) return resolveArray(token, value, element, prefix, substitutions);
		if (typeof (token) === "string") return resolveString(token, value, element, prefix);
		return token;
	};

	var isPresent = function (value: any) {
		return $.isArray(value) ? !!value.length : !!value ? value !== "false" && value !== "False" : value === 0;
	};

	var makeString = function (obj: any) {
		return obj == null ? "" : "" + obj;
	};

	var isDeferred = function (value): boolean {
		return value && value.isDeferred;
	};

	var resolveArrayAsync = function (array: any[], value: any, callback: (any) => void): JqvcAbortableObject {
		switch (array[0][0]) {
			case "remote":
				return $.ajax({
					type: "POST",
					url: array[1],
					contentType: "application/json",
					dataType: "json",
					data: JSON.stringify({ args: array.length < 3 ? [value] : array.slice(2) })
				}).done(function (result) {
						callback(resolveDataType(result));
					});
			case "delay":
				var id = window.setTimeout(function () {
					callback(array[2]);
				}, array[1]);
				return {
					abort: function () {
						window.clearTimeout(id);
					}
				};
			default:
				throw new Error("Unknown asynchronous function '" + array[0] + "' in function call '" + JSON.stringify(array) + "'.");
		}
	};

	var launchAsync = function (rule: any[], call: any[], value: any, element: HTMLInputElement, substitutions: JqvcSubstitution[], abortable: JqvcAbortableObject[], callback: (boolean) => void) {
		var substitution: JqvcSubstitution = { insteadOf: call[0], use: pending };
		substitutions.push(substitution);
		var abortableCall = resolveArrayAsync(call, value, function (asyncResult) {
			for (var i = abortable.length - 1; i >= 0; i--)
				if (abortable[i] === abortableCall)
					abortable.splice(i, 1);

			substitution.use = asyncResult;
			var result = resolveToken(rule, value, element, "", substitutions);
			if (isDeferred(result)) {
				for (var i = 0, len = result.calls.length; i < len; i++) {
					launchAsync(rule, result.calls[i], value, element, substitutions, abortable, callback);
				}
			} else {
				//console.log("Result: " + result + " " + typeof result);
				callback(isPresent(result));
			}
		});
		abortable.push(abortableCall);
	};

	var validatorMethod = function (validatorName: string) {
		return function (val: any, element: HTMLInputElement, params: JqvcValidationParams): any {
			var value = resolveDataType(val);
			for (var i = 0, len = params.abortable.length; i < len; i++) {
				params.abortable[i].abort();
			}
			params.abortable = [];
			var substitutions: JqvcSubstitution[] = [];
			var result = resolveToken(params.rule, value, element, "", substitutions);
			if (!isDeferred(result))
				return isPresent(result);

			var validator = this;
			validator.startRequest(element);
			var whenFinished = function (valid) {
				if (valid) {
					var submitted = validator.formSubmitted;
					validator.prepareElement(element);
					validator.formSubmitted = submitted;
					validator.successList.push(element);
					delete validator.invalid[element.name];
					validator.showErrors();
				} else {
					var errors = {};
					var message = validator.defaultMessage(element, validatorName);
					errors[element.name] = message;
					validator.invalid[element.name] = true;
					validator.showErrors(errors);
				}
				validator.stopRequest(element, valid);
			};
			for (var i = 0, len = <number> result.calls.length; i < len; i++) {
				launchAsync(params.rule, result.calls[i], value, element, substitutions, params.abortable, whenFinished);
			}
			return "pending";
		};
	};

	var adapterMethod = function (validatorName: string) {
		return function (options) {
			if (options.message) {
				options.messages[validatorName] = options.message;
			}
			options.rules[validatorName] = <JqvcValidationParams> { rule: JSON.parse(options.params.rule), abortable: [] };
		}
	};

	var init = function () {
		for (var i = 0, len = validatorNamesToRegister.length; i < len; i++) {
			var validatorName = validatorNamesToRegister[i];
			$.validator.unobtrusive.adapters.add(validatorName, ["rule"], adapterMethod(validatorName));
			$.validator.addMethod(validatorName, validatorMethod(validatorName));
		}
	};

	init();
}(jQuery);