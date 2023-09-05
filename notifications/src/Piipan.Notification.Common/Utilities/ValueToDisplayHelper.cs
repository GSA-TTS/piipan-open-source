namespace Piipan.Notification.Common.Utilities
{
    public static class ValueToDisplayHelper
    {
        public const string DateFormat = "MM/dd/yyyy";
        public static string? GetDisplayValue<T>(T value)
        {
            if (value == null)
            {
                return "-";
            }
            return value switch
            {
                bool b => b ? "Yes" : "No",
                DateTime d => d.ToString(DateFormat),
                _ => value.ToString()
            };
        }
    }
}
