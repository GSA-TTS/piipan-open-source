namespace Piipan.Notifications.Core.Extensions
{
    public interface IUsImageRetriever
    {
        string RetrieveUsSymbolColorImagePath();

    }

    /// <summary>
    /// Utility class to retrieve Symbol Image file. 
    /// This image resides in a "images" directory which is a sibling
    /// directory to the "bin" directory where the assembly resides
    /// </summary>
    public class UsImageRetriever : IUsImageRetriever
    {
        /// <summary>
        /// Retrieves the image path of 18f-symbol.color.png file
        /// </summary>
        /// <returns>File path of the 18f-symbol-color.png file</returns>
        public string RetrieveUsSymbolColorImagePath()
        {
            var assemblyFilePath = typeof(UsImageRetriever).Assembly.Location;

            string? assemblyDirectory = new FileInfo(assemblyFilePath).DirectoryName;

            //gets the image file from the "images" directory which is a sibling directory to the assembly's
            return $"{assemblyDirectory}\\..\\images\\18f-symbol-color.png";
        }
    }
}
