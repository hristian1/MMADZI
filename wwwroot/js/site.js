document.addEventListener("DOMContentLoaded", () => {
    const dock = document.querySelector("[data-category-dock]");
    if (!dock) {
        return;
    }

    const toggle = dock.querySelector("[data-category-dock-toggle]");
    const closeControls = dock.querySelectorAll("[data-category-dock-close]");

    const setOpen = (isOpen) => {
        dock.classList.toggle("is-open", isOpen);
        toggle?.setAttribute("aria-expanded", isOpen ? "true" : "false");
        document.body.classList.toggle("category-dock-open", isOpen);
    };

    toggle?.addEventListener("click", () => {
        setOpen(!dock.classList.contains("is-open"));
    });

    closeControls.forEach((control) => {
        control.addEventListener("click", () => setOpen(false));
    });

    setOpen(dock.classList.contains("is-open"));

    document.addEventListener("keydown", (event) => {
        if (event.key === "Escape") {
            setOpen(false);
        }
    });
});
