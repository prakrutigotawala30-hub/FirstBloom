/* =========================================================
   FIRSTBLOOM STUDENT PORTAL
   SIDEBAR JAVASCRIPT
   ========================================================= */

document.addEventListener("DOMContentLoaded", function () {

    const layout =
        document.getElementById("studentLayout");

    const sidebar =
        document.getElementById("studentSidebar");

    const toggle =
        document.getElementById("studentSidebarToggle");


    if (!layout || !sidebar || !toggle) {
        return;
    }


    /* =====================================================
       SIDEBAR TOGGLE
       ===================================================== */

    toggle.addEventListener("click", function () {

        const isMobile =
            window.innerWidth <= 991;


        if (isMobile) {

            sidebar.classList.toggle(
                "student-mobile-open"
            );

            const isOpen =
                sidebar.classList.contains(
                    "student-mobile-open"
                );

            toggle.setAttribute(
                "aria-expanded",
                isOpen ? "true" : "false"
            );

            return;
        }


        layout.classList.toggle(
            "student-sidebar-collapsed"
        );


        const isCollapsed =
            layout.classList.contains(
                "student-sidebar-collapsed"
            );


        toggle.setAttribute(
            "aria-expanded",
            isCollapsed ? "false" : "true"
        );

    });


    /* =====================================================
       CLOSE MOBILE SIDEBAR WHEN CLICKING OUTSIDE
       ===================================================== */

    document.addEventListener("click", function (event) {

        if (window.innerWidth > 991) {
            return;
        }


        const clickedInsideSidebar =
            sidebar.contains(event.target);

        const clickedToggle =
            toggle.contains(event.target);


        if (
            !clickedInsideSidebar &&
            !clickedToggle
        ) {

            sidebar.classList.remove(
                "student-mobile-open"
            );

            toggle.setAttribute(
                "aria-expanded",
                "false"
            );

        }

    });


    /* =====================================================
       RESET MOBILE STATE ON DESKTOP
       ===================================================== */

    window.addEventListener("resize", function () {

        if (window.innerWidth > 991) {

            sidebar.classList.remove(
                "student-mobile-open"
            );

        }

    });


    /* =====================================================
       ACTIVE MENU
       ===================================================== */

    const currentUrl =
        window.location.pathname
            .toLowerCase();


    const navItems =
        document.querySelectorAll(
            ".student-nav-item"
        );


    navItems.forEach(function (item) {

        const href =
            item.getAttribute("href");


        if (!href || href === "#") {
            return;
        }


        try {

            const linkUrl =
                new URL(
                    href,
                    window.location.origin
                ).pathname.toLowerCase();


            if (
                linkUrl === currentUrl ||
                (
                    linkUrl !== "/" &&
                    currentUrl.startsWith(linkUrl)
                )
            ) {

                item.classList.add("active");

            }

        } catch (error) {

            // Ignore invalid URLs.

        }

    });

});