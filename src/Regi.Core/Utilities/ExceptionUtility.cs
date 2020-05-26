using Regi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Regi.Utilities
{
    public static class ExceptionUtility
    {
        private static string _projectRoleValues => string.Join(", ", typeof(ProjectRole).GetEnumNames().Where(n => n != nameof(ProjectRole.Unknown)));

        public static string GetMessage(Exception e)
        {
            if (e is System.Text.Json.JsonException jsonException)
            {
                if (Regex.IsMatch(jsonException.Path, @"projects\[\d\]\.roles\[\d\]"))
                {
                    return $"Received an invalid value for \"roles\"\u2014must be one of {_projectRoleValues}. Line: {jsonException.LineNumber} Position: {jsonException.BytePositionInLine}";
                }
            }

            return e.Message;
        }
    }
}
