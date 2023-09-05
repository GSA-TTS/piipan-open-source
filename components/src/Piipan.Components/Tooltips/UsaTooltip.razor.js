// Sets the main page to inert, meaning focus cannot go inside it. Only the last modal can have focus go inside it.
// Also, we add an escape button listener to the ModalContainer, which closes the last opened modal.
export function ShowTooltip(tooltipElement) {
    const rect = tooltipElement.getBoundingClientRect();
    let triggerXPercent = 0;
    if (rect.x < 0 || (rect.x + rect.width > window.innerWidth)) {
        const triggerElement = tooltipElement.closest('.usa-tooltip').querySelector('.usa-tooltip__trigger').getBoundingClientRect();
        const triggerElementXPos = (triggerElement.x + (triggerElement.width / 2));
        triggerXPercent = triggerElementXPos / window.innerWidth;
    }
    
    return { Flip: rect.y < 0, XPercent: triggerXPercent };
}