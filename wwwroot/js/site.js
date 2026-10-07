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
       RESTORE SIDEBAR STATE
       ===================================================== */

    function restoreSidebarState() {

        if (isMobile()) {

            // Mobile always starts closed
            layout.classList.remove("sidebar-collapsed");

            sidebar.classList.remove("mobile-open");

            toggle.setAttribute(
                "aria-expanded",
                "false"
            );

            return;
        }


        // Desktop: restore saved state
        const savedState =
            localStorage.getItem("fbSidebarState");

        if (savedState === "collapsed") {

            layout.classList.add("sidebar-collapsed");

            toggle.setAttribute(
                "aria-expanded",
                "false"
            );

        } else {

            layout.classList.remove("sidebar-collapsed");

            toggle.setAttribute(
                "aria-expanded",
                "true"
            );
        }
    }


    /* =====================================================
       RESTORE STATE WHEN PAGE LOADS
       ===================================================== */

    restoreSidebarState();


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


            /* =============================================
               SAVE DESKTOP SIDEBAR STATE
               ============================================= */

            if (isCollapsed) {

                localStorage.setItem(
                    "fbSidebarState",
                    "collapsed"
                );

            } else {

                localStorage.setItem(
                    "fbSidebarState",
                    "open"
                );
            }


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

            /*
             * IMPORTANT:
             * Do NOT remove sidebar-collapsed here.
             * The saved desktop state must remain.
             */

            const isCollapsed =
                layout.classList.contains("sidebar-collapsed");

            toggle.setAttribute(
                "aria-expanded",
                (!isCollapsed).toString()
            );

        } else {

            /*
             * Mobile should not use desktop collapsed mode.
             */

            layout.classList.remove("sidebar-collapsed");

            sidebar.classList.remove("mobile-open");

            toggle.setAttribute(
                "aria-expanded",
                "false"
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