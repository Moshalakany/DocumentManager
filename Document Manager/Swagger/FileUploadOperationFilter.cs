using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Document_Manager.Swagger
{
    public class FileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var actionParameters = context.MethodInfo.GetParameters();
            
            var formFileParams = actionParameters
                .Where(p => p.ParameterType == typeof(IFormFile) || p.ParameterType == typeof(IFormFileCollection))
                .ToList();

            var formFileListParams = actionParameters
                .Where(p => p.ParameterType.IsGenericType && p.ParameterType.GetGenericArguments().SingleOrDefault() == typeof(IFormFile))
                .ToList();

            if (formFileParams.Count == 0 && formFileListParams.Count == 0)
                return;

            // Clear default parameters that would be generated
            var fileParamNames = formFileParams
                .Union(formFileListParams)
                .Select(p => p.Name)
                .ToList();

            var parametersToRemove = operation.Parameters
                .Where(p => fileParamNames.Contains(p.Name))
                .ToList();

            foreach (var param in parametersToRemove)
                operation.Parameters.Remove(param);

            // Add form file handling metadata
            operation.RequestBody = new OpenApiRequestBody
            {
                Content = 
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = new Dictionary<string, OpenApiSchema>(),
                            Required = new HashSet<string>()
                        }
                    }
                }
            };

            var properties = operation.RequestBody.Content["multipart/form-data"].Schema.Properties;

            // Handle standard IFormFile params
            foreach (var param in formFileParams)
            {
                var required = context.ApiDescription.ParameterDescriptions
                    .First(p => p.Name == param.Name)
                    .IsRequired;
                
                properties.Add(param.Name, new OpenApiSchema
                {
                    Type = "string",
                    Format = "binary",
                    Description = GetParameterDescription(param)
                });

                if (required)
                    operation.RequestBody.Content["multipart/form-data"].Schema.Required.Add(param.Name);
            }

            // Handle collections of IFormFile
            foreach (var param in formFileListParams)
            {
                var required = context.ApiDescription.ParameterDescriptions
                    .First(p => p.Name == param.Name)
                    .IsRequired;
                
                properties.Add(param.Name, new OpenApiSchema
                {
                    Type = "array",
                    Items = new OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary"
                    },
                    Description = GetParameterDescription(param)
                });

                if (required)
                    operation.RequestBody.Content["multipart/form-data"].Schema.Required.Add(param.Name);
            }

            // Handle additional form fields
            var formBoundProperties = context.MethodInfo.GetCustomAttributes<Microsoft.AspNetCore.Mvc.ConsumesAttribute>()
                ?.Where(a => a.ContentTypes.Any(ct => ct.Equals("multipart/form-data", StringComparison.OrdinalIgnoreCase)))
                ?.SelectMany(a => a.ContentTypes);

            if (formBoundProperties != null && formBoundProperties.Any())
            {
                // Find non-file parameters
                var otherParams = actionParameters
                    .Where(p => p.ParameterType != typeof(IFormFile) && 
                               !p.ParameterType.IsGenericType && 
                               p.ParameterType.GetGenericArguments().SingleOrDefault() != typeof(IFormFile))
                    .ToList();

                foreach (var param in otherParams)
                {
                    var required = context.ApiDescription.ParameterDescriptions
                        .First(p => p.Name == param.Name)
                        .IsRequired;
                    
                    properties.Add(param.Name, new OpenApiSchema
                    {
                        Type = "string",
                        Description = GetParameterDescription(param)
                    });

                    if (required)
                        operation.RequestBody.Content["multipart/form-data"].Schema.Required.Add(param.Name);
                }
            }
        }

        private string GetParameterDescription(ParameterInfo parameter)
        {
            return parameter.GetCustomAttributes<System.ComponentModel.DescriptionAttribute>()
                .Select(a => a.Description)
                .FirstOrDefault() ?? parameter.Name;
        }
    }
}
