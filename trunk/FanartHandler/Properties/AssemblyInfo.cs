using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System;
using MediaPortal.Common.Utils;

[assembly: CLSCompliant(false)]


[assembly: SecurityPermission(SecurityAction.RequestMinimum, UnmanagedCode = false)]


// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("FanartHandler")]
[assembly: AssemblyDescription("Fanart Handler for MediaPortal")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("FanartHandler")]
[assembly: AssemblyCopyright("Open Source software licensed under the GNU/GPL agreement.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: CompatibleVersion("1.1.6.27644")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("65149fb3-6ff6-45cc-9ffd-098fdcc61ee7")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion(FanartHandler.Utils.GetMajorMinorVersionNumber )]
//[assembly: AssemblyFileVersion("1.3.0.0")]
