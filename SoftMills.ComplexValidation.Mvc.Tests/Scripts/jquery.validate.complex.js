/*! SoftMills.ComplexValidation v0.1.1 | (c) 2013 Anthony Mills | MIT license */





!function ($) {
    // Add subsequent elements in the chain if you need more validators
    var validatorNamesToRegister = ["v", "va", "vb", "vc", "vd"];

    var pending = { isDeferred: true, calls: [] };

    var getDeferred = function () {
        var args = [];
        for (var _i = 0; _i < (arguments.length - 0); _i++) {
            args[_i] = arguments[_i + 0];
        }
        var async = null;
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

    var resolveArray = function (array, value, element, prefix, substitutions) {
        for (var i = 0, len = substitutions.length; i < len; i++)
            if (array === substitutions[i].insteadOf)
                return substitutions[i].use;

        switch (array[0]) {
            case "remote":
            case "delay":
                var syncCall = [array], asyncCalls = null;
                for (var i = 1, len = array.length; i < len; i++) {
                    var val = resolveToken(array[i], value, element, prefix, substitutions);
                    if (isDeferred(val)) {
                        asyncCalls = (asyncCalls || []).concat(val.calls);
                    } else {
                        syncCall.push(val);
                    }
                }
                return { isDeferred: true, calls: async || [syncCall] };

            case "all":
            case "and":
            case "present":
                if (array.length < 2)
                    return isPresent(value);
                var async = null;
                for (var i = 1, len = array.length; i < len; i++) {
                    var val = resolveToken(array[i], value, element, prefix, substitutions);
                    async = getDeferred(async, val);
                    if (!isDeferred(val) && !isPresent(val))
                        return false;
                }
                return async || true;
            case "any":
            case "or":
                if (array.length < 2)
                    return isPresent(value);
                var async = null;
                for (var i = 1, len = array.length; i < len; i++) {
                    var val = resolveToken(array[i], value, element, prefix, substitutions);
                    async = getDeferred(async, val);
                    if (!isDeferred(val) && isPresent(val))
                        return true;
                }
                return async || false;
            case "absent":
            case "not":
                if (array.length < 2)
                    return !isPresent(value);
                var async = null;
                for (var i = 1, len = array.length; i < len; i++) {
                    var val = resolveToken(array[i], value, element, prefix, substitutions);
                    async = getDeferred(async, val);
                    if (!isDeferred(val) && !isPresent(val))
                        return true;
                }
                return async || false;
            case "anyabsent":
            case "nor":
                if (array.length < 2)
                    return !isPresent(value);
                var async = null;
                for (var i = 1, len = array.length; i < len; i++) {
                    var val = resolveToken(array[i], value, element, prefix, substitutions);
                    async = getDeferred(async, val);
                    if (!isDeferred(val) && isPresent(val))
                        return false;
                }
                return async || true;

            case "if":
                var async = null;
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
                var async = null;
                for (var i = 1, len = array.length; i < len; i++) {
                    var val = resolveToken(array[i], value, element, prefix, substitutions);
                    async = getDeferred(async, val);
                    if (!isDeferred(val) && isPresent(val))
                        return async || value;
                }
                return async || null;
            case "count":
                var async = null;
                for (var i = 1, counter = 0, len = array.length; i < len; i++) {
                    var token = resolveToken(array[i], value, element, prefix, substitutions);
                    async = getDeferred(async, token);
                    if (!isDeferred(token) && isPresent(token))
                        counter++;
                }
                return async || counter;
            case "contains":
                var test = resolveToken(array[array.length < 3 ? 1 : 2], value, element, prefix, substitutions);
                var haystack = array.length < 3 ? value : resolveToken(array[1], value, element, prefix, substitutions);
                return haystack.some(function (t) {
                    return t == test;
                });
            case "in":
                var test = array.length < 3 ? value : resolveToken(array[1], value, element, prefix, substitutions);
                var haystack = array.slice(array.length < 3 ? 1 : 2).map(function (t) {
                    return resolveToken(t, value, element, prefix, substitutions);
                });
                return haystack.some(function (t) {
                    return t == test;
                });
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
                var str = "", async = null;
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

            case "eq":
                var first = array.length < 3 ? value : resolveToken(array[1], value, element, prefix, substitutions);
                if (first == null)
                    return true;
                var second = resolveToken(array[array.length < 3 ? 1 : 2], value, element, prefix, substitutions);
                if (second == null)
                    return true;
                return getDeferred(first, second) || first === second;
            case "neq":
                first = array.length < 3 ? value : resolveToken(array[1], value, element, prefix, substitutions);
                if (first == null)
                    return true;
                second = resolveToken(array[array.length < 3 ? 1 : 2], value, element, prefix, substitutions);
                if (second == null)
                    return true;
                return getDeferred(first, second) || first !== second;
            case "lt":
                first = array.length < 3 ? value : resolveToken(array[1], value, element, prefix, substitutions);
                if (first == null)
                    return true;
                second = resolveToken(array[array.length < 3 ? 1 : 2], value, element, prefix, substitutions);
                if (second == null)
                    return true;
                return getDeferred(first, second) || first < second;
            case "lte":
                first = array.length < 3 ? value : resolveToken(array[1], value, element, prefix, substitutions);
                if (first == null)
                    return true;
                second = resolveToken(array[array.length < 3 ? 1 : 2], value, element, prefix, substitutions);
                if (second == null)
                    return true;
                return getDeferred(first, second) || first <= second;
            case "gt":
                first = array.length < 3 ? value : resolveToken(array[1], value, element, prefix, substitutions);
                if (first == null)
                    return true;
                second = resolveToken(array[array.length < 3 ? 1 : 2], value, element, prefix, substitutions);
                if (second == null)
                    return true;
                return getDeferred(first, second) || first > second;
            case "gte":
                first = array.length < 3 ? value : resolveToken(array[1], value, element, prefix, substitutions);
                if (first == null)
                    return true;
                second = resolveToken(array[array.length < 3 ? 1 : 2], value, element, prefix, substitutions);
                if (second == null)
                    return true;
                return getDeferred(first, second) || first >= second;

            default:
                throw new Error("Unknown function '" + array[0] + "' in function call '" + JSON.stringify(array) + "'.");
        }
    };

    var resolveDataType = function (value) {
        if (typeof value !== "string") {
            return $.isArray(value) ? value.map(function (v) {
                return resolveDataType(v);
            }) : value;
        } else if (value === "true" || value === "True") {
            return true;
        } else if (value === "false" || value === "False") {
            return false;
        } else if ((value - 0) == value && value.length > 0) {
            return parseFloat(value);
        } else {
            var dateValue = new Date(value);
            if (!isNaN(dateValue))
                return dateValue;
        }
        return value;
    };

    var resolveString = function (str, value, element, prefix) {
        if (str === "")
            return value;
        var i, len;
        var pos = element.name.lastIndexOf(".") + 1;
        var name = element.name.substr(0, pos) + prefix + str;
        var elements = document.getElementsByName(name);
        if (elements.length === 0)
            throw new Error("Cannot resolve referenced element named '" + name + "'.");
        for (i = 0, len = elements.length; i < len; i++) {
            var el = elements[i];
            if (el.type === "checkbox")
                return el.checked;
            else if (el.type === "radio") {
                if (el.checked)
                    return resolveDataType(el.value);
            } else
                return resolveDataType($(el).val());
        }
        return null;
    };

    var resolveToken = function (token, value, element, prefix, substitutions) {
        if (token instanceof Array)
            return resolveArray(token, value, element, prefix, substitutions);
        if (typeof (token) === "string")
            return resolveString(token, value, element, prefix);
        return token;
    };

    var isPresent = function (value) {
        return $.isArray(value) ? !!value.length : !!value ? value !== "false" && value !== "False" : value === 0;
    };

    var makeString = function (obj) {
        return obj == null ? "" : "" + obj;
    };

    var isDeferred = function (value) {
        return value && value.isDeferred;
    };

    var resolveArrayAsync = function (array, value, callback) {
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

    var launchAsync = function (rule, call, value, element, substitutions, abortable, callback) {
        var substitution = { insteadOf: call[0], use: pending };
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

    var validatorMethod = function (validatorName) {
        return function (val, element, params) {
            var value = resolveDataType(val);
            for (var i = 0, len = params.abortable.length; i < len; i++) {
                params.abortable[i].abort();
            }
            params.abortable = [];
            var substitutions = [];
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
            for (var i = 0, len = result.calls.length; i < len; i++) {
                launchAsync(params.rule, result.calls[i], value, element, substitutions, params.abortable, whenFinished);
            }
            return "pending";
        };
    };

    var adapterMethod = function (validatorName) {
        return function (options) {
            if (options.message) {
                options.messages[validatorName] = options.message;
            }
            options.rules[validatorName] = { rule: JSON.parse(options.params.rule), abortable: [] };
        };
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
//# sourceMappingURL=jquery.validate.complex.js.map
