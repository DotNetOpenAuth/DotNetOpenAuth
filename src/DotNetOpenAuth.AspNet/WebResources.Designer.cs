﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.488
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DotNetOpenAuth.AspNet {
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
    internal class WebResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal WebResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("DotNetOpenAuth.AspNet.WebResources", typeof(WebResources).Assembly);
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
        ///   Looks up a localized string similar to A setting in web.config requires a secure connection for this request but the current connection is not secured..
        /// </summary>
        internal static string ConnectionNotSecure {
            get {
                return ResourceManager.GetString("ConnectionNotSecure", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to encrypt the authentication ticket..
        /// </summary>
        internal static string FailedToEncryptTicket {
            get {
                return ResourceManager.GetString("FailedToEncryptTicket", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The provided data could not be decrypted. If the current application is deployed in a web farm configuration, ensure that the &apos;decryptionKey&apos; and &apos;validationKey&apos; attributes are explicitly specified in the &lt;machineKey&gt; configuration section..
        /// </summary>
        internal static string Generic_CryptoFailure {
            get {
                return ResourceManager.GetString("Generic_CryptoFailure", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An OAuth data provider has already been registered for this application..
        /// </summary>
        internal static string OAuthDataProviderRegistered {
            get {
                return ResourceManager.GetString("OAuthDataProviderRegistered", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This operation is not supported on the current provider..
        /// </summary>
        internal static string OAuthRequireReturnUrl {
            get {
                return ResourceManager.GetString("OAuthRequireReturnUrl", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to obtain the authentication response from service provider..
        /// </summary>
        internal static string OpenIDFailedToGetResponse {
            get {
                return ResourceManager.GetString("OpenIDFailedToGetResponse", resourceCulture);
            }
        }
    }
}
