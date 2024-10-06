using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace coding_challenge_fs_updated.Infrastracture.Services
{
    public class TypeScriptConverterService
    {
        private readonly ClassStringToActualClassService _classStringToActualClassService;

        public TypeScriptConverterService()
        {
            _classStringToActualClassService = new();
        }

        public string ConvertToTypescript(string csharpClassDefinition)
        {
            // Clean and prepare class and property regexes
            csharpClassDefinition = csharpClassDefinition.Trim();
            var classRegex = new Regex(@"public class (\w+)");
            var propertyRegex = new Regex(@"public (.+?) (\w+) \{ get; set; \}");
            var listRegex = new Regex(@"List<(\w+)>");

            var sb = new StringBuilder();

            // Match classes
            foreach (Match classMatch in classRegex.Matches(csharpClassDefinition))
            {
                string className = classMatch.Groups[1].Value;
                sb.AppendLine($"export interface {className} {{");

                // Capture properties inside the class
                string classBody = ExtractClassBody(csharpClassDefinition, classMatch.Value);
                var compiledClass = _classStringToActualClassService.CompileClassFromString(classBody, className);
                foreach (Match propMatch in propertyRegex.Matches(classBody))
                {
                    var isListType = listRegex.IsMatch(propMatch.Groups[1].Value);
                    if (!_classStringToActualClassService.HasProperty(compiledClass, propMatch.Groups[2].Value) && !isListType)
                    { 
                        continue;
                    }

                    string tsType = GetTypescriptType(propMatch.Groups[1].Value, listRegex);
                    string propName = ToCamelCase(propMatch.Groups[2].Value);
                    sb.AppendLine($"    {propName}: {tsType};");
                }

                sb.AppendLine("}");
            }

            return sb.ToString();
        }

        private string ExtractClassBody(string csharpClassDefinition, string classMatchValue)
        {
            int classStartIndex = csharpClassDefinition.IndexOf(classMatchValue);
            int classEndIndex = csharpClassDefinition.LastIndexOf('}') + 1;
            return csharpClassDefinition.Substring(classStartIndex, classEndIndex - classStartIndex);
        }

        private string GetTypescriptType(string csharpType, Regex listRegex)
        {
            if (csharpType.Contains("?")) return CSharpToTypescriptType(csharpType.Replace("?", "")) + "?";
            if (listRegex.IsMatch(csharpType)) return CSharpToTypescriptType(listRegex.Match(csharpType).Groups[1].Value) + "[]";
            return CSharpToTypescriptType(csharpType);
        }

        private string CSharpToTypescriptType(string csharpType) => csharpType switch
        {
            "string" => "string",
            "int" or "long" => "number",
            _ => csharpType
        };

        private string ToCamelCase(string input) => char.ToLower(input[0]) + input[1..];

    }
}
