using System;
using System.Dynamic;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Samshit.AuthGateway.Controllers
{
    [ApiController]
    [Route("/auth/doc")]
    public class DocController : Controller
    {
        private const string CONTENT_DIR = "/Content";
        private IWebHostEnvironment _env;

        public DocController(IWebHostEnvironment env)
        {
            _env = env;
        }

        public IActionResult Get()
        {
            try
            {
                var path = $"{_env.ContentRootPath}{CONTENT_DIR}/api.json";
                var docFile = System.IO.File.ReadAllBytes(path);
                return new FileContentResult(docFile, "application/json");
            }
            catch (Exception)
            {
                return NotFound("No doc file found in content dir.");
            }
        }

        [HttpGet("yaml")]
        public IActionResult GetYaml()
        {
            try
            {
                string yaml = String.Empty;
                var path = $"{_env.ContentRootPath}{CONTENT_DIR}/api.json";
                var jsonFile = System.IO.File.ReadAllText(path);
                if (!string.IsNullOrEmpty(jsonFile))
                {
                    var expConverter = new ExpandoObjectConverter();
                    var jsonDocument = JsonConvert.DeserializeObject<ExpandoObject>(jsonFile, expConverter);
                    if (jsonDocument != null)
                    {
                        var serializer = new YamlDotNet.Serialization.Serializer();
                        yaml = serializer.Serialize(jsonDocument);
                    }
                }

                if (!string.IsNullOrEmpty(yaml))
                {
                    var yamlData = Encoding.UTF8.GetBytes(yaml);
                    return new FileContentResult(yamlData, "text/plain");
                }

                return NotFound();
            }
            catch (Exception)
            {
                return NotFound("No doc file found in content dir.");
            }
        }
    }
}