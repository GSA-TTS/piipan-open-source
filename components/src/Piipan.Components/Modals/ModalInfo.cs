using Microsoft.AspNetCore.Components;

namespace Piipan.Components.Modals
{
    /// <summary>
    /// All necessary information to show the modal. Currently this only contains a RenderFragment,
    /// but more properties could be used in the future.
    /// </summary>
    public class ModalInfo
    {
        public RenderFragment RenderFragment { get; set; }
        public string ModalId { get; set; }
        public bool ForceAction { get; set; }
    }
}
