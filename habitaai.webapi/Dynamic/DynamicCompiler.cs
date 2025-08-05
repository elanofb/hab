// Dynamic/DynamicCompiler.cs
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;
using habitaai.webapi.domain;
//using System.Data;
//using System.Data.SqlClient; // <- isso força o carregamento da DLL necessária
using Microsoft.Data.SqlClient;

namespace habitaai.webapi.Dynamic;

public static class DynamicCompiler
{
    public static RequestDelegate Compile(string code, string methodName = "Handle")
    {
        var tree = CSharpSyntaxTree.ParseText(code);

        var refs = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .ToList();

        // 🔽 Adiciona manualmente a referência à System.Data.SqlClient
        var sqlClientAssemblyLocation = typeof(SqlConnection).Assembly.Location;
        refs.Add(MetadataReference.CreateFromFile(sqlClientAssemblyLocation));

        // Se também estiver usando System.Text.Json
        var jsonAssemblyLocation = typeof(System.Text.Json.JsonSerializer).Assembly.Location;
        refs.Add(MetadataReference.CreateFromFile(jsonAssemblyLocation));

        // Se usar Encoding.UTF8
        var encodingAssemblyLocation = typeof(System.Text.Encoding).Assembly.Location;
        refs.Add(MetadataReference.CreateFromFile(encodingAssemblyLocation));

        var compilation = CSharpCompilation.Create(
            Guid.NewGuid().ToString(),
            new[] { tree },
            references: refs,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);
        if (!result.Success)
        {
            var errors = string.Join("\n", result.Diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => d.ToString()));
            throw new Exception($"Erro ao compilar código: \n{errors}");
        }

        ms.Seek(0, SeekOrigin.Begin);
        var assembly = Assembly.Load(ms.ToArray());

        var type = assembly.GetTypes().FirstOrDefault(t => t.GetMethod(methodName) != null)
                   ?? throw new Exception("Método não encontrado.");

        var method = type.GetMethod(methodName)!;

        return (RequestDelegate)Delegate.CreateDelegate(typeof(RequestDelegate), method);
    }
}
