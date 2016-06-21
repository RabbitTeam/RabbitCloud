using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Rabbit.Rpc.Client;
using Rabbit.Rpc.Ids.Implementation;
using Rabbit.Rpc.ProxyGenerator;
using Rabbit.Rpc.ProxyGenerator.Implementation;
using Rabbit.Rpc.Server.Implementation.ServiceDiscovery.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Rabbit.Rpc.ClientGenerator
{
    internal class Program
    {
        private static void Main()
        {
            var assemblyFiles =
                Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assemblies"), "*.dll").ToArray();
            var assemblies = assemblyFiles.Select(i => Assembly.Load(File.ReadAllBytes(i))).ToArray();

            IServiceProxyGenerater serviceProxyGenerater = new ServiceProxyGenerater(new DefaultServiceIdGenerator());

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

                        IEnumerable<MetadataReference> references = assemblyFiles.Select(file => MetadataReference.CreateFromFile(file));
                        var compilation = CSharpCompilation.Create("Rabbit.Rpc.Proxys.dll", getTrees(), references.Concat(new[]
                        {
                            MetadataReference.CreateFromFile(typeof(Task).Assembly.Location),
                            MetadataReference.CreateFromFile(typeof(ServiceDescriptor).Assembly.Location),
                            MetadataReference.CreateFromFile(typeof(IRemoteInvokeService).Assembly.Location),
                            MetadataReference.CreateFromFile(typeof(IServiceProxyGenerater).Assembly.Location)
                        }), new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
                        using (var stream = new MemoryStream())
                        {
                            var result = compilation.Emit(stream);
                            if (!result.Success)
                            {
                                Console.WriteLine("编译失败，错误信息：");
                                foreach (var message in result.Diagnostics.Select(i => i.ToString()))
                                {
                                    Console.WriteLine(message);
                                }
                                continue;
                            }
                            {
                                var fileName = Path.Combine(outputDirectory, "Rabbit.Rpc.Proxys.dll");
                                File.WriteAllBytes(fileName, stream.GetBuffer());
                                Console.WriteLine($"生成成功，路径：{fileName}");
                            }
                        }
                        break;

                    case "2":
                        foreach (var syntaxTree in getTrees())
                        {
                            var className = ((ClassDeclarationSyntax)((CompilationUnitSyntax)syntaxTree.GetRoot()).Members[0]).Identifier.Value;
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