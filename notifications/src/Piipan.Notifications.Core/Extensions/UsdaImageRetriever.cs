namespace Piipan.Notifications.Core.Extensions
{
    public interface IUsdaImageRetriever
    {
        string RetrieveUsdaSymbolColorImagePath();

    }

    /// <summary>
    /// Utility class to retrieve USDA Symbol Image file. 
    /// This image resides in a "images" directory which is a sibling
    /// directory to the "bin" directory where the assembly resides
    /// </summary>
    public class UsdaImageRetriever : IUsdaImageRetriever
    {
        /// <summary>
        /// Retrieves the image path of usda-symbol.color.png file
        /// </summary>
        /// <returns>File path of the usda-symbol-color.png file</returns>
        public string RetrieveUsdaSymbolColorImagePath()
        {
            var assemblyFilePath = typeof(UsdaImageRetriever).Assembly.Location;

            string? assemblyDirectory = new FileInfo(assemblyFilePath).DirectoryName;

            //gets the image file from the "images" directory which is a sibling directory to the assembly's
            return $"{assemblyDirectory}\\..\\images\\usda-symbol-color.png";
        }
    }
}
