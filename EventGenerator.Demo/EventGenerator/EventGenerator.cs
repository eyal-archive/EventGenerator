namespace EventGenerator;

using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

[Generator]
internal sealed class EventGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (syntaxNode, _) => syntaxNode is ClassDeclarationSyntax classDeclaration && classDeclaration.Identifier.Text == "EventHandlers",
                transform: (context, _) => (ClassDeclarationSyntax)context.Node
            )
            .Where(classDeclaration => classDeclaration.Identifier.Text == "EventHandlers")
            .Collect();

        context.RegisterSourceOutput(classDeclarations, (context, classDeclarations) =>
        {
            var sourceBuilder = new StringBuilder();
            sourceBuilder.AppendLine("partial class Journal");
            sourceBuilder.AppendLine("{");

            foreach (var classDeclaration in classDeclarations)
            {
                foreach (var member in classDeclaration.Members)
                {
                    if (member is FieldDeclarationSyntax field && IsEventHandlerField(field))
                    {
                        var typeArgument = GetTypeArgument(field.Declaration.Type);
                        var fieldName = field.Declaration.Variables.First().Identifier.Text;
                        var fieldSource = GenerateSourceCodeForField(fieldName, typeArgument);
                        
                        sourceBuilder.AppendLine(fieldSource);
                    }
                }
            }

            sourceBuilder.AppendLine("}");

            context.AddSource("Journal.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
        });
    }

    private static bool IsEventHandlerField(FieldDeclarationSyntax field)
    {
        var typeSyntax = field.Declaration.Type;

        return typeSyntax is GenericNameSyntax genericNameSyntax &&
               genericNameSyntax.Identifier.Text == "EventHandler" &&
               genericNameSyntax.TypeArgumentList.Arguments.Count == 1;
    }

    private static string GetTypeArgument(TypeSyntax typeSyntax)
    {
        if (typeSyntax is GenericNameSyntax genericNameSyntax)
        {
            return genericNameSyntax.TypeArgumentList.Arguments.First().ToString();
        }

        return string.Empty;
    }

    private static string GenerateSourceCodeForField(string fieldName, string typeArgument)
        => $@"
    public event EventHandler<{typeArgument}> {fieldName}
    {{
        add => EventHandlers.{fieldName}.Updated += value;
        remove => EventHandlers.{fieldName}.Updated -= value;
    }}
";
}
