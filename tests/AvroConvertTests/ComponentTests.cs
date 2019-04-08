﻿namespace AvroConvertTests
{
    using Xunit;

    public class ComponentTests
    {
        [Fact]
        public void Serialize_ThenDeserialize_ObjectsAreEqual()
        {
            //Arrange
            User user = new User();
            user.name = "Krzys";
            user.favorite_color = "yellow";
            user.favorite_number = null;

            //Act
            var serialized = AvroConvert.AvroConvert.Serialize(user);

            var deserialized = AvroConvert.AvroConvert.Deserialize<User>(serialized);

            //Assert
            Assert.NotNull(serialized);
            Assert.NotNull(deserialized);
            Assert.Equal(user, deserialized);
        }
    }
}