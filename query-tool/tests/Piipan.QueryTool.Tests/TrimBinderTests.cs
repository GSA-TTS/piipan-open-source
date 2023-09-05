using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using Moq;
using Piipan.QueryTool.Binders;
using Xunit;

namespace Piipan.QueryTool.Tests
{
    public class TrimBinderTests
    {
        [Theory]
        [InlineData(" A ")]
        [InlineData("\u00A0A")]
        public async Task TrimBinderTrimsStrings(string incoming)
        {
            // Arrange
            var binder = new TrimModelBinder();
            var formCollection = new FormCollection(
                new Dictionary<string, StringValues>()
                {
                    { "someString", new StringValues(incoming) }
                });
            var vp = new FormValueProvider(BindingSource.Form, formCollection, CultureInfo.CurrentCulture);
            var metadataProvider = new EmptyModelMetadataProvider();
            var bindingContext = new DefaultModelBindingContext
            {
                ModelMetadata = metadataProvider.GetMetadataForType(Type.GetType("System.String")),
                ModelName = "someString",
                ModelState = new ModelStateDictionary(),
                ValueProvider = vp,
            };

            // Act
            await binder.BindModelAsync(bindingContext);
            var resultString = bindingContext.Result.Model as string;

            // Assert
            Assert.Equal(incoming.Trim(), resultString);
        }

        [Fact]
        public async Task ThrowsOnNullContext()
        {
            // Arrange
            var binder = new TrimModelBinder();

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => binder.BindModelAsync(null));
        }

        [Fact]
        public async Task AddsErrorOnNonStringType()
        {
            // Arrange
            var binder = new TrimModelBinder();

            var valueProvider = new Mock<IValueProvider>();
            valueProvider
                .Setup(m => m.GetValue(It.IsAny<string>()))
                .Returns(new ValueProviderResult("something"));

            var modelState = new Mock<ModelStateDictionary>();

            var context = new Mock<ModelBindingContext>();
            context.Setup(m => m.ModelName).Returns("modelName");
            context
                .Setup(m => m.ModelMetadata)
                .Returns((new EmptyModelMetadataProvider()).GetMetadataForType(typeof(int)));
            context.Setup(m => m.ValueProvider).Returns(valueProvider.Object);
            context.Setup(m => m.ModelState).Returns(modelState.Object);

            // Act
            await binder.BindModelAsync(context.Object);            

            // Assert
            Assert.Equal(1, modelState.Object.ErrorCount);
        }

        [Fact]
        public async Task AddsErrorOnNullNonNullableType()
        {
            // Arrange
            var binder = new TrimModelBinder();

            var valueProvider = new Mock<IValueProvider>();
            valueProvider
                .Setup(m => m.GetValue(It.IsAny<string>()))
                .Returns(new ValueProviderResult(""));

            var modelState = new Mock<ModelStateDictionary>();

            var context = new Mock<ModelBindingContext>();
            context.Setup(m => m.ModelName).Returns("modelName");
            context
                .Setup(m => m.ModelMetadata)
                .Returns((new EmptyModelMetadataProvider()).GetMetadataForType(typeof(int)));
            context.Setup(m => m.ValueProvider).Returns(valueProvider.Object);
            context.Setup(m => m.ModelState).Returns(modelState.Object);

            // Act
            await binder.BindModelAsync(context.Object);            

            // Assert
            Assert.Equal(1, modelState.Object.ErrorCount);
        }

        [Fact]
        public async Task NoOpOnEmptyValue()
        {
            // Arrange
            var binder = new TrimModelBinder();

            var valueProvider = new Mock<IValueProvider>();
            valueProvider
                .Setup(m => m.GetValue(It.IsAny<string>()))
                .Returns(ValueProviderResult.None);

            var context = new Mock<ModelBindingContext>();
            context
                .Setup(m => m.ValueProvider)
                .Returns(valueProvider.Object);

            // Act / Assert
            await binder.BindModelAsync(context.Object);

            // Assert
            context.VerifySet(m => m.Result = It.IsAny<ModelBindingResult>(), Times.Never);
        }

        [Theory]
        [InlineData("")]
        [InlineData("    ")]
        public async Task SuccessOnWhitespaceModel(string modelValue)
        {
            // Arrange
            var binder = new TrimModelBinder();

            var valueProvider = new Mock<IValueProvider>();
            valueProvider
                .Setup(m => m.GetValue(It.IsAny<string>()))
                .Returns(new ValueProviderResult(modelValue));

            var context = new Mock<ModelBindingContext>();
            context.Setup(m => m.ModelName).Returns("modelName");
            context
                .Setup(m => m.ModelMetadata)
                .Returns((new EmptyModelMetadataProvider()).GetMetadataForType(typeof(string)));
            context.Setup(m => m.ValueProvider).Returns(valueProvider.Object);
            context.Setup(m => m.ModelState).Returns(new ModelStateDictionary());

            // Act / Assert
            await binder.BindModelAsync(context.Object);

            // Assert
            context.VerifySet(m => m.Result = It.Is<ModelBindingResult>(r => 
                r.Model == null
            ), Times.Once);
        }
    }

    public class TrimModelBinderProviderTests
    {
        [Fact]
        public void GetBinder_ThrowsOnNullContext()
        {
            // Arrange
            var provider = new TrimModelBinderProvider();

            // Act / Assert
            Assert.Throws<ArgumentNullException>(() => provider.GetBinder(null));
        }

        [Fact]
        public void GetBinder_SuccessOnStringType()
        {
            // Arrange
            var provider = new TrimModelBinderProvider();
            var context = new Mock<ModelBinderProviderContext>();
            context
                .Setup(m => m.Metadata)
                .Returns((new EmptyModelMetadataProvider()).GetMetadataForType(typeof(String)));

            // Act
            var binder = provider.GetBinder(context.Object);

            // Assert
            Assert.NotNull(binder);
            Assert.IsType<TrimModelBinder>(binder);
        }

        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(object))]
        public void GetBinder_NullOnNonStringType(Type t)
        {
            // Arrange
            var provider = new TrimModelBinderProvider();
            var context = new Mock<ModelBinderProviderContext>();
            context
                .Setup(m => m.Metadata)
                .Returns((new EmptyModelMetadataProvider()).GetMetadataForType(t));

            // Act
            var binder = provider.GetBinder(context.Object);

            // Assert
            Assert.Null(binder);
        }
    }
}
