﻿namespace AvroConvertTests
{
    using System.Collections.Generic;
    using AvroConvert;
    using Xunit;

    public class DeserializeTests
    {
        private readonly byte[] _avroBytes;

        public DeserializeTests()
        {
            _avroBytes = System.IO.File.ReadAllBytes("example2.avro");
        }

        [Fact]
        public void Deserialize_ValidBytes_SetOfPropertiesAreReturned()
        {
            //Arrange
            Dictionary<string, object> result1 = new Dictionary<string, object>();
            result1.Add("name", "Alyssa");
            result1.Add("favorite_number", 256);
            result1.Add("favorite_color", null);

            Dictionary<string, object> result2 = new Dictionary<string, object>();
            result2.Add("name", "Ben");
            result2.Add("favorite_number", 7);
            result2.Add("favorite_color", "red");

            List<object> expectedResult = new List<object>();
            expectedResult.Add(result1);
            expectedResult.Add(result2);


            //Act
            var result = AvroConvert.Deserialize(_avroBytes);


            //Assert
            Assert.Equal(expectedResult, result);
        }

    }
}
