# SoftMills.ComplexValidation

*Overengineered with love by Anthony J. Mills of the Software Mills*

Once upon a time I worked at a job where we were doing some really complex validation logic on our ASP.NET MVC forms. I ended up making some ... interesting ... attributes: `RequiredIf`, `RequiredIfAbsent`, `RequiredIfAnyAbsent`, `RequiredIfAllPresentAbsent` ... and more. It was a mess. And each validation attribute needed its own error message, especially when it referenced other values in the model.

When I finished there, I thought back on my experience. One day I had an idea, a way to do validations that would allow validations of any complexity, running both on the client and the server. A way that would get the attribute to look up the names of referenced fields, allowing you to provide default error messages like `{0} must be between {1} and {2}.` -- with `{0}` and `{1}` and `{2}` being filled in with the names of the referenced fields. A way that would allow as many validation attributes to be put on a single property as you liked, even though all of them are processed through the same pipeline.

## Project Quality Summary

This code is of alpha quality, basically. I'm pretty confident about all of the functions except `remote` (which doesn't have a comprehensive test suite yet), and it should work great for text boxes, selects, and check boxes. I'm pretty sure it doesn't work well for radio buttons, and I don't have a good story currently for handling multiselects.

There is a NuGet package which lists the version as 0.1. It does so as a warning. The package is primarily a convenience for me, the author, because I'm using this in a couple commercial projects of mine.

## Introduction

So, here's the basic idea. Your current validation rules might look like this:

    [RegularExpression("[0-9]{5}")]
    public string ZipCode { get; set; }

But what if, instead, they looked like this?

    [ValidIf("['regex',['str','[0-9]{5}']]")]
    public string ZipCode { get; set; }

Big deal, you might say. That doesn't make it easier to read. It makes it harder! But it does allow introduction of variables...

    [ValidIf("['regex','ZipRegex']")]
    public string ZipCode { get; set; }

    // Put this into a hidden field for client validation
    public string ZipRegex { get { return Resources.ZipRegex; } }

And what if we make it a little ... fancier? How much code do you currently have to write to implement this?

    [ValidIf(@"['if',
	    ['eq','CountryId','UsaId'],['regex',['str','[0-9]{5}']],
        ['eq','CountryId','CanadaId'],['regex',['str','[A-Za-z][0-9][A-Za-z] ?[0-9][A-Za-z][0-9]']],
        true]",
        ErrorMessage = "{0} must be a valid zip code or postal code.")]

Or, perhaps, this:

    [ValidIf("['or',['not',['eq','CountryId','UsaId']],['regex',['str','[0-9]{5}']]]",
        ErrorMessage = "{0} must be a valid zip code.")]
    [ValidIf2("['or',['not',['eq','CountryId','CanadaId']],['regex',['str','[A-Za-z][0-9][A-Za-z] ?[0-9][A-Za-z][0-9]']]]",
	    ErrorMessage = "{0} must be a valid postal code.")]

(In other words, if the value of the `CountryId` field is equal to the value of the `UsaId` field (probably a hidden field), then the value is valid if it matches a US post office regular expression, but if the value of the `CountryId` field is equal to the value of the `CanadaId` field, then it's valid if it matches a Canadian post office regular expression. If the `CountryId` field is something else, it's valid, period.)

ComplexValidation implements a mini-language, basically. The grammar is JSON. The use of arrays is inspired by LISP. Arrays are function calls. The first array element is the name of the function, and the rest of the elements are parameters to the function. Strings are interpreted as form element names, and they evaluate to the given element's value (they're relative to the current form element, and you can use dots to access elements in submodels). If you need an actual string constant, the `str` function is special; it just passes its string parameter through verbatim.

So yeah. This is a really simple Lisp dedicated to doing validations. It's got lots of functions relating to validating, and you can even define your own pretty easily. Since rules are JSON strings, this allows really easy and fast parsing on the client side, meaning every single validation method is supported client-side as well!

## The meaning of "present"

