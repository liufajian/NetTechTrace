using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using System.Reflection;
using System.Runtime.Loader;

namespace NetCoreTest.RoslynTests
{
    [TestClass]
    public class RoslynTest1
    {
        [TestMethod]
        public void Test1(string[] args)
        {
            // We will change the namespace of this sample code.
            var code =
            @"  using System; 

                namespace OldNamespace 
                { 
                    public class Person
                    {
                        public string Name { get; set; }
                        public int Age {get; set; }
                    }
                }";

            // Use Task to call ChangeNamespaceAsync
            Task.Run(async () =>
            {
                await ChangeNamespaceAsync(code, "NamespaceChangedUsingRoslyn");
            })
            .GetAwaiter()
            .GetResult();

            // Wait to exit.
            Console.Read();
        }

        /// <summary>
        /// Changes the namespace for the given code.
        /// </summary>
        static async Task ChangeNamespaceAsync(string code, string @namespace)
        {
            // Parse the code into a SyntaxTree.
            var tree = CSharpSyntaxTree.ParseText(code);

            // Get the root CompilationUnitSyntax.
            var root = await tree.GetRootAsync().ConfigureAwait(false) as CompilationUnitSyntax;

            // Get the namespace declaration.
            var oldNamespace = root.Members.Single(m => m is NamespaceDeclarationSyntax) as NamespaceDeclarationSyntax;

            // Get all class declarations inside the namespace.
            var classDeclarations = oldNamespace.Members.Where(m => m is ClassDeclarationSyntax);

            // Create a new namespace declaration.
            var newNamespace = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(@namespace)).NormalizeWhitespace();

            // Add the class declarations to the new namespace.
            newNamespace = newNamespace.AddMembers(classDeclarations.Cast<MemberDeclarationSyntax>().ToArray());

            // Replace the oldNamespace with the newNamespace and normailize.
            root = root.ReplaceNode(oldNamespace, newNamespace).NormalizeWhitespace();

            string newCode = root.ToFullString();

            // Write the new file.
            File.WriteAllText("Person.cs", root.ToFullString());

            // Output new code to the console.
            Console.WriteLine(newCode);
            Console.WriteLine("Namespace replaced...");
        }

        /// <summary>
        /// Create a class from scratch.
        /// </summary>
        static void CreateClass()
        {
            // Create a namespace: (namespace CodeGenerationSample)
            var @namespace = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("CodeGenerationSample")).NormalizeWhitespace();

            // Add System using statement: (using System)
            @namespace = @namespace.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")));

            //  Create a class: (class Order)
            var classDeclaration = SyntaxFactory.ClassDeclaration("Order");

            // Add the public modifier: (public class Order)
            classDeclaration = classDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            // Inherit BaseEntity<T> and implement IHaveIdentity: (public class Order : BaseEntity<T>, IHaveIdentity)
            classDeclaration = classDeclaration.AddBaseListTypes(
                SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("BaseEntity<Order>")),
                SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("IHaveIdentity")));

            // Create a string variable: (bool canceled;)
            var variableDeclaration = SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName("bool"))
                .AddVariables(SyntaxFactory.VariableDeclarator("canceled"));

            // Create a field declaration: (private bool canceled;)
            var fieldDeclaration = SyntaxFactory.FieldDeclaration(variableDeclaration)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));

            // Create a Property: (public int Quantity { get; set; })
            var propertyDeclaration = SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName("int"), "Quantity")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));

            // Create a stament with the body of a method.
            var syntax = SyntaxFactory.ParseStatement("canceled = true;");

            // Create a method
            var methodDeclaration = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), "MarkAsCanceled")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .WithBody(SyntaxFactory.Block(syntax));

            // Add the field, the property and method to the class.
            classDeclaration = classDeclaration.AddMembers(fieldDeclaration, propertyDeclaration, methodDeclaration);

            // Add the class to the namespace.
            @namespace = @namespace.AddMembers(classDeclaration);

            // Normalize and get code as string.
            var code = @namespace
                .NormalizeWhitespace()
                .ToFullString();

            // Output new code to the console.
            Console.WriteLine(code);
        }

        public static void GenerateAssembly()
        {
            const string code = @"using System;
using System.IO;
namespace RoslynCore
{
 public static class Helper
 {
  public static double CalculateCircleArea(double radius)
  {
    return radius * radius * Math.PI;
  }
  }
}";
            var tree = SyntaxFactory.ParseSyntaxTree(code);
            string fileName = "mylib.dll";
            // Detect the file location for the library that defines the object type
            var systemRefLocation = typeof(object).GetTypeInfo().Assembly.Location;
            // Create a reference to the library
            var systemReference = MetadataReference.CreateFromFile(systemRefLocation);
            // A single, immutable invocation to the compiler
            // to produce a library
            var compilation = CSharpCompilation.Create(fileName)
              .WithOptions(
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
              .AddReferences(systemReference)
              .AddSyntaxTrees(tree);
            string path = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            EmitResult compilationResult = compilation.Emit(path);
            if (compilationResult.Success)
            {
                // Load the assembly
                Assembly asm =
                  AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
                // Invoke the RoslynCore.Helper.CalculateCircleArea method passing an argument
                double radius = 10;
                object result =
                  asm.GetType("RoslynCore.Helper").GetMethod("CalculateCircleArea").
                  Invoke(null, new object[] { radius });
                Console.WriteLine($"Circle area with radius = {radius} is {result}");
            }
            else
            {
                foreach (Diagnostic codeIssue in compilationResult.Diagnostics)
                {
                    string issue = @$"ID: {codeIssue.Id}, Message: {codeIssue.GetMessage()},
                    Location: { codeIssue.Location.GetLineSpan()},
            Severity: { codeIssue.Severity}
                    ";
                  Console.WriteLine(issue);
                }
            }
        }
    }
}
