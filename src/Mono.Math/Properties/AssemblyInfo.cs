﻿//-----------------------------------------------------------------------
// <copyright file="AssemblyInfo.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

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

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyCopyright("Copyright © 2012 Outercurve Foundation")]
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

[assembly: InternalsVisibleTo("Org.Mentalis.Security.Cryptography, PublicKey=0024000004800000940000000602000000240000525341310004000001000100AD093C3765257C89A7010E853F2C7C741FF92FA8ACE06D7B8254702CAD5CF99104447F63AB05F8BB6F51CE0D81C8C93D2FCE8C20AAFF7042E721CBA16EAAE98778611DED11C0ABC8900DC5667F99B50A9DADEC24DBD8F2C91E3E8AD300EF64F1B4B9536CEB16FB440AF939F57624A9B486F867807C649AE4830EAB88C6C03998")]
#else
[assembly: InternalsVisibleTo("Org.Mentalis.Security.Cryptography")]
#endif
