using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

namespace Piipan.Components.Modals
{
    public interface IModalManager
    {
        public Action ModalsUpdated { get; set; }
        public IReadOnlyCollection<ModalInfo> OpenModals { get; }
        public void Show<T>(T modal, ModalInfo modalInfo = null) where T : IComponent;

        public void Close(ModalInfo modalInfo);
    }
}
