(function piipanUtilities() {
    if (!window.piipan) {
        window.piipan = {};
    }
    if (!window.piipan.utilities) {
        window.piipan.utilities = {};
    }
    window.piipan.utilities.getCursorPosition = (element) => {
        return element.selectionStart;
    }
    window.piipan.utilities.setCursorPosition = (element, position) => {
        element.setSelectionRange(position, position);
    }
    window.piipan.utilities.setValue = (element, value) => {
        element.value = value;
    }
    window.piipan.utilities.focusElement = (id) => {
        document.getElementById(id)?.focus();
    }
    window.piipan.utilities.scrollToElement = (id) => {
        document.getElementById(id)?.scrollIntoView();
    }
    window.piipan.utilities.doesElementHaveInvalidInput = (id) => {
        return document.getElementById(id)?.validity?.badInput || false;
    }
    window.piipan.utilities.validateForm = async (event) => {
        const valid = await event.target.dotNetReference.invokeMethodAsync('ValidateForm');
        if (valid) {
            await event.target.dotNetReference.invokeMethodAsync('PresubmitForm');
            event.target.submit();
        }
    }
    window.piipan.utilities.registerFormValidation = (formId, dotNetReference) => {
        const form = document.getElementById(formId);
        form.dotNetReference = dotNetReference;
        form.addEventListener('submit', window.piipan.utilities.validateForm);
    }
}());