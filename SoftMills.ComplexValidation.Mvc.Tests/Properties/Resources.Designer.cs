﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34003
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SoftMills.ComplexValidation.Mvc.Tests.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("SoftMills.ComplexValidation.Mvc.Tests.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} must be equal to {1}..
        /// </summary>
        internal static string Validation_EqualTo {
            get {
                return ResourceManager.GetString("Validation_EqualTo", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} must be greater than {1}..
        /// </summary>
        internal static string Validation_GreaterThan {
            get {
                return ResourceManager.GetString("Validation_GreaterThan", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} must be greater than or equal to {1}..
        /// </summary>
        internal static string Validation_GreaterThanOrEqualTo {
            get {
                return ResourceManager.GetString("Validation_GreaterThanOrEqualTo", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} must be less than {1}..
        /// </summary>
        internal static string Validation_LessThan {
            get {
                return ResourceManager.GetString("Validation_LessThan", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} must be less than or equal to {1}..
        /// </summary>
        internal static string Validation_LessThanOrEqualTo {
            get {
                return ResourceManager.GetString("Validation_LessThanOrEqualTo", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} is required..
        /// </summary>
        internal static string Validation_Required {
            get {
                return ResourceManager.GetString("Validation_Required", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} is required if {1} is entered..
        /// </summary>
        internal static string Validation_RequiredIf {
            get {
                return ResourceManager.GetString("Validation_RequiredIf", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} is required if {1} is left blank..
        /// </summary>
        internal static string Validation_RequiredIfAbsent {
            get {
                return ResourceManager.GetString("Validation_RequiredIfAbsent", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} is too long. It can have at most {1} characters in it..
        /// </summary>
        internal static string Validation_StringLength {
            get {
                return ResourceManager.GetString("Validation_StringLength", resourceCulture);
            }
        }
    }
}