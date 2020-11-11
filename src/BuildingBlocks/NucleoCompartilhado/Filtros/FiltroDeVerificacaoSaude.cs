using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;

namespace NucleoCompartilhado.Filtros
{
    public sealed class FiltroDeVerificacaoSaude : IDocumentFilter
    {
        public const string VerificacaoSaudeEndpoint = "/api/saude";

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var pathItem = new OpenApiPathItem();

            var operation = new OpenApiOperation();
            operation.Tags.Add(new OpenApiTag { Name = "Saude" });

            var properties = new Dictionary<string, OpenApiSchema>
            {
                { "status", new OpenApiSchema() { Type = "string" } },
                { "errors", new OpenApiSchema() { Type = "datetime" } }
            };

            var response = new OpenApiResponse();
            response.Content.Add("application/json", new OpenApiMediaType
            {
                Schema = new OpenApiSchema
                {
                    Type = "object",
                    AdditionalPropertiesAllowed = true,
                    Properties = properties,
                }
            });

            operation.Responses.Add("200", response);
            pathItem.AddOperation(OperationType.Get, operation);
            swaggerDoc?.Paths.Add(VerificacaoSaudeEndpoint, pathItem);
        }
    }
}