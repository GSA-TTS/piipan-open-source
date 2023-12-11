using Piipan.Notifications.Core.Extensions;
using Xunit;

namespace Piipan.Notifications.Core.Tests.Extensions
{
    public class UsImageRetrieverTest
    {
        [Fact]
        public void RetrieveUsSymbolColorImage_RetrievesImagePath()
        {
            UsImageRetriever retriever = new UsImageRetriever();
            string imagePath = retriever.RetrieveUsSymbolColorImagePath();
            Assert.NotNull(imagePath);
            FileInfo fileInfo = new FileInfo(imagePath);
            Assert.True(fileInfo.FullName.EndsWith("\\images\\18f-symbol-color.png"));
        }
    }
}
