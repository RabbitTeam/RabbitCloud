using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Rabbit.Rpc.Ids.Implementation;
using Rabbit.Rpc.Logging;
using Rabbit.Rpc.ProxyGenerator;
using Rabbit.Rpc.ProxyGenerator.Implementation;
using Rabbit.Rpc.ProxyGenerator.Utilitys;
using Rabbit.Rpc.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Rabbit.Rpc.ClientGenerator
{
    internal class Program
    {
        private static void Main()
        {
            var assemblyFiles =
                Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assemblies"), "*.dll").ToArray();
            var assemblies = assemblyFiles.Select(i => Assembly.Load(File.ReadAllBytes(i))).ToArray();

            IServiceProxyGenerater serviceProxyGenerater = new ServiceProxyGenerater(new DefaultServiceIdGenerator(new NullLogger<DefaultServiceIdGenerator>()));

            Console.WriteLine("成功加载了以下程序集");
            foreach (var name in assemblies.Select(i => i.GetName().Name))
            {
                Console.WriteLine(name);
            }

            var services = assemblies
                .SelectMany(assembly => assembly.GetExportedTypes())
                .Where(i => i.IsInterface && i.GetCustomAttribute<RpcServiceAttribute>() != null);

            while (true)
            {
                Console.WriteLine("1.生成客户端代理程序集");
                Console.WriteLine("2.生成客户端代理代码");

                var command = Console.ReadLine();

                Func<IEnumerable<SyntaxTree>> getTrees = () =>
                {
                    var trees = new List<SyntaxTree>();
                    foreach (var service in services)
                    {
                        trees.Add(serviceProxyGenerater.GenerateProxyTree(service));
                    }
                    return trees;
                };

                var outputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "outputs");
                if (!Directory.Exists(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }

                switch (command)
                {
                    case "1":
                        var bytes = CompilationUtilitys.CompileClientProxy(getTrees(), assemblyFiles.Select(file => MetadataReference.CreateFromFile(file)));
                        {
                            var fileName = Path.Combine(outputDirectory, "Rabbit.Rpc.ClientProxys.dll");
                            File.WriteAllBytes(fileName, bytes);
                            Console.WriteLine($"生成成功，路径：{fileName}");
                        }
                        break;

                    case "2":
                        foreach (var syntaxTree in getTrees())
                        {
                            var className = string.Empty;
                            var tempRoot = ((CompilationUnitSyntax)syntaxTree.GetRoot()).Members[0];
                            if (tempRoot is NamespaceDeclarationSyntax)
                            {
                                className = ((tempRoot as NamespaceDeclarationSyntax).Members[0] as ClassDeclarationSyntax).Identifier.ValueText;
                            }
                            else if (tempRoot is ClassDeclarationSyntax)
                            {
                                className = (tempRoot as ClassDeclarationSyntax).Identifier.ValueText;
                            }
                            var code = syntaxTree.ToString();
                            var fileName = Path.Combine(outputDirectory, $"{className}.cs");
                            File.WriteAllText(fileName, code, Encoding.UTF8);
                            Console.WriteLine($"生成成功，路径：{fileName}");
                        }
                        break;

                    default:
                        Console.WriteLine("无效的输入！");
                        continue;
                }
            }
        }
    }
}