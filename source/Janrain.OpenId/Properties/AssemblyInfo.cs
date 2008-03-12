// Uncomment this line to build a partially trusted assembly.
// This has some security bonuses in that if there was a way to
// hijack this assembly to do something it is not designed to do,
// it will fail before doing much damage.
// But a partially trusted assembly's events, handled by the hosting
// web site, will also be under the partial trust restriction.
//#define PARTIAL_TRUST

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Web;
using System.Net;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Janrain.OpenId")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("Janrain.OpenId")]
[assembly: AssemblyCopyright("Copyright ©  2007")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("7d73990c-47c0-4256-9f20-a893add9e289")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:
[assembly: AssemblyVersion("0.1.*")]
[assembly: AssemblyFileVersion("0.1.*")]

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
// Allows hosting this assembly in an ASP.NET setting.  Not all applications
// will host this using ASP.NET, so this is optional.  Besides, we need at least
// one optional permission to activate CAS permission shrinking.
#if PARTIAL_TRUST
[assembly: AspNetHostingPermission(SecurityAction.RequestOptional, Level = AspNetHostingPermissionLevel.Medium)]
#endif

// The following are only required for diagnostic logging (Trace.Write, Debug.Assert, etc.).
#if TRACE
[assembly: KeyContainerPermission(SecurityAction.RequestMinimum, Unrestricted = true)]
[assembly: ReflectionPermission(SecurityAction.RequestMinimum, MemberAccess = true)]
[assembly: RegistryPermission(SecurityAction.RequestMinimum, Unrestricted = true)]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, ControlEvidence = true, UnmanagedCode = true, ControlThread = true)]
[assembly: FileIOPermission(SecurityAction.RequestMinimum, AllFiles = FileIOPermissionAccess.PathDiscovery | FileIOPermissionAccess.Read)]
#else
#if PARTIAL_TRUST
[assembly: KeyContainerPermission(SecurityAction.RequestOptional, Unrestricted = true)]
[assembly: ReflectionPermission(SecurityAction.RequestOptional, MemberAccess = true)]
[assembly: RegistryPermission(SecurityAction.RequestOptional, Unrestricted = true)]
[assembly: SecurityPermission(SecurityAction.RequestOptional, ControlEvidence = true, UnmanagedCode = true, ControlThread = true)]
[assembly: FileIOPermission(SecurityAction.RequestOptional, AllFiles = FileIOPermissionAccess.PathDiscovery | FileIOPermissionAccess.Read)]
#endif
#endif
