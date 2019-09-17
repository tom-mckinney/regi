// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

// Global
[assembly: SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Objects are only used for tests and mocking")]
[assembly: SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "Useful for test names")]
[assembly: SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "Configure await is ugly...")]
[assembly: SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Used for mocking exceptions")]
[assembly: SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations", Justification = "Used for making test stubs")]

// Specific instances
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Swallow all exceptions", Scope = "member", Target = "~M:Regi.Test.Helpers.XunitTextWriter.Write(System.Char)")]
