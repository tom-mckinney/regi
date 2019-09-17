// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

// Global
[assembly: SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "ConfigureAwait is ugly... Also this library will not be exposed as an API for consumption")]
[assembly: SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Not a priority to abstract exception messages yet")]
[assembly: SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Validated at higher scopes")]
[assembly: SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Needed for object deserialization")]
[assembly: SuppressMessage("Design", "CA1051:Do not declare visible instance fields")]
[assembly: SuppressMessage("Globalization", "CA1305:Specify IFormatProvider")]
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types")]
[assembly: SuppressMessage("Design", "CA1034:Nested types should not be visible")]

// Specific instances
[assembly: SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Scope = "type", Target = "~T:Regi.Services.QueueService")]
[assembly: SuppressMessage("Design", "CA1052:Static holder types should be Static or NotInheritable", Justification = "Program is used as a generic", Scope = "type", Target = "~T:Regi.Program")]
[assembly: SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "commands will generally be written in lowercase format", Scope = "member", Target = "~M:Regi.Models.CommandDictionary`1.TryGetValue(Regi.Models.AppTask,System.Collections.Generic.IList{`0}@)~System.Boolean")]
