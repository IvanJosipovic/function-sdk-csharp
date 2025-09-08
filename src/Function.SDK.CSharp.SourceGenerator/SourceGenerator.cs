using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using KubernetesCRDModelGen;
using Microsoft.CodeAnalysis;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.System.Text.Json;

namespace Function.SDK.CSharp.SourceGenerator;

[Generator]
public class SourceGenerator : IIncrementalGenerator
{
    private static CodeGenerator codeGenerator;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
#if DEBUG
        if (!Debugger.IsAttached)
        {
            //Debugger.Launch();
        }
#endif
        codeGenerator = new CodeGenerator();

        var pipeline = context.AdditionalTextsProvider.Select(static (text, cancellationToken) =>
        {
            var fileName = Path.GetFileName(text.Path);

            if (!fileName.Equals("xrd.yaml"))
            {
                return default;
            }

            return (Name: fileName, Text: text.GetText(cancellationToken)?.ToString());
        })
        .Where((pair) => pair is not ((_, null) or (null, _)));

        context.RegisterSourceOutput(pipeline, static (context, pair) =>
        {
            try
            {
                // Log the filename that is loaded
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "KG0",
                        "File loaded",
                        "Loaded file: {0}",
                        "Function.SDK.CSharp.SourceGenerator",
                        DiagnosticSeverity.Info,
                        true),
                    Location.None,
                    pair.Name));

                TextReader reader = new StringReader(pair.Text);

                var openAPIReader = new OpenApiJsonReader();

                var deserializer = new DeserializerBuilder()
                    .WithTypeConverter(new SystemTextJsonYamlTypeConverter())
                    .WithTypeInspector(x => new SystemTextJsonTypeInspector(x))
                    .IgnoreUnmatchedProperties()
                    .Build();

                var parser = new MergingParser(new Parser(reader));

                parser.Consume<StreamStart>();

