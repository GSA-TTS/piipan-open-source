using Piipan.Notifications.Core.Extensions;
using Xunit;

namespace Piipan.Notifications.Core.Tests.Extensions
{
    public class UsdaImageRetrieverTest
    {
        [Fact]
        public void RetrieveUsdaSymbolColorImage_RetrievesImagePath()
        {
            UsdaImageRetriever retriever = new UsdaImageRetriever();
            string imagePath = retriever.RetrieveUsdaSymbolColorImagePath();
            Assert.NotNull(imagePath);
            FileInfo fileInfo = new FileInfo(imagePath);
            Assert.True(fileInfo.FullName.EndsWith("\\images\\usda-symbol-color.png"));
        }
    }
}
