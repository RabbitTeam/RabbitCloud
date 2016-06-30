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
        #region Field

        /// <summary>
        /// 输出路径。
        /// </summary>
        private static readonly string OutputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "outputs");

        /// <summary>
        /// 服务代理生成器。
        /// </summary>
        private static readonly IServiceProxyGenerater ServiceProxyGenerater = new ServiceProxyGenerater(new DefaultServiceIdGenerator(new NullLogger<DefaultServiceIdGenerator>()));

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

        #endregion Field

        #region Constructor

        static Program()
        {
            AssemblyFiles =
                Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assemblies"), "*.dll").ToArray();
            Assemblies = AssemblyFiles.Select(i => Assembly.Load(File.ReadAllBytes(i))).ToArray();

            Console.WriteLine("成功加载了以下程序集");
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
                Console.WriteLine("1.生成客户端代理程序集");
                Console.WriteLine("2.生成客户端代理代码");

                var command = Console.ReadLine() ?? string.Empty;

                if (!Directory.Exists(OutputDirectory))
                    Directory.CreateDirectory(OutputDirectory);

                Action action;
                if (!CommandActions.TryGetValue(command, out action))
                {
                    Console.WriteLine("无效的输入！");
                    continue;
                }

                action();
            }
        }

        #region Private Method

        private static void GenerateAssembly()
        {
            var bytes = CompilationUtilitys.CompileClientProxy(GetTrees(), AssemblyFiles.Select(file => MetadataReference.CreateFromFile(file)));
            {
                var fileName = Path.Combine(OutputDirectory, "Rabbit.Rpc.ClientProxys.dll");
                File.WriteAllBytes(fileName, bytes);
                Console.WriteLine($"生成成功，路径：{fileName}");
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
                Console.WriteLine($"生成成功，路径：{fileName}");
            }
        }

        private static IEnumerable<SyntaxTree> GetTrees()
        {
            var services = Assemblies
                .SelectMany(assembly => assembly.GetExportedTypes())
                .Where(i => i.IsInterface && i.GetCustomAttribute<RpcServiceAttribute>() != null);
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