using Piipan.Shared.API.Validation;
using Xunit;

namespace Piipan.Shared.Tests.Validation
{
    /// <summary>
    /// Tests associated with the UsaRequiredIf attribute
    /// </summary>
    public class UsaRequiredIfAttributeTests
    {
        public class DummyModel
        {
            public string Prop1 { get; set; }

            [UsaRequiredIf(nameof(Prop1), "", "some error")]
            public string Prop2 { get; set; }

            [UsaRequiredIf(
                nameof(Prop1), "", "some error",
                nameof(Prop2), "", "some other error")]
            public string Prop3 { get; set; }
        }

        /// <summary>
        /// Validate a UsaRequiredIf attribute attached to a dummy model using various parameters
        /// </summary>
        [Theory]
        [InlineData("", "test", false)]
        [InlineData("", "", false)]
        [InlineData("test", "", true)] // Error thrown when property 1 has a value and property 2 does not
        [InlineData("test", "test", false)]
        public void UsaRequiredIf_ValidateTests_OneProperty_RequiredIfNotEmpty(string prop1Value, string prop2Value, bool hasError)
        {
            // Arrange
            var attribute = new UsaRequiredIfAttribute(
                nameof(DummyModel.Prop1), "", "Property is required since Prop1 is not empty");

            var dummyModel = new DummyModel()
            {
                Prop1 = prop1Value,
                Prop2 = prop2Value
            };

            // Act
            var validationResult = attribute.GetValidationResult(prop2Value, new System.ComponentModel.DataAnnotations.ValidationContext(dummyModel));

            // Assert
            Assert.Equal(hasError, !string.IsNullOrEmpty(validationResult?.ErrorMessage));
        }

        /// <summary>
        /// Validate a UsaRequiredIf attribute attached to a dummy model using various parameters against an expected value
        /// </summary>
        [Theory]
        [InlineData("expectedvalue", "test", false)]
        [InlineData("expectedvalue", "", true)] // Error thrown when property 1 has expectedvalue and property 2 is empty
        [InlineData("notexpectedvalue", "", false)] // Error NOT thrown when property 1 has an unexpected value and property 2 is empty
        [InlineData("notexpectedvalue", "test", false)]
        public void UsaRequiredIf_ValidateTests_OneProperty_RequiredIfEqual(string prop1Value, string prop2Value, bool hasError)
        {
            // Arrange
            var attribute = new UsaRequiredIfAttribute(
                nameof(DummyModel.Prop1), "expectedvalue", "Property is required since Prop1 is expectedvalue");

            var dummyModel = new DummyModel()
            {
                Prop1 = prop1Value,
                Prop2 = prop2Value
            };

            // Act
            var validationResult = attribute.GetValidationResult(prop2Value, new System.ComponentModel.DataAnnotations.ValidationContext(dummyModel));

            // Assert
            Assert.Equal(hasError, !string.IsNullOrEmpty(validationResult?.ErrorMessage));
        }

        /// <summary>
        /// Validate a UsaRequiredIf attribute attached to a dummy model using various parameters using multiple properties
        /// </summary>
        [Theory]
        [InlineData("", "", "", false)]
        [InlineData("", "", "test", false)]
        [InlineData("test", "", "", true)] // Error thrown when property 1 has a value and property 3 does not
        [InlineData("", "test", "", true)] // Error thrown when property 2 has a value and property 3 does not
        public void UsaRequiredIf_ValidateTests_MultipleProperties_RequiredIfNotEmpty(string prop1Value, string prop2Value, string prop3Value, bool hasError)
        {
            // Arrange
            var attribute = new UsaRequiredIfAttribute(
                nameof(DummyModel.Prop1), "", "Property is required since Prop1 is not empty",
                nameof(DummyModel.Prop2), "", "Property is required since Prop2 is not empty");

            var dummyModel = new DummyModel()
            {
                Prop1 = prop1Value,
                Prop2 = prop2Value,
                Prop3 = prop3Value
            };

            // Act
            var validationResult = attribute.GetValidationResult(prop3Value, new System.ComponentModel.DataAnnotations.ValidationContext(dummyModel));

            // Assert
            Assert.Equal(hasError, !string.IsNullOrEmpty(validationResult?.ErrorMessage));
        }

