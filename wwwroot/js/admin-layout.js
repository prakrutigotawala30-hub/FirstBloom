document.addEventListener("DOMContentLoaded", function () {

    const wrapper =
        document.querySelector(".admin-wrapper");

    const sidebar =
        document.querySelector(".admin-sidebar");

    const toggle =
        document.getElementById("adminSidebarToggle");


    if (!wrapper || !sidebar || !toggle) {
        return;
    }


    // =====================================================
    // ADD TOOLTIP NAME TO EVERY SIDEBAR ITEM
    // =====================================================

    const sidebarLinks =
        document.querySelectorAll(".admin-nav a");

    sidebarLinks.forEach(function (link) {

        const textElement =
            link.querySelector("span");

        if (textElement) {

            const menuName =
                textElement.textContent.trim();

            link.setAttribute(
                "data-tooltip",
                menuName
            );

        }

    });


    // =====================================================
    // RESTORE SAVED SIDEBAR STATE
    // =====================================================

    const savedState =
        localStorage.getItem("adminSidebarState");


    if (savedState === "collapsed") {

        wrapper.classList.add("sidebar-collapsed");

        toggle.setAttribute(
            "aria-expanded",
            "false"
        );

    }
    else {

        wrapper.classList.remove("sidebar-collapsed");

        toggle.setAttribute(
            "aria-expanded",
            "true"
        );

    }


    // =====================================================
    // TOGGLE SIDEBAR
    // =====================================================

    toggle.addEventListener("click", function () {

        wrapper.classList.toggle(
            "sidebar-collapsed"
        );


        const isCollapsed =
            wrapper.classList.contains(
                "sidebar-collapsed"
            );


        // Save state

        if (isCollapsed) {

            localStorage.setItem(
                "adminSidebarState",
                "collapsed"
            );

        }
        else {

            localStorage.setItem(
                "adminSidebarState",
                "open"
            );

        }


        // Accessibility

        toggle.setAttribute(
            "aria-expanded",
            (!isCollapsed).toString()
        );

    });

});