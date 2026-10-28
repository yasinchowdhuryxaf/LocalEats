document.addEventListener("submit", event => {
    const form = event.target;
    if (!(form instanceof HTMLFormElement) || form.id !== "checkout-form" || event.defaultPrevented) return;
    const button = form.querySelector('button[type="submit"]');
    if (button) { button.disabled = true; button.textContent = "Reviewing…"; }
});
window.addEventListener("pageshow", () => {
    const button = document.querySelector('#checkout-form button[type="submit"]');
    if (button) { button.disabled = button.dataset.canReview !== "true"; button.textContent = "Review checkout"; }
});
