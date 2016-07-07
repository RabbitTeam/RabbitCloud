using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rabbit.Rpc.ProxyGenerator;
using Rabbit.Rpc.ProxyGenerator.Utilitys;
using Rabbit.Rpc.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace Rabbit.Rpc.ClientGenerator
{
    public class Program
    {
        #region Field

        /// <summary>
        /// 输出路径。
        /// </summary>
        private static readonly string OutputDirectory = Path.Combine(AppContext.BaseDirectory, "outputs");

        /// <summary>
        /// 服务代理生成器。
        /// </summary>
        private static readonly IServiceProxyGenerater ServiceProxyGenerater;

        /// <summary>
        /// 所获取到的程序集文件路径。
        /// </summary>
        private static readonly IEnumerable<string> AssemblyFiles;

        /// <summary>
        /// 加载到的程序集。
        /// </summary>
        private static readonly Assembly[] Assemblies;

        /// <summary>
        /// 命令方法字典。
        /// </summary>
        private static readonly Dictionary<string, Action> CommandActions = new Dictionary<string, Action>
        {
            {"1", GenerateAssembly},
            {"2", GenerateCodeFiles}
        };

        private static readonly ILogger Logger;

        #endregion Field

        #region Constructor

        static Program()
        {
            var services = new ServiceCollection();

            services
                .AddLogging()
                .AddRpcCore()
                .AddClientProxy();

            var provider = services.BuildServiceProvider();

            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            loggerFactory
                .AddConsole();

            Logger = loggerFactory.CreateLogger<Program>();

            ServiceProxyGenerater = provider.GetRequiredService<IServiceProxyGenerater>();

            AssemblyFiles =
                Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "assemblies"), "*.dll").ToArray();

            Assemblies = AssemblyFiles.Select(i =>
            {
                using (var stream = File.OpenRead(i))
                {
                    return AssemblyLoadContext.Default.LoadFromStream(stream);
                }
            }).ToArray();

            Console.WriteLine("Successfully loaded the following assemblies");
            foreach (var name in Assemblies.Select(i => i.GetName().Name))
            {
                Console.WriteLine(name);
            }
        }

        #endregion Constructor

        private static void Main()
        {
            while (true)
            {
                Console.WriteLine("1.Generate the client proxy assemblies");
                Console.WriteLine("2.Generate client proxy code");

                var command = Console.ReadLine() ?? string.Empty;

                if (!Directory.Exists(OutputDirectory))
                    Directory.CreateDirectory(OutputDirectory);

                Action action;
                if (!CommandActions.TryGetValue(command, out action))
                {
                    Console.WriteLine("Invalid Input!");
                    continue;
                }

                action();
            }
        }

        #region Private Method

        private static void GenerateAssembly()
        {
            using (var stream = CompilationUtilitys.CompileClientProxy(GetTrees(), AssemblyFiles.Select(file => MetadataReference.CreateFromFile(file)), Logger))
            {
                var fileName = Path.Combine(OutputDirectory, "Rabbit.Rpc.ClientProxys.dll");
                File.WriteAllBytes(fileName, stream.ToArray());
                Console.WriteLine($"Generate successful path:{ fileName}");
            }
        }

        private static void GenerateCodeFiles()
        {
            foreach (var syntaxTree in GetTrees())
            {
                var compilationUnitSyntax = (CompilationUnitSyntax)syntaxTree.GetRoot();

                var classDeclarationSyntax = FindClassDeclaration(compilationUnitSyntax.Members);
                var className = classDeclarationSyntax.Identifier.ValueText;
                var code = syntaxTree.ToString();
                var fileName = Path.Combine(OutputDirectory, $"{className}.cs");
                File.WriteAllText(fileName, code, Encoding.UTF8);
                Console.WriteLine($"Generate successful path:{fileName}");
            }
        }

        private static IEnumerable<SyntaxTree> GetTrees()
        {
            var services = Assemblies
                .SelectMany(assembly => assembly.GetExportedTypes())
                .Where(i => i.GetTypeInfo().IsInterface && i.GetTypeInfo().GetCustomAttribute<RpcServiceAttribute>() != null);
            return services.Select(service => ServiceProxyGenerater.GenerateProxyTree(service));
        }

        private static ClassDeclarationSyntax FindClassDeclaration(IEnumerable<MemberDeclarationSyntax> members)
        {
            foreach (var member in members)
            {
                var classDeclaration = member as ClassDeclarationSyntax;
                if (classDeclaration != null)
                    return classDeclaration;

                var namespaceDeclaration = member as NamespaceDeclarationSyntax;
                if (namespaceDeclaration != null)
                    return FindClassDeclaration(namespaceDeclaration.Members);
            }
            return null;
        }

        #endregion Private Method
    }
}