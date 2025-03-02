﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace HuaweiCloud.OBS.Extensions.Async.SourceGenerators;

[Generator]
public class AsyncExtensionGenerator : IIncrementalGenerator
{
    private static IEnumerable<(string, string?)> ExportedAsyncMethods()
    {
        yield return ("AppendObject", null);
        yield return ("AbortMultipartUpload", null);
        yield return ("CopyObject", null);
        yield return ("CopyPart", null);
        yield return ("CompleteMultipartUpload", null);
        yield return ("DeleteObject", null);
        yield return ("DeleteObjects", null);
        yield return ("GetObject", null);
        yield return ("GetObjectAcl", null);
        yield return ("GetObjectMetadata", null);
        yield return ("HeadObject", "bool");
        yield return ("InitiateMultipartUpload", null);
        yield return ("ListParts", null);
        yield return ("ListObjects", null);
        yield return ("PutObject", null);
        yield return ("RestoreObject", null);
        yield return ("SetObjectAcl", null);
        yield return ("UploadPart", null);
    }

    private static string ClassField(string inner)
    {
        return $$"""
               // <auto-generated/>
               
               using OBS;
               using OBS.Model;
               
               namespace OBS.Extensions;
               
               public static partial class AsyncExtension
               {
               {{inner}}
               }
               """;
    }

    private const string OBS                  = "global::OBS";
    private const string Task                 = "global::System.Threading.Tasks.Task";
    private const string TaskCompletionSource = "global::System.Threading.Tasks.TaskCompletionSource";
    private const string CancellationToken    = "global::System.Threading.CancellationToken";
    private const string ObsClient            = $"{OBS}.ObsClient";
    private const string Model                = $"{OBS}.Model";
    
    private static string GenerateMethod(string name, string? response)
    {
        var arg = $"{Model}.{name}Request";
        var ret = response ?? $"{name}Response";
        return $$"""
                     public static {{Task}}<{{ret}}> {{name}}Async(this {{ObsClient}} client,
                        {{arg}} request,
                        object state = default,
                        {{CancellationToken}}? token = null)
                    {       
                        var source = new {{TaskCompletionSource}}<{{ret}}>(state);
                        var ar     = client.Begin{{name}}(request, ar => { source.SetResult(client.End{{name}}(ar)); }, state);
                        token?.Register(() =>
                        {
                            client.End{{name}}(ar);
                            source.SetCanceled();
                        });
                                  
                        return source.Task;
                    }
                 
                 
                 """;
    }
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        
        var provider = context.SyntaxProvider
            .CreateSyntaxProvider(
                (s, _) => s is IdentifierNameSyntax { Identifier.Text: "ObsClient" },
                (ctx, _) => ctx.Node);

        context.RegisterSourceOutput(provider.Collect(), (ctx, t) =>
        {
            ctx.AddSource(
                "OBS.Extensions.AsyncExtension.g.cs",
                SourceText.From(
                    ClassField(
                        ExportedAsyncMethods()
                            .Select(tuple => GenerateMethod(tuple.Item1, tuple.Item2))
                            .Aggregate(new StringBuilder(), (s, c) => s.Append(c))
                            .ToString()
                    ), Encoding.UTF8));
        });

    }
}