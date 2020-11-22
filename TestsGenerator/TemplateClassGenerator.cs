﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsGenerator
{
    public class TemplateClassGenerator
    {
        public TestClassTemplate GetTemplate(string sourceCode)
        {
            List<TestClassTemplate> templates = GetTestTemplates(sourceCode);
            if (templates.Count != 0)
            {
                string fileName = templates[0].FileName;
                string innerText = "";
                foreach (TestClassTemplate template in templates)
                {
                    innerText += template.InnerText;
                }
                return new TestClassTemplate(fileName, innerText);
            }
            else
                return null;
        }

        private List<TestClassTemplate> GetTestTemplates(string sourceCode)
        {
            SyntaxProcessor syntaxProcessor = new SyntaxProcessor();
            SyntaxProcessResult syntaxProcessResult = syntaxProcessor.Process(sourceCode);
            List<TestClassTemplate> result = new List<TestClassTemplate>();
            foreach (ClassInformation classInfo in syntaxProcessResult.Classes)
            {
                NamespaceDeclarationSyntax namespaceDeclaration = NamespaceDeclaration(
                    QualifiedName(
                        IdentifierName(classInfo.NamespaceName),
                        IdentifierName("Tests")));
                CompilationUnitSyntax testClass = CompilationUnit()
                    .WithUsings(GetTemplateUsings(classInfo))
                    .WithMembers(SingletonList<MemberDeclarationSyntax>(namespaceDeclaration
                        .WithMembers(SingletonList<MemberDeclarationSyntax>(ClassDeclaration(classInfo.Name + "Tests")
                            .WithAttributeLists(
                                SingletonList(
                                    AttributeList(
                                        SingletonSeparatedList(
                                            Attribute(
                                                IdentifierName("TestClass"))))))
                            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                            .WithMembers(GetTestMethods(classInfo.Methods))))));
                string fileName = classInfo.Name + "Tests.cs";
                string innerText = testClass.NormalizeWhitespace().ToFullString();
                result.Add(new TestClassTemplate(fileName, innerText));
            }
            return result;
        }

        private SyntaxList<MemberDeclarationSyntax> GetTestMethods(List<string> methods)
        {
            List<MemberDeclarationSyntax> result = new List<MemberDeclarationSyntax>();
            foreach (string method in methods)
            {
                result.Add(GenerateTestMethod(method));
            }
            return new SyntaxList<MemberDeclarationSyntax>(result);
        }

        private MethodDeclarationSyntax GenerateTestMethod(string methodName)
        {
            string attributeForTemplate = "TestMethod";
            string methodBody = "Assert.Fail(\"autogenerated\");";
            return MethodDeclaration(
                PredefinedType(
                    Token(SyntaxKind.VoidKeyword)),
                Identifier(methodName + "Test"))
                .WithAttributeLists(
                    SingletonList(
                        AttributeList(
                            SingletonSeparatedList(
                                Attribute(
                                    IdentifierName(attributeForTemplate))))))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                .WithBody(Block(ParseStatement(methodBody)));
        }

        private SyntaxList<UsingDirectiveSyntax> GetTemplateUsings(ClassInformation classInfo)
        {
            List<UsingDirectiveSyntax> usings = new List<UsingDirectiveSyntax>
            {
                UsingDirective
                (
                    IdentifierName("System")
                ),
                UsingDirective
                (
                    QualifiedName
                    (
                        QualifiedName
                        (
                            IdentifierName("System"),
                            IdentifierName("Collections")
                        ),
                        IdentifierName("Generic")
                    )
                ),
                UsingDirective
                (
                    QualifiedName
                    (
                        IdentifierName("System"),
                        IdentifierName("Linq")
                    )
                ),
                UsingDirective
                (
                    QualifiedName
                    (
                        QualifiedName
                        (
                            QualifiedName
                            (
                                IdentifierName("Microsoft"),
                                IdentifierName("VisualStudio")
                            ),
                            IdentifierName("TestTools")
                        ),
                        IdentifierName("UnitTesting")
                    )
                ),
                UsingDirective
                (
                    IdentifierName(classInfo.NamespaceName)
                )
            };
            return new SyntaxList<UsingDirectiveSyntax>(usings);
        }
    }
}