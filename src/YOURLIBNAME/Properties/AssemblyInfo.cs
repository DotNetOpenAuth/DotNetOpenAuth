// Uncomment this line to build a partially trusted assembly.
// This has some security bonuses in that if there was a way to
// hijack this assembly to do something it is not designed to do,
// it will fail before doing much damage.
// But a partially trusted assembly's events, handled by the hosting
// web site, will also be under the partial trust restriction.
// Also note that http://support.microsoft.com/kb/839300 states a 
// strong-name signed assembly must use AllowPartiallyTrustedCallers 
// to be called from a web page, but defining PARTIAL_TRUST below also
// accomplishes this.
//#define PARTIAL_TRUST

// We DON'T put an AssemblyVersionAttribute in here because it is generated in the build.

using System;
using System.Net;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Web.UI;

[assembly: TagPrefix("YOURLIBNAME", "oauth")]

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("YOURLIBNAME")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("YOURLIBNAME")]
[assembly: AssemblyCopyright("Copyright ©  2008")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: NeutralResourcesLanguage("en-US")]
[assembly: CLSCompliant(true)]
// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("7d73990c-47c0-4256-9f20-a893add9e289")]

#if StrongNameSigned
// See comment at top of this file.  We need this so that strong-naming doesn't
// keep this assembly from being useful to shared host (medium trust) web sites.
[assembly: AllowPartiallyTrustedCallers]

[assembly: InternalsVisibleTo("YOURLIBNAME.Test, PublicKey=0024000004800000940000000602000000240000525341310004000001000100AD093C3765257C89A7010E853F2C7C741FF92FA8ACE06D7B8254702CAD5CF99104447F63AB05F8BB6F51CE0D81C8C93D2FCE8C20AAFF7042E721CBA16EAAE98778611DED11C0ABC8900DC5667F99B50A9DADEC24DBD8F2C91E3E8AD300EF64F1B4B9536CEB16FB440AF939F57624A9B486F867807C649AE4830EAB88C6C03998")]
#else
[assembly: InternalsVisibleTo("YOURLIBNAME.Test")]
#endif

// Specify what permissions are required and optional for the assembly.
// In order for CAS to remove unnecessary privileges from this assembly (which is desirable
// for security), we need at least one RequestMinimum and at least one RequestOptional.
// These permissions were determined using PermCalc.exe

// We need to be allowed to execute code.  Besides, it gives a good baseline RequestMinimum permission.
[assembly: SecurityPermission(SecurityAction.RequestMinimum, Execution = true)]
// Allows the consumer to call out to the web server.  This is unnecessary in provider-only scenarios.
// Note: we don't use a single demand for https?://.* because the regex pattern must exactly
// match the one used by hosting providers.  Listing them individually seems to be more common.
[assembly: WebPermission(SecurityAction.RequestMinimum, ConnectPattern = @"http://.*")]
[assembly: WebPermission(SecurityAction.RequestMinimum, ConnectPattern = @"https://.*")]

#if PARTIAL_TRUST
// Allows hosting this assembly in an ASP.NET setting.  Not all applications
// will host this using ASP.NET, so this is optional.  Besides, we need at least
// one optional permission to activate CAS permission shrinking.
[assembly: AspNetHostingPermission(SecurityAction.RequestOptional, Level = AspNetHostingPermissionLevel.Medium)]

// The following are only required for diagnostic logging (Trace.Write, Debug.Assert, etc.).
#if TRACE || DEBUG
[assembly: KeyContainerPermission(SecurityAction.RequestOptional, Unrestricted = true)]
[assembly: ReflectionPermission(SecurityAction.RequestOptional, MemberAccess = true)]
[assembly: RegistryPermission(SecurityAction.RequestOptional, Unrestricted = true)]
[assembly: SecurityPermission(SecurityAction.RequestOptional, ControlEvidence = true, UnmanagedCode = true, ControlThread = true)]
[assembly: FileIOPermission(SecurityAction.RequestOptional, AllFiles = FileIOPermissionAccess.PathDiscovery | FileIOPermissionAccess.Read)]
#endif
#endif