A lot of functions in CV deal with the concept of "presence". For instance, When you have an expression like `[ValidIf("'Name'")]`, what does that mean? Well, basically a value is present if it is true (if it's a boolean) or has characters (a string). Numbers are always truthy, and dates are always truthy.

So `[ValidIf("'Name'")]` means that the current element is valid if the `Name` field is present. If `Name` is a text field, then the current element is valid if it isn't empty. If `Name` is a checkbox, then the current element is valid if `Name` is checked.

## Predefined Functions

CV defines a number of functions that should handle most of your validation needs. If there's something else, look in the Custom Functions section that follows this one.

### Asynchronous Calls

    ['remote',url,params...]

Does a remote call the the URL with the given parameters. On the server side, you register a handler to be called when a given URL is referenced.

### Tests for Presence

Since presence tests can involve Boolean values, it turns out that some presence tests are synonyms of well-known Boolean operators. Use whichever is the most intuitive. 

	['present']                  // returns whether the current element is present
	['present','Name']           // returns whether the Name element is present
    ['present','Name','Desc']    // returns whether both Name and Desc are present

Note that `all` and `and` are synonyms for `present`. Any number of parameters are allowed.

    ['any']                      // returns whether the current element is present
    ['any','Name']               // returns whether the Name element is present
	['any','Name','Desc']        // returns whether either Name or Desc are present

Note that `or` is a synonym for `any`.

    ['absent']                   // returns whether the current element is absent
    ['absent','Name']            // returns whether the Name element is absent
    ['absent','Name','Desc']     // returns whether both Name and Desc are absent

Note that `not` is a synonym for `absent`.

    ['anyabsent']                // returns whether the current element is absent
    ['anyabsent','Name']         // returns whether the Name element is absent
    ['anyabsent','Name','Desc']  // returns whether either Name or Desc are absent

Note that `nor` is a synonym for `anyabsent`.

    ['if',test1,value1,...,valueElse]

Think of `if` like a ternary operator in C#. Or maybe a case statement. If `test1` represents a present value, then `value1` is evaluated and returned. Otherwise, it tests the next case, and so on, until it runs out of cases. If that's the case, it evaluates `valueElse` and returns that.

    ['coalesce',value1,...]

Returns the first value that is present. If no value is present, it returns null.

    ['count',value1,...]

Returns the total number of values that are present. Say, for instance, if you were doing a search form and the spec said that at least three of the fields needed to be filled in.

### Comparisons

    ['eq',value]           // current == value
    ['eq',value1,value2]   // value1 == value2
    ['neq',value]          // current != value
    ['neq',value1,value2]  // value1 != value2
    ['lt',value]           // current < value
    ['lt',value1,value2]   // value1 < value2
    ['lte',value]          // current <= value
    ['lte',value1,value2]  // value1 <= value2
    ['gt',value]           // current > value
    ['gt',value1,value2]   // value1 > value2
    ['gte',value]          // current >= value
    ['gte',value1,value2]  // value1 >= value2

Comparisons are not all that interesting, really. Behavior is undefined if the values are different types.

### Strings

	['concat',value1,...]

Takes the given values, converts them to strings, and concatenates the result.

    ['len',value]

Converts the given value to a string and returns the resulting string length.

	['regex',pattern]       // returns whether the current element matches the regular expression
    ['regex',pattern,value] // returns whether the value matches the regular expression

The pattern has an implied `^` and `$` (it must match the whole pattern). On the server side, it uses `RegexOptions.ECMAScript` for consistency. For things like case-insensitivity, specify them in the regular expression.

### Utility

    ['str',value]

Returns the value straight (if a string) or converts it to a string (if, say, a number). If the value is a string, does not try to look up the value.

    ''               // Resolves to the value of the current field
	'Field'          // Resolves to the value of the field named "Field"
    ['value',value]  // Evaluates the given value, converts it to a string, and looks it up

Calling `value` is useful if you don't have a string constant representing the name you want to look up. For example, if you're concatenating strings and you want to look up the resulting name.

	['with',prefix,value]

Evaluates the given value, but using the prefix prefixed to any element names. Basically works like `with` in other computer languages.

## Custom Functions

It's pretty easy to add additional functions. On the server side, you can subclass `ValidIfAttribute` and override `ResolveFunction()`, providing your custom function resolution before deferring to the superclass.

    public class ReallyValidIfAttribute : ValidIfAttribute {
        protected override object ResolveFunction(JArray args, object value, object container) {
            switch (args[0].ToString()) {
                case "odd": {
                    dynamic val = ResolveToken(args[1], value, container);
                    return val & 1 == 1;
                }
            }
            return base.ResolveFunction(args, value, container);
        }
    }

On the client side, you can grab the JavaScript and add your function to `resolveArray()`. If you don't use asynchronous logic, this is pretty easy:

    case "odd":
        return !!(resolveToken(array[i], value, element, prefix, substitutions) & 1);

## Data Types

On the server side, CV uses `dynamic` to test inequalities and equivalence, so as long as your model is reasonable, things should work out, whether you're testing dates, numbers, whatever.

On the client side, though, Dates need a little special consideration. The JavaScript portion tries to resolve a text field as a boolean, then a number, then a date, and then finally leaves it as a string. The date part is basically doing `new Date(value)` and seeing if the result is `NaN` or not.

If the format you are using for dates does not work with this method (say, for example, you're using `dd-mm-yyyy`), you can edit the JavaScript to change how it gets a date from a string.

## Using multiple validations on a single property

ASP.NET MVC has a limit of one validation attribute of a single type per property. To get around this, `ValidIf` is designed to be easily subclassed, either simply:

    [ValidIf("['gte','MinValue']", ErrorMessage = "{0} must be at least the minimum.")]
    [ValidIf2("['lte','MaxValue']", ErrorMessage = "{0} must be at most the maximum.")]
    public int ChosenValue { get; set; }

Or more complicatedly:

	[GreaterThanOrEqualTo("MinValue")]
    [LessThanOrEqualTo("MaxValue")]
    public int ChosenValue { get; set; }

Basically, `ValidIf2Attribute` is a simple descendent of `ValidIfAttribute`, and `GreaterThanOrEqualToAttribute` and `LessThanOrEqualToAttribute` are only slightly more complicated descendents.

Now, if you're paying attention, you're wondering: if you can create any descendant, does the client-side validation still work? Yes! `ValidIfAttribute` keeps track itself. The first `ValidIfAttribute` (or descendent) will get a `v` client-side identifier, the second will get a `va`, the third gets `vb`, the fourth gets `vc`, etc.

As shipped, you can use `[ValidIf()]` through `[ValidIf5()]`. If you need more than five validations on any particular property, just define another descendant and have the client-side validation register the next few client-side identifiers (it's easy, just add them to an array at the top of the JavaScript file).

## Predefined Validations

Some validation attributes have been defined to handle the most common cases.

    [RequiredIf("Field")]
    [RequiredIfAbsent("Field")]
	[EqualTo("Field")]
    [LessThan("Field")]
    [LessThanOrEqualTo("Field")]
    [GreaterThan("Field")]
    [GreaterThanOrEqualTo("Field")]

You can make your own attributes easily, similar to these. That's the subject of the next section.

## Reusable Validations

Using `ValidIf[]` can be a bit hard to read. Using the predefined validations above is a bit easier. Is there a way to make one's own packaged validations? Absolutely.

For instance, let's look at `LessThanOrEqualToAttribute` from the C# source code.

    public class LessThanOrEqualToAttribute : ValidIfAttribute {
        public LessThanOrEqualToAttribute(string other)
            : base("['lte','" + HttpUtility.JavaScriptStringEncode(other) + "']", other) {}
    }

Yes, that's pretty much all there is to it. Note, by the way, how `other` is passed as a parameter to the base constructor. This means when we define error messages, we can reference the name of the other field in the error message. For instance, `{0} must be less than or equal to {1}.`.

Now if we want to define default error messages for these packaged validations, that is the subject of the next section.

## Default Error Messages

Having to specify a separate error message for each validation is a pain. Often, you can come up with a default error message with some placeholders, and that can service most of the places that you use

In order to register a default error message for each of the reusable validations you've dreamed up above, you need to register them with the `DataAnnotationsModelValidatorProvider`. Fortunately, this can be done fairly painlessly in your `Global.asax.cs` file.

    namespace Your.Application.Namespace {
        protected void Application_Start() {
            // initialize other things

            RegisterClientValidatableAttribute<RequiredIfAttribute>();
            RegisterClientValidatableAttribute<RequiredIfAbsentAttribute>();
            // ...
        }

        private static void RegisterClientValidatableAttribute<T>()
            where T : ValidationAttribute
        {
            DataAnnotationsModelValidatorProvider.RegisterAdapter(
                typeof(T),
                typeof(ClientValidatableAttributeAdapter<T>));
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
    }

Now, if you define Validation_RequiredIf as a string in your resource file, that will be used as the default error message string for `RequiredIfAttribute`.

## Limitations

Since CV uses the `dynamic` keyword for server comparisons, it requires at least .NET 4.0. A truly clever programmer could probably fix this.

Originally CV used Microsoft's built-in JSON services. But since Json.Net is faster and officially included anyway now, I've rewritten CV to use Json.Net.

While CV allows you to do complex asynchronous validations like `['remote',['remote','UrlUrl'],['remote','ParamUrl']]` ... the underlying jQuery Validate library that powers ASP.NET MVC validations was not built for this. This may be rectified in the future, but for now, just don't do that. One remote call per property is probably fine.

I should probably reiterate that if `new Date(value)` does not properly parse the date format you are using for your text fields, you should probably change that part of `resolveDataType()`.

## License

This code is placed under the MIT license. If you use it in your application, please email me a thank-you note.

## Acknowledgments

Many thanks to Nick Riggs for Foolproof Validation, which was the inspiration for some of the code in here. If Foolproof had allowed me to put a `GreaterThan` and a `LessThan` on the same element (not possible because Foolproof uses the same 'is' client-side validation identifier for all its attributes), I might never have written this.

Many thanks also to the TypeScript team, and Anders Hejlsberg in particular. The JavaScript portion of this was written in TypeScript, and Anders has produced many projects that I have loved, from Delphi to C# to TypeScript. Many thanks!