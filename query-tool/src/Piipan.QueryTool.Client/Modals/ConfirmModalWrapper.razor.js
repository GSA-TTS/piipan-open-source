const beforeUnloadListener = (event) => {
    event.preventDefault();
    return event.returnValue = "Continue Without Saving?";
};

// This is called from ConfirmModalWrapper when NavigationBlocked changes. If it's true, we should block navigation
// using the default browser functionality anytime Blazor cannot intercept it.
export function SetUnloadListener(shouldConfirm) {
    if (shouldConfirm) {
        window.addEventListener("beforeunload", beforeUnloadListener, { capture: true });
    }
    else {
        window.removeEventListener("beforeunload", beforeUnloadListener, { capture: true });
    }
}