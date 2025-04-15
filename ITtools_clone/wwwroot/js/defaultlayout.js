document.addEventListener("DOMContentLoaded", function () {
    let sidebar = document.getElementById("sidebar");
    let toggleButton = document.getElementById("toggleSidebar");

    // Layout
    function updateLayout() {
        if (window.innerWidth > 800) {
            sidebar.classList.remove("hidden");
        } else {
            sidebar.classList.add("hidden");
        }
    }

    toggleButton.addEventListener("click", function () {
        sidebar.classList.toggle("hidden");
    });

    window.addEventListener("resize", updateLayout);

    updateLayout();


    // Toggle icon
    let toggles = document.querySelectorAll('[data-bs-toggle="collapse"]');

    toggles.forEach(toggle => {
        let icon = toggle.querySelector("#toggle-icon");

        toggle.addEventListener("click", function () {
            let isOpen = toggle.getAttribute("aria-expanded") === "true";
            
            icon.classList.toggle("bi-chevron-right", !isOpen);
            icon.classList.toggle("bi-chevron-down", isOpen);
        });
    });


    const form = document.querySelector('form[action*="Search"]');
    const searchBox = document.getElementById("search-box");

    form.addEventListener("submit", function (e) {
        const query = searchBox.value.trim();
        if (query === "") {
            e.preventDefault(); // chặn submit
            searchBox.focus(); // có thể highlight lại input nếu muốn
        }
    });
});