                while (parser.Accept<DocumentStart>(out var start))
                {
                    var crd = deserializer.Deserialize<V2CompositeResourceDefinition>(parser);

                    var key = $"{crd.ApiVersion}/{crd.Kind}";

                    if (key == $"{V2CompositeResourceDefinition.KubeGroup}/{V2CompositeResourceDefinition.KubeApiVersion}/{V2CompositeResourceDefinition.KubeKind}")
                    {
                        try
                        {
                            var versions = crd.Spec.Versions.Where(x => x.Referenceable);

                            var crossplaneProperties = """
                                {
                                  "crossplane": {
                                    "description": "Configures how Crossplane will reconcile this composite resource",
                                    "properties": {
                                      "compositionRef": {
                                        "properties": {
                                          "name": {
                                            "type": "string"
                                          }
                                        },
                                        "required": [
                                          "name"
                                        ],
                                        "type": "object"
                                      },
                                      "compositionRevisionRef": {
                                        "properties": {
                                          "name": {
                                            "type": "string"
                                          }
                                        },
                                        "required": [
                                          "name"
                                        ],
                                        "type": "object"
                                      },
                                      "compositionRevisionSelector": {
                                        "properties": {
                                          "matchLabels": {
                                            "additionalProperties": {
                                              "type": "string"
                                            },
                                            "type": "object"
                                          }
                                        },
                                        "required": [
                                          "matchLabels"
                                        ],
                                        "type": "object"
                                      },
                                      "compositionSelector": {
                                        "properties": {
                                          "matchLabels": {
                                            "additionalProperties": {
                                              "type": "string"
                                            },
                                            "type": "object"
                                          }
                                        },
                                        "required": [
                                          "matchLabels"
                                        ],
                                        "type": "object"
                                      },
                                      "compositionUpdatePolicy": {
                                        "default": "Automatic",
                                        "enum": [
                                          "Automatic",
                                          "Manual"
                                        ],
                                        "type": "string"
                                      },
                                      "resourceRefs": {
                                        "items": {
                                          "properties": {
                                            "apiVersion": {
                                              "type": "string"
                                            },
                                            "kind": {
                                              "type": "string"
                                            },
                                            "name": {
                                              "type": "string"
                                            }
                                          },
                                          "required": [
                                            "apiVersion",
                                            "kind"
                                          ],
                                          "type": "object"
                                        },
                                        "type": "array",
                                        "x-kubernetes-list-type": "atomic"
                                      }
                                    },
                                    "type": "object"
                                  }
                                }
                                """;

                            var statusProperties = """
                                {
                                  "status": {
                                    "properties": {
                                      "conditions": {
                                        "description": "Conditions of the resource.",
                                        "items": {
                                          "properties": {
                                            "lastTransitionTime": {
                                              "format": "date-time",
                                              "type": "string"
                                            },
                                            "message": {
                                              "type": "string"
                                            },
                                            "observedGeneration": {
                                              "format": "int64",
                                              "type": "integer"
                                            },
                                            "reason": {
                                              "type": "string"
                                            },
                                            "status": {
                                              "type": "string"
                                            },
                                            "type": {
                                              "type": "string"
                                            }
                                          },
                                          "required": [
                                            "lastTransitionTime",
                                            "reason",
                                            "status",
                                            "type"
                                          ],
                                          "type": "object"
                                        },
                                        "type": "array",
                                        "x-kubernetes-list-map-keys": [
                                          "type"
                                        ],
                                        "x-kubernetes-list-type": "map"
                                      }
                                    },
                                    "type": "object"
                                  }
                                }
                                """;

                            foreach (var version in versions)
                            {
                                var schema = version.Schema.OpenAPIV3Schema;

                                // Append crossplane and status properties to the Composite Resource Model
                                schema["properties"]["spec"]["properties"]["crossplane"] = JsonNode.Parse(crossplaneProperties)["crossplane"].DeepClone();

                                if (schema["properties"]["status"] == null)
                                {
                                    schema["properties"]["status"] = JsonNode.Parse($$"""
                                    {
                                        "description": "Status defines the observed state of the {{crd.Kind}}.",
                                        "type": "object",
                                        "properties": {}
                                    }
                                    """);
                                }

                                if (schema["properties"]["status"]["properties"] == null)
                                {
                                    schema["properties"]["status"]["properties"] = JsonNode.Parse("{}");
                                }

                                schema["properties"]["status"]["properties"]["conditions"] = JsonNode.Parse(statusProperties)["status"]["properties"]["conditions"].DeepClone();

                                var doc = openAPIReader.ReadFragment<OpenApiSchema>(schema, OpenApiSpecVersion.OpenApi3_0, new OpenApiDocument(), out var diag);

                                if (diag != null && diag.Errors.Count > 0)
                                {
                                    context.ReportDiagnostic(Diagnostic.Create(
                                        new DiagnosticDescriptor(
                                            "KG3",
                                            "Error Parsin Open API Spec",
                                            "Loaded file: {0}\r\n{1}",
                                            "Function.SDK.CSharp.SourceGenerator",
                                            DiagnosticSeverity.Error,
                                            true),
                                        Location.None,
                                        pair.Name, diag.Errors.Select(x => x.Message).Aggregate((a, b) => a + "\r\n" + b)));

                                }

                                var code = codeGenerator.GenerateCompilationUnit(doc, "Function.SDK.CSharp.SourceGenerator.Models", version.Name, crd.Spec.Names.Kind, crd.Spec.Group, crd.Spec.Names.Plural, crd.Spec.Names.ListKind);

                                var filename = CodeGenerator.RemoveIllegalFileNameCharacters($"{version.Name}.{crd.Metadata.Name}.g.cs");

                                context.AddSource(filename, code.NormalizeWhitespace().ToFullString());
                            }
                        }
                        catch (Exception e)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                new DiagnosticDescriptor(
                                "KG2",
                                $"Error converting {pair.Name} {key}",
                                "{0}\n{1}",
                                "Function.SDK.CSharp.SourceGenerator",
                                DiagnosticSeverity.Error,
                                true), Location.None, $"Error converting {pair.Name} {key} {e.Message}", e.StackTrace));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                    "KG1",
                    $"Error parsing {pair.Name}",
                    "{0}\n{1}",
                    "Function.SDK.CSharp.SourceGenerator",
                    DiagnosticSeverity.Error,
                    true), Location.None, e, e.StackTrace));
            }
        });
    }
}
