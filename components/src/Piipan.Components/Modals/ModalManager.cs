using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;

namespace Piipan.Components.Modals
{
    /// <summary>
    /// The manager service that takes care of showing/hiding modals for the application.
    /// </summary>
    public class ModalManager : IModalManager
    {
        public Action ModalsUpdated { get; set; }
        public IReadOnlyCollection<ModalInfo> OpenModals { get; private set; } = Array.Empty<ModalInfo>();

        // Creates the ModalInfo and adds it to our open modals, and then calls ModalsUpdated.
        // This will get picked up by the ModalContainer to render all open modals.
        public void Show<T>(T modal, ModalInfo modalInfo = null) where T : IComponent
        {
            Type modalType = typeof(T);
            var properties = modalType.GetProperties()
                .Where(prop => prop.IsDefined(typeof(ParameterAttribute), false));

            modalInfo ??= new ModalInfo();
            modalInfo.ModalId ??= typeof(T).Name;
            modalInfo.RenderFragment = new RenderFragment(n =>
            {
                n.OpenComponent<T>(1);

                int sequenceNum = 2;
                foreach (var prop in properties)
                {
                    object value = prop.GetValue(modal);
                    n.AddAttribute(sequenceNum++, prop.Name, value);
                }

                n.CloseComponent();
            });

            // Add a new modal by creating a temporary list, adding a modal to it,
            // and then converting it to a read-only list
            var tempModals = OpenModals.ToList();
            tempModals.Add(modalInfo);
            OpenModals = tempModals.AsReadOnly();
            ModalsUpdated?.Invoke();
        }

        // Removes the given modalInfo from our open modals, and then calls ModalsUpdated.
        // This will get picked up by the ModalContainer to render all remaining open modals.
        public void Close(ModalInfo modalInfo)
        {
            // Remove a modal by creating a temporary list, removing a modal from it,
            // and then converting it to a read-only list
            var tempModals = OpenModals.ToList();
            tempModals.Remove(modalInfo);
            OpenModals = tempModals.AsReadOnly();
            ModalsUpdated?.Invoke();
        }
    }
}
