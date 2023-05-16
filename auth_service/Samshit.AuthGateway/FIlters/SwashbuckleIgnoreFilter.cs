using System.Linq;
using Microsoft.OpenApi.Models;
using Samshit.DataModels.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Samshit.AuthGateway.FIlters
{
    public class SwashbuckleIgnoreFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema.Properties == null || schema.Properties.Count == 0)
                return;

            foreach (var prop in context.Type
                .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                var customSwaggerIgnoreAttrs = prop.GetCustomAttributes(typeof(SwaggerIgnoreAttribute), true).ToArray();
                foreach (var attr in customSwaggerIgnoreAttrs)
                {
                    if (attr is SwaggerIgnoreAttribute)
                    {
                        var camelCasePropName = ToCamelCase(prop.Name);
                        if (schema.Properties.ContainsKey(camelCasePropName))
                        {
                            schema.Properties?.Remove(camelCasePropName);
                        }
                    }
                }
            }
        }

        private static string ToCamelCase(string source)
        {
            if (source.Length > 0)
            {
                var ending = source.Substring(1);
                var firstLetter = source[0].ToString().ToLower();
                return $"{firstLetter}{ending}";
            }

            return source;
        }
    }
}
