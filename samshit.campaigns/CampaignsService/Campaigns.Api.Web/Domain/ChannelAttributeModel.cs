using System;
using System.ComponentModel;

namespace Campaigns.Api.Web.Domain
{
    public class ChannelAttributeModel
    {
        const string UNDEFINED_TYPE = "Undefined";
        const string STRING_TYPE = "String";
        const string INT32_TYPE = "Int32";
        const string INT64_TYPE = "Int64";
        const string DATETIME_TYPE = "DateTime";
        const string BOOL_TYPE = "Boolean";
        public int Id { get; set; }
        public int ChannelId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }

        public static bool TryParse(ChannelAttributeModel model, out object res)
        {
            res = default;
            try
            {
                TypeConverter tc = default;
                dynamic typeInstance;
                if (model.Type.Contains(STRING_TYPE, StringComparison.InvariantCultureIgnoreCase))
                {
                    res = model.Value;
                    return true;
                }
                if (model.Type.Contains(INT32_TYPE, StringComparison.InvariantCultureIgnoreCase))
                {
                    typeInstance = Activator.CreateInstance(typeof(int));
                    if (typeInstance != null)
                    {
                        tc = TypeDescriptor.GetConverter(typeInstance);
                        res = tc.ConvertTo(model.Value, typeof(int));
                        return true;
                    }
                }
                if (model.Type.Contains(INT64_TYPE, StringComparison.InvariantCultureIgnoreCase))
                {
                    typeInstance = Activator.CreateInstance(typeof(long));
                    if (typeInstance != null)
                    {
                        tc = TypeDescriptor.GetConverter(typeInstance);
                        res = tc.ConvertTo(model.Value, typeof(long));
                        return true;
                    }
                }
                if (model.Type.Contains(DATETIME_TYPE, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (!string.IsNullOrEmpty(model.Value))
                    {
                        var parseResult = DateTime.TryParse(model.Value, out var parsedDateTime);
                        if (parseResult)
                        {
                            res = parsedDateTime;
                            return true;
                        }
                    }
                }
                if (model.Type.Contains(BOOL_TYPE, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (!string.IsNullOrEmpty(model.Value))
                    {
                        if (model.Value == Boolean.TrueString || model.Value == Boolean.FalseString)
                        {
                            res = model.Value == Boolean.TrueString;
                            return true;
                        }
                    }
                }

                return false;
            }
            catch (Exception _)
            {
                return false;
            }
        }

        public static ChannelAttributeModel Create(int channelId, string name, object value)
        {
            var typeName = value.GetType().FullName;
            var attribute = new ChannelAttributeModel
            {
                ChannelId = channelId,
                Name = name,
                Type = typeName ?? UNDEFINED_TYPE,
                Value = value.ToString()
            };
            return attribute;
        }
    }
}