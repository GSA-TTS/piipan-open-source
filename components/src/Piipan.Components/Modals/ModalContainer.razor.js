let modalsOpened = false;
let dotNetReference = undefined;
function EscapeListener(ev) {
    if (ev.key === "Escape") {
        return dotNetReference.invokeMethodAsync('CloseNearestModal');
    }
}

// Sets the main page to inert, meaning focus cannot go inside it. Only the last modal can have focus go inside it.
// Also, we add an escape button listener to the ModalContainer, which closes the last opened modal.
export function SetFocusTrapsAndEscapeListener(callingComponent) {
    dotNetReference = callingComponent;
    const modalWrappers = document.querySelectorAll('.usa-modal-wrapper');
    const inertArea = document.getElementById('inert-area');
    if (modalWrappers.length === 0) {
        inertArea?.removeAttribute('inert');
        if (modalsOpened) {
            document.removeEventListener('keydown', EscapeListener);
        }
        modalsOpened = false;
    }
    else {
        inertArea?.setAttribute('inert', '');

        for (let i = 0; i < modalWrappers.length - 1; i++) {
            if (i === modalWrappers.length - 1) {
                modalWrappers.removeAttribute('inert');
            }
            else {
                modalWrappers.setAttribute('inert', '');
            }
        }
        if (!modalsOpened) {
            document.addEventListener('keydown', EscapeListener);
        }
        modalsOpened = true;
    }

}