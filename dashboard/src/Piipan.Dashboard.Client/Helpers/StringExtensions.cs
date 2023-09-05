namespace Piipan.Dashboard.Client.Helpers
{
    public static class StringExtensions
    {
        public static string ToLowerExceptFirstLetter(this string inputString)
        {
            if (inputString?.Length > 1)
            {
                return inputString[0..1].ToUpper() + inputString[1..].ToLower();
            }
            else
            {
                return inputString;
            }
        }
    }
}
