using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Rabbit.Rpc.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Rabbit.Rpc.ProxyGenerator.Utilitys
{
    public static class CompilationUtilitys
    {
        #region Public Method

        public static byte[] CompileClientProxy(IEnumerable<SyntaxTree> trees, IEnumerable<MetadataReference> references)
        {
            references = new[]
            {
                MetadataReference.CreateFromFile(typeof(Task).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ServiceDescriptor).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(IRemoteInvokeService).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(IServiceProxyGenerater).Assembly.Location)
            }.Concat(references);
            return Compile(AssemblyInfo.Create("Rabbit.Rpc.Proxys"), trees, references);
        }

        public static byte[] Compile(AssemblyInfo assemblyInfo, IEnumerable<SyntaxTree> trees, IEnumerable<MetadataReference> references)
        {
            return Compile(assemblyInfo.Title + ".dll", assemblyInfo, trees, references);
        }

        public static byte[] Compile(string assemblyName, AssemblyInfo assemblyInfo, IEnumerable<SyntaxTree> trees, IEnumerable<MetadataReference> references)
        {
            trees = trees.Concat(new[] { GetAssemblyInfo(assemblyInfo) });
            var compilation = CSharpCompilation.Create(assemblyName, trees, references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            using (var stream = new MemoryStream())
            {
                var result = compilation.Emit(stream);
                if (!result.Success)
                {
                    foreach (var message in result.Diagnostics.Select(i => i.ToString()))
                    {
                        Console.WriteLine(message);
                    }
                    return null;
                }
                return stream.GetBuffer();
            }
        }

        #endregion Public Method

        #region Private Method

        private static SyntaxTree GetAssemblyInfo(AssemblyInfo info)
        {
            return CompilationUnit()
                .WithUsings(
                    List(
                        new[]
                        {
                            UsingDirective(
                                QualifiedName(
                                    IdentifierName("System"),
                                    IdentifierName("Reflection"))),
                            UsingDirective(
                                QualifiedName(
                                    QualifiedName(
                                        IdentifierName("System"),
                                        IdentifierName("Runtime")),
                                    IdentifierName("InteropServices")))
                        }))
                .WithAttributeLists(
                    List(
                        new[]
                        {
                            AttributeList(
                                SingletonSeparatedList(
                                    Attribute(
                                        IdentifierName("AssemblyTitle"))
                                        .WithArgumentList(
                                            AttributeArgumentList(
                                                SingletonSeparatedList(
                                                    AttributeArgument(
                                                        LiteralExpression(
                                                            SyntaxKind.StringLiteralExpression,
                                                            Literal(info.Title))))))))
                                .WithTarget(
                                    AttributeTargetSpecifier(
                                        Token(SyntaxKind.AssemblyKeyword))),
                            AttributeList(
                                SingletonSeparatedList(
                                    Attribute(
                                        IdentifierName("AssemblyProduct"))
                                        .WithArgumentList(
                                            AttributeArgumentList(
                                                SingletonSeparatedList(
                                                    AttributeArgument(
                                                        LiteralExpression(
                                                            SyntaxKind.StringLiteralExpression,
                                                            Literal(info.Product))))))))
                                .WithTarget(
                                    AttributeTargetSpecifier(
                                        Token(SyntaxKind.AssemblyKeyword))),
                            AttributeList(
                                SingletonSeparatedList(
                                    Attribute(
                                        IdentifierName("AssemblyCopyright"))
                                        .WithArgumentList(
                                            AttributeArgumentList(
                                                SingletonSeparatedList(
                                                    AttributeArgument(
                                                        LiteralExpression(
                                                            SyntaxKind.StringLiteralExpression,
                                                            Literal(info.Copyright))))))))
                                .WithTarget(
                                    AttributeTargetSpecifier(
                                        Token(SyntaxKind.AssemblyKeyword))),
                            AttributeList(
                                SingletonSeparatedList(
                                    Attribute(
                                        IdentifierName("ComVisible"))
                                        .WithArgumentList(
                                            AttributeArgumentList(
                                                SingletonSeparatedList(
                                                    AttributeArgument(
                                                        LiteralExpression(info.ComVisible
                                                            ? SyntaxKind.TrueLiteralExpression
                                                            : SyntaxKind.FalseLiteralExpression)))))))
                                .WithTarget(
                                    AttributeTargetSpecifier(
                                        Token(SyntaxKind.AssemblyKeyword))),
                            AttributeList(
                                SingletonSeparatedList(
                                    Attribute(
                                        IdentifierName("Guid"))
                                        .WithArgumentList(
                                            AttributeArgumentList(
                                                SingletonSeparatedList(
                                                    AttributeArgument(
                                                        LiteralExpression(
                                                            SyntaxKind.StringLiteralExpression,
                                                            Literal(info.Guid))))))))
                                .WithTarget(
                                    AttributeTargetSpecifier(
                                        Token(SyntaxKind.AssemblyKeyword))),
                            AttributeList(
                                SingletonSeparatedList(
                                    Attribute(
                                        IdentifierName("AssemblyVersion"))
                                        .WithArgumentList(
                                            AttributeArgumentList(
                                                SingletonSeparatedList(
                                                    AttributeArgument(
                                                        LiteralExpression(
                                                            SyntaxKind.StringLiteralExpression,
                                                            Literal(info.Version))))))))
                                .WithTarget(
                                    AttributeTargetSpecifier(
                                        Token(SyntaxKind.AssemblyKeyword))),
                            AttributeList(
                                SingletonSeparatedList(
                                    Attribute(
                                        IdentifierName("AssemblyFileVersion"))
                                        .WithArgumentList(
                                            AttributeArgumentList(
                                                SingletonSeparatedList(
                                                    AttributeArgument(
                                                        LiteralExpression(
                                                            SyntaxKind.StringLiteralExpression,
                                                            Literal(info.FileVersion))))))))
                                .WithTarget(
                                    AttributeTargetSpecifier(
                                        Token(SyntaxKind.AssemblyKeyword)))
                        }))
                .NormalizeWhitespace()
                .SyntaxTree;
        }

        #endregion Private Method

        #region Help Class

        internal class AssemblyInfo
        {
            public string Title { get; set; }
            public string Product { get; set; }
            public string Copyright { get; set; }
            public string Guid { get; set; }
            public string Version { get; set; }
            public string FileVersion { get; set; }
            public bool ComVisible { get; set; }

            public static AssemblyInfo Create(string name, string copyright = "Copyright ©  Rabbit", string version = "1.0.0.0")
            {
                return new AssemblyInfo
                {
                    Title = name,
                    Product = name,
                    Copyright = copyright,
                    Guid = System.Guid.NewGuid().ToString("D"),
                    ComVisible = false,
                    Version = version,
                    FileVersion = version
                };
            }
        }

        #endregion Help Class
    }
}