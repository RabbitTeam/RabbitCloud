using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Rabbit.Rpc.Ids;
using Rabbit.Rpc.ProxyGenerator.Utilitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Rabbit.Rpc.ProxyGenerator.Implementation
{
    public class ServiceProxyGenerater : IServiceProxyGenerater
    {
        #region Field

        private readonly IServiceIdGenerator _serviceIdGenerator;

        #endregion Field

        #region Constructor

        public ServiceProxyGenerater(IServiceIdGenerator serviceIdGenerator)
        {
            _serviceIdGenerator = serviceIdGenerator;
        }

        #endregion Constructor

        #region Implementation of IServiceProxyGenerater

        /// <summary>
        /// 生成服务代理。
        /// </summary>
        /// <param name="interfacTypes">需要被代理的接口类型。</param>
        /// <returns>服务代理实现。</returns>
        public IEnumerable<Type> GenerateProxys(IEnumerable<Type> interfacTypes)
        {
            var trees = interfacTypes.Select(GenerateProxyTree).ToList();
            var bytes = CompilationUtilitys.CompileClientProxy(trees,
                AppDomain.CurrentDomain.GetAssemblies()
                    .Select(a => MetadataReference.CreateFromFile(a.Location))
                    .Concat(new[]
                    {
                        MetadataReference.CreateFromFile(typeof(Task).Assembly.Location)
                    }));
            var assembly = Assembly.Load(bytes);
            return assembly.GetExportedTypes();
        }

        /// <summary>
        /// 生成服务代理代码树。
        /// </summary>
        /// <param name="interfaceType">需要被代理的接口类型。</param>
        /// <returns>代码树。</returns>
        public SyntaxTree GenerateProxyTree(Type interfaceType)
        {
            var className = interfaceType.Name.StartsWith("I") ? interfaceType.Name.Substring(1) : interfaceType.Name;
            className += "ClientProxy";

            var members = new List<MemberDeclarationSyntax>
            {
                GetConstructorDeclaration(className)
            };

            members.AddRange(GenerateMethodDeclarations(interfaceType.GetMethods()));
            return CompilationUnit()
                .WithUsings(GetUsings())
                .WithMembers(
                    SingletonList<MemberDeclarationSyntax>(
                        NamespaceDeclaration(
                            QualifiedName(
                                QualifiedName(
                                    IdentifierName("Rabbit"),
                                    IdentifierName("Rpc")),
                                IdentifierName("ClientProxys")))
                .WithMembers(
                    SingletonList<MemberDeclarationSyntax>(
                        ClassDeclaration(className)
                            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                            .WithBaseList(
                                BaseList(
                                    SeparatedList<BaseTypeSyntax>(
                                        new SyntaxNodeOrToken[]
                                        {
                                            SimpleBaseType(IdentifierName("ServiceProxyBase")),
                                            Token(SyntaxKind.CommaToken),
                                            SimpleBaseType(GetQualifiedNameSyntax(interfaceType))
                                        })))
                            .WithMembers(List(members))))))
                .NormalizeWhitespace().SyntaxTree;
        }

        #endregion Implementation of IServiceProxyGenerater

        #region Private Method

        private static QualifiedNameSyntax GetQualifiedNameSyntax(Type type)
        {
            var fullName = type.Namespace + "." + type.Name;
            return GetQualifiedNameSyntax(fullName);
        }

        private static QualifiedNameSyntax GetQualifiedNameSyntax(string fullName)
        {
            return GetQualifiedNameSyntax(fullName.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries));
        }

        private static QualifiedNameSyntax GetQualifiedNameSyntax(IReadOnlyCollection<string> names)
        {
            var ids = names.Select(IdentifierName).ToArray();

            var index = 0;
            QualifiedNameSyntax left = null;
            while (index + 1 < names.Count)
            {
                left = left == null ? QualifiedName(ids[index], ids[index + 1]) : QualifiedName(left, ids[index + 1]);
                index++;
            }
            return left;
        }

        private static SyntaxList<UsingDirectiveSyntax> GetUsings()
        {
            return List(
                new[]
                {
                    UsingDirective(IdentifierName("System")),
                    UsingDirective(GetQualifiedNameSyntax("System.Threading.Tasks")),
                    UsingDirective(GetQualifiedNameSyntax("System.Collections.Generic")),
                    UsingDirective(GetQualifiedNameSyntax("Rabbit.Rpc.Client")),
                    UsingDirective(GetQualifiedNameSyntax("Rabbit.Rpc.Serialization")),
                    UsingDirective(GetQualifiedNameSyntax("Rabbit.Rpc.ProxyGenerator.Implementation"))
                });
        }

        private static ConstructorDeclarationSyntax GetConstructorDeclaration(string className)
        {
            return ConstructorDeclaration(Identifier(className))
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword)))
                .WithParameterList(
                    ParameterList(
                        SeparatedList<ParameterSyntax>(
                                new SyntaxNodeOrToken[]{
                                    Parameter(
                                        Identifier("remoteInvokeService"))
                                    .WithType(
                                        IdentifierName("IRemoteInvokeService")),
                                    Token(SyntaxKind.CommaToken),
                                    Parameter(
                                        Identifier("serializer"))
                                    .WithType(
                                        IdentifierName("ISerializer"))})))
                .WithInitializer(
                        ConstructorInitializer(
                            SyntaxKind.BaseConstructorInitializer,
                            ArgumentList(
                                SeparatedList<ArgumentSyntax>(
                                    new SyntaxNodeOrToken[]{
                                        Argument(
                                            IdentifierName("remoteInvokeService")),
                                        Token(SyntaxKind.CommaToken),
                                        Argument(
                                            IdentifierName("serializer"))}))))
                .WithBody(Block());
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateMethodDeclarations(IEnumerable<MethodInfo> methods)
        {
            var array = methods.ToArray();
            return array.Select(GenerateMethodDeclaration).ToArray();
        }

        private MemberDeclarationSyntax GenerateMethodDeclaration(MethodInfo method)
        {
            var serviceId = _serviceIdGenerator.GenerateServiceId(method);

            var arguments = method.ReturnType.GetGenericArguments();
            var resultType = arguments.Any() ? arguments.First() : null;
            SimpleNameSyntax returnDeclaration;
            if (resultType != null)
            {
                returnDeclaration = GenericName(Identifier("Task")).WithTypeArgumentList(
                    TypeArgumentList(
                        SingletonSeparatedList<TypeSyntax>(
                            GetQualifiedNameSyntax(resultType))));
            }
            else
            {
                returnDeclaration = IdentifierName("Task");
            }

            var parameterList = new List<SyntaxNodeOrToken>();
            var parameterDeclarationList = new List<SyntaxNodeOrToken>();

            foreach (var parameter in method.GetParameters())
            {
                parameterDeclarationList.Add(Parameter(
                                    Identifier(parameter.Name))
                                    .WithType(GetQualifiedNameSyntax(parameter.ParameterType)));
                parameterDeclarationList.Add(Token(SyntaxKind.CommaToken));

                parameterList.Add(InitializerExpression(
                    SyntaxKind.ComplexElementInitializerExpression,
                    SeparatedList<ExpressionSyntax>(
                        new SyntaxNodeOrToken[]{
                            LiteralExpression(
                                SyntaxKind.StringLiteralExpression,
                                Literal(parameter.Name)),
                            Token(SyntaxKind.CommaToken),
                            IdentifierName(parameter.Name)})));
                parameterList.Add(Token(SyntaxKind.CommaToken));
            }
            if (parameterList.Any())
            {
                parameterList.RemoveAt(parameterList.Count - 1);
                parameterDeclarationList.RemoveAt(parameterDeclarationList.Count - 1);
            }

            var declaration = MethodDeclaration(
                returnDeclaration,
                Identifier(method.Name))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.AsyncKeyword)))
                .WithParameterList(ParameterList(SeparatedList<ParameterSyntax>(parameterDeclarationList)));

            ExpressionSyntax expressionSyntax;
            StatementSyntax statementSyntax;

            if (resultType != null)
            {
                expressionSyntax = GenericName(
                    Identifier("Invoke")).WithTypeArgumentList(
                        TypeArgumentList(
                            SingletonSeparatedList<TypeSyntax>(GetQualifiedNameSyntax(resultType))));
            }
            else
            {
                expressionSyntax = IdentifierName("Invoke");
            }
            expressionSyntax = AwaitExpression(
                InvocationExpression(expressionSyntax)
                    .WithArgumentList(
                        ArgumentList(
                            SeparatedList<ArgumentSyntax>(
                                new SyntaxNodeOrToken[]
                                {
                                        Argument(
                                            ObjectCreationExpression(
                                                GenericName(
                                                    Identifier("Dictionary"))
                                                    .WithTypeArgumentList(
                                                        TypeArgumentList(
                                                            SeparatedList<TypeSyntax>(
                                                                new SyntaxNodeOrToken[]
                                                                {
                                                                    PredefinedType(
                                                                        Token(SyntaxKind.StringKeyword)),
                                                                    Token(SyntaxKind.CommaToken),
                                                                    PredefinedType(
                                                                        Token(SyntaxKind.ObjectKeyword))
                                                                }))))
                                                .WithInitializer(
                                                    InitializerExpression(
                                                        SyntaxKind.CollectionInitializerExpression,
                                                        SeparatedList<ExpressionSyntax>(
                                                            parameterList)))),
                                        Token(SyntaxKind.CommaToken),
                                        Argument(
                                            LiteralExpression(
                                                SyntaxKind.StringLiteralExpression,
                                                Literal(serviceId)))
                                }))));

            if (resultType != null)
            {
                statementSyntax = ReturnStatement(expressionSyntax);
            }
            else
            {
                statementSyntax = ExpressionStatement(expressionSyntax);
            }

            declaration = declaration.WithBody(
                        Block(
                            SingletonList(statementSyntax)));

            return declaration;
        }

        #endregion Private Method
    }
}