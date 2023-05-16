using System;
using System.Collections;
using System.Collections.Generic;
using Campaigns.Api.Web.Domain;
using Xunit;

namespace Campaigns.Api.Tests
{
    public class TestBadDataGenerator : IEnumerable<object[]>
    {
        public static IEnumerable<object[]> GetBadItemsFromDataGenerator()
        {
            yield return new object[]
            {
                new ChannelAttributeModel() {ChannelId = 1, Id = 1, Name = "TestParamater_BadInt32", Value = "12.3", Type = "System.Int32"},
                new ChannelAttributeModel() {ChannelId = 1, Id = 1, Name = "TestParamater_BadBool", Value = "fOOlse", Type = "System.Boolean"},
                new ChannelAttributeModel() {ChannelId = 1, Id = 1, Name = "TestParamater_BadInt32Overflowed", Value = (Int64.MaxValue-10).ToString(), Type = "System.Int32"}
            };
        }

        public IEnumerator<object[]> GetEnumerator()
        {
            return GetBadItemsFromDataGenerator().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class TestDataGenerator : IEnumerable<object[]>
    {
        public static IEnumerable<object[]> GetItemsFromDataGenerator()
        {
            yield return new object[]
            {
                new ChannelAttributeModel()
                {
                    ChannelId = 1, Id = 1, Name = "TestParamater_String", Value = "Test", Type = "System.String"
                },
                new ChannelAttributeModel()
                {
                    ChannelId = 1, Id = 2, Name = "TestParamater_Int32", Value = (Int32.MinValue + 10).ToString(), Type = "System.Int32"
                },
                new ChannelAttributeModel()
                {
                    ChannelId = 1, Id = 3, Name = "TestParamater_Int64", Value = (Int64.MinValue + 10).ToString(), Type = "System.Int64"
                },
                new ChannelAttributeModel()
                {
                    ChannelId = 1, Id = 3, Name = "TestParamater_Boolean", Value = (false).ToString(), Type = "System.Boolean"
                },
                new ChannelAttributeModel()
                {
                    ChannelId = 1, Id = 3, Name = "TestParamater_DateTime", Value = (DateTime.Now).ToString(), Type = "System.DateTime"
                }
            };
        }

        public IEnumerator<object[]> GetEnumerator()
        {
            return GetItemsFromDataGenerator().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class ChannelAttributeModelTest
    {
        private ChannelAttributeModel _model;

        [Fact]
        public void NotParseUnsupportedTypesTest()
        {
            var model = new ChannelAttributeModel() { ChannelId = 1, Id = 1, Name = "TestParamater_Decimal", Value = "12.3", Type = "System.Decimal" };
            bool hasParsed;
            object parsedValue;

            hasParsed = ChannelAttributeModel.TryParse(model, out parsedValue);
            {
                Assert.False(hasParsed, "Should not parse, but it did o_____0 ");
                Assert.Null(parsedValue);
            }
        }

        [Theory]
        [ClassData(typeof(TestBadDataGenerator))]
        public void NotParseSupportedTypesBadValuesTest(
            ChannelAttributeModel badInt32,
            ChannelAttributeModel badBool,
            ChannelAttributeModel badInt32Overflowed
            )
        {
            bool hasParsed;
            object parsedValue;

            hasParsed = ChannelAttributeModel.TryParse(badInt32, out parsedValue);
            {
                Assert.False(hasParsed, "Should not parse Int32, but it did o_____0 ");
                Assert.Null(parsedValue);
            }
            hasParsed = ChannelAttributeModel.TryParse(badBool, out parsedValue);
            {
                Assert.False(hasParsed, "Should not parse Boolean, but it did o_____0 ");
                Assert.Null(parsedValue);
            }
            hasParsed = ChannelAttributeModel.TryParse(badInt32Overflowed, out parsedValue);
            {
                Assert.False(hasParsed, "Should not parse overflowed Int32, but it did o_____0 ");
                Assert.Null(parsedValue);
            }
        }

        [Theory]
        [ClassData(typeof(TestDataGenerator))]
        public void ParseSupportedTypesTest(
            ChannelAttributeModel stringModel,
            ChannelAttributeModel int32Model,
            ChannelAttributeModel int64Model,
            ChannelAttributeModel boolModel,
            ChannelAttributeModel dtModel
            )
        {
            bool hasParsed;
            object parsedValue;

            hasParsed = ChannelAttributeModel.TryParse(stringModel, out parsedValue);
            {
                Assert.True(hasParsed, "Failed to parse value of type: String");
                Assert.IsType<string>(parsedValue);
            }
            hasParsed = ChannelAttributeModel.TryParse(int32Model, out parsedValue);
            {
                Assert.True(hasParsed, "Failed to parse value of type: Int32");
                Assert.IsType<Int32>(parsedValue);
            }
            hasParsed = ChannelAttributeModel.TryParse(int64Model, out parsedValue);
            {
                Assert.True(hasParsed, "Failed to parse value of type: Int64");
                Assert.IsType<Int64>(parsedValue);
            }
            hasParsed = ChannelAttributeModel.TryParse(boolModel, out parsedValue);
            {
                Assert.True(hasParsed, "Failed to parse value of type: Boolean");
                Assert.IsType<Boolean>(parsedValue);
            }
            hasParsed = ChannelAttributeModel.TryParse(dtModel, out parsedValue);
            {
                Assert.True(hasParsed, "Failed to parse value of type: DateTime");
                Assert.IsType<DateTime>(parsedValue);
            }
        }

        [Fact]
        public void CreateAttributeTest()
        {
            var testTypeName = "System.Int32";
            var testName = "test";
            var testValue = 11;
            var testChannelId = 1;
            _model = ChannelAttributeModel.Create(testChannelId, testName, testValue);
            Assert.NotNull(_model);
            Assert.Equal(testTypeName, _model.Type);
            Assert.Equal(testValue.ToString(), _model.Value);
            Assert.Equal(testChannelId, _model.ChannelId);
            Assert.Equal(testName, _model.Name);
        }
    }
}
