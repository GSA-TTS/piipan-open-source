(function piipanUtilities() {
    if (!window.piipan) {
        window.piipan = {};
    }
    if (!window.piipan.utilities) {
        window.piipan.utilities = {};
    }
    window.piipan.utilities.clearValue = (element) => {
        element.value = "";
    }
    window.piipan.utilities.focusElement = (id) => {
        document.getElementById(id)?.focus();
    }
    window.piipan.utilities.scrollToElement = (id) => {
        document.getElementById(id)?.scrollIntoView();
    }
}());