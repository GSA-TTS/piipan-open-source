export function DoesElementHaveInvalidInput(id) {
    return document.getElementById(id)?.validity?.badInput || false;
}