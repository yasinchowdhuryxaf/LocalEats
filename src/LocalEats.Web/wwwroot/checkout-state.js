document.addEventListener("submit", event => {
    const form = event.target;
    if (!(form instanceof HTMLFormElement) || form.id !== "checkout-form" || event.defaultPrevented) return;
    const action = document.createElement("input");
    action.type = "hidden"; action.name = "Action"; action.value = event.submitter?.value || "review";
    action.dataset.pendingAction = "true"; form.append(action);
    form.querySelectorAll('button[type="submit"]').forEach(button => { button.disabled = true; button.textContent = "Processing…"; });
});
window.addEventListener("pageshow", () => {
    document.querySelectorAll('#checkout-form input[data-pending-action]').forEach(input => input.remove());
    document.querySelectorAll('#checkout-form button[type="submit"]').forEach(button => {
        button.disabled = button.dataset.ready !== "true"; button.textContent = button.dataset.label;
    });
});
