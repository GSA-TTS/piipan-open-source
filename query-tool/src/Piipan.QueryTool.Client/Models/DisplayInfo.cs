namespace Piipan.QueryTool.Client.Models
{
    public enum DisplayInfoType
    {
        Email,
        Phone,
        None
    }
    public record DisplayInfo(string Label, object Value, DisplayInfoType DisplayType = DisplayInfoType.None);
}
