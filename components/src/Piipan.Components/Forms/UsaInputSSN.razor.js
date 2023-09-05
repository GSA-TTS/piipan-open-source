
export function GetCursorPosition(element) {
    return element.selectionStart;
}

export function SetCursorPosition(element, position) {
    element.setSelectionRange(position, position);
}

export function SetValue(element, value) {
    element.value = value;
}