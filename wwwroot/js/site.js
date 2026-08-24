document.addEventListener("DOMContentLoaded", function () {

    const layout = document.getElementById("fbLayout");
    const sidebar = document.getElementById("fbSidebar");
    const toggle = document.getElementById("fbSidebarToggle");

    if (!layout || !sidebar || !toggle) {
        return;
    }


    /* =====================================================
       DESKTOP / MOBILE DETECTION
       ===================================================== */

    function isMobile() {
        return window.innerWidth <= 991;
    }


    /* =====================================================
       TOGGLE SIDEBAR
       ===================================================== */

    toggle.addEventListener("click", function () {

        if (isMobile()) {

            sidebar.classList.toggle("mobile-open");

            const isOpen =
                sidebar.classList.contains("mobile-open");

            toggle.setAttribute(
                "aria-expanded",
                isOpen.toString()
            );

        } else {

            layout.classList.toggle("sidebar-collapsed");

            const isCollapsed =
                layout.classList.contains("sidebar-collapsed");

            toggle.setAttribute(
                "aria-expanded",
                (!isCollapsed).toString()
            );
        }
    });


    /* =====================================================
       CLOSE MOBILE SIDEBAR WHEN CLICKING A LINK
       ===================================================== */

    const sidebarLinks =
        sidebar.querySelectorAll(".fb-nav-item");

    sidebarLinks.forEach(function (link) {

        link.addEventListener("click", function () {

            if (isMobile()) {

                sidebar.classList.remove("mobile-open");

                toggle.setAttribute(
                    "aria-expanded",
                    "false"
                );
            }
        });

    });


    /* =====================================================
       HANDLE WINDOW RESIZE
       ===================================================== */

    window.addEventListener("resize", function () {

        if (!isMobile()) {

            sidebar.classList.remove("mobile-open");

            toggle.setAttribute(
                "aria-expanded",
                layout.classList.contains("sidebar-collapsed")
                    ? "false"
                    : "true"
            );

        } else {

            layout.classList.remove("sidebar-collapsed");

            toggle.setAttribute(
                "aria-expanded",
                sidebar.classList.contains("mobile-open")
                    ? "true"
                    : "false"
            );
        }

    });


    /* =====================================================
       ACTIVE SIDEBAR MENU
       ===================================================== */

    const currentPath =
        window.location.pathname.toLowerCase();

    sidebarLinks.forEach(function (link) {

        const href =
            link.getAttribute("href");

        if (!href || href === "#") {
            return;
        }

        const linkPath =
            new URL(
                href,
                window.location.origin
            ).pathname.toLowerCase();

        if (currentPath === linkPath) {

            link.classList.add("active");

        } else {

            link.classList.remove("active");

        }

    });

});