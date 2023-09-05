namespace Piipan.Notification.Common
{
    public interface IViewRenderService
    {
        Task<string> GenerateMessageContent<TModel>(string name, TModel model);
    }
}
