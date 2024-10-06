using System;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;
using System.Collections.Generic;
using System.Collections;


namespace coding_challenge_fs_updated.Infrastracture.Services
{
    internal class ClassStringToActualClassService
    {

        internal Type? CompileClassFromString(string classCode, string className)
        {
            // Create a syntax tree from the class code
            var syntaxTree = CSharpSyntaxTree.ParseText(FormatProperToStringClass(classCode));

            var coreLibAssembly = Assembly.Load("System.Private.CoreLib").Location;
            var systemRuntimeAssembly = Assembly.Load("System.Runtime").Location;
            var systemCollection = Assembly.Load("System.Collections").Location;

            // Compile the code into an in-memory assembly
            var compilation = CSharpCompilation.Create("DynamicAssembly")
                .AddReferences(
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),          // System.Object
                    MetadataReference.CreateFromFile(coreLibAssembly),                           // System.Private.CoreLib
                    MetadataReference.CreateFromFile(systemRuntimeAssembly),                     // System.Runtime
                    MetadataReference.CreateFromFile(typeof(List<>).Assembly.Location),          // System.Collections.Generic (List<>)
                    MetadataReference.CreateFromFile(systemCollection)
                )
                .AddSyntaxTrees(syntaxTree)
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));


            // Emit the compiled assembly to memory
            using (var ms = new System.IO.MemoryStream())
            {
                var result = compilation.Emit(ms);

                // If compilation failed, capture diagnostics
                if (!result.Success)
                {
                    var errors = result.Diagnostics
                        .Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error)
                        .Select(diagnostic => diagnostic.ToString());

                    Console.WriteLine("Compilation failed with errors:");
                    foreach (var error in errors)
                    {
                        Console.WriteLine(error);
                    }

                    return null;  // Return null if there are compilation errors
                }


                // Compilation was successful
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                var assembly = Assembly.Load(ms.ToArray());

                // Get the compiled type
                var type = assembly.GetType(className);

                return type;
            }
        }

        internal bool HasProperty(Type type, string propertyName)
        {
            return type?.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance) != null;
        }

        private string FormatProperToStringClass(string input)
        {
            int openBrackets = 0;
            int closeBrackets = 0;

            // Count the number of opening and closing brackets
            foreach (char c in input)
            {
                if (c == '{') openBrackets++;
                if (c == '}') closeBrackets++;
            }

            // Check if there is an excess closing bracket
            if (closeBrackets > openBrackets)
            {
                // Remove the last closing bracket if it's excess
                int lastIndex = input.LastIndexOf('}');
                if (lastIndex != -1)
                {
                    input = input.Remove(lastIndex, 1);
                }
            }

            return input;
        }
    }
}