        /// <summary>
        /// Validate a UsaRequiredIf attribute attached to a dummy model using various parameters using multiple properties against an expected value
        /// </summary>
        [Theory]
        [InlineData("expectedvalue", "", "test", false)]
        [InlineData("", "expectedvalue", "test", false)]
        [InlineData("expectedvalue", "", "", true)] // Error thrown when property 1 has expectedvalue and property 3 is empty
        [InlineData("", "expectedvalue", "", true)] // Error thrown when property 2 has expectedvalue and property 3 is empty
        [InlineData("notexpectedvalue", "", "", false)] // Error NOT thrown when property 1 has an unexpected value and property 3 is empty
        [InlineData("", "notexpectedvalue", "", false)] // Error NOT thrown when property 2 has an unexpected value and property 3 is empty
        [InlineData("expectedvalue", "expectedvalue", "", true)] // Error thrown when both properties have expectedvalue and property 3 is empty
        [InlineData("notexpectedvalue", "notexpectedvalue", "test", false)]
        public void UsaRequiredIf_ValidateTests_MultipleProperty_RequiredIfEqual(string prop1Value, string prop2Value, string prop3Value, bool hasError)
        {
            // Arrange
            var attribute = new UsaRequiredIfAttribute(
                nameof(DummyModel.Prop1), "expectedvalue", "Property is required since Prop1 is expectedvalue",
                nameof(DummyModel.Prop2), "expectedvalue", "Property is required since Prop2 is expectedvalue");

            var dummyModel = new DummyModel()
            {
                Prop1 = prop1Value,
                Prop2 = prop2Value,
                Prop3 = prop3Value
            };

            // Act
            var validationResult = attribute.GetValidationResult(prop3Value, new System.ComponentModel.DataAnnotations.ValidationContext(dummyModel));

            // Assert
            Assert.Equal(hasError, !string.IsNullOrEmpty(validationResult?.ErrorMessage));
        }

        /// <summary>
        /// Validate a UsaRequiredIf attribute attached to a dummy model using various parameters using multiple properties. One property is checking an expected value,
        /// and the other is checking to see if it has any value.
        /// For this test, Property 3 is required if: Property 1 has the value of "expectedvalue", OR Property 2 has any value.
        /// </summary>
        [Theory]
        [InlineData("expectedvalue", "", "", true)] // Error thrown when property 1 has expectedvalue and property 3 is empty
        [InlineData("", "anyvalue", "", true)] // Error thrown when property 2 has any value and property 3 is empty
        [InlineData("expectedvalue", "anyvalue", "", true)] // Error thrown when Property 1 has the expected value, property 2 has any value, and property 3 is empty
        [InlineData("notexpectedvalue", "", "", false)] // No error necessary, because Property 1 is not the expected value, and Property 2 doesn't have a value.
        [InlineData("expectedvalue", "", "test", false)] // No error because Property 3 has a value
        [InlineData("", "anyvalue", "test", false)] // No error because Property 3 has a value
        [InlineData("expectedvalue", "anyvalue", "test", false)] // No error because Property 3 has a value
        [InlineData("notexpectedvalue", "", "test", false)] // No error because Property 3 has a value, but beyond that it isn't even required to have one
        public void UsaRequiredIf_ValidateTests_MultipleProperty_RequiredIfEqualOrNotEmpty(string prop1Value, string prop2Value, string prop3Value, bool hasError)
        {
            // Arrange
            var attribute = new UsaRequiredIfAttribute(
                nameof(DummyModel.Prop1), "expectedvalue", "Property is required since Prop1 is expectedvalue",
                nameof(DummyModel.Prop2), "", "Property is required since Prop2 is not empty");

            var dummyModel = new DummyModel()
            {
                Prop1 = prop1Value,
                Prop2 = prop2Value,
                Prop3 = prop3Value
            };

            // Act
            var validationResult = attribute.GetValidationResult(prop3Value, new System.ComponentModel.DataAnnotations.ValidationContext(dummyModel));

            // Assert
            Assert.Equal(hasError, !string.IsNullOrEmpty(validationResult?.ErrorMessage));
        }
    }
}
