
document.addEventListener("DOMContentLoaded", function () {

    const wrapper = document.getElementById("fbNotificationWrapper");
    const button = document.getElementById("fbNotificationBtn");
    const dropdown = document.getElementById("fbNotificationDropdown");
    const list = document.getElementById("fbNotificationList");
    const count = document.getElementById("fbNotificationCount");
    const subtitle = document.getElementById("fbNotificationSubtitle");
    const markAll = document.getElementById("fbMarkAll");


    // =========================================================
    // SAFETY CHECK
    // =========================================================

    if (!wrapper || !button || !dropdown || !list || !count) {
        console.warn("Notification elements not found.");
        return;
    }


    // =========================================================
    // LOCAL STORAGE KEY
    // =========================================================

    const viewedStorageKey = "firstbloom_viewed_notifications";


    // =========================================================
    // GET VIEWED NOTIFICATION IDS
    // =========================================================

    function getViewedNotifications() {

        try {

            const stored =
                localStorage.getItem(viewedStorageKey);

            if (!stored) {
                return [];
            }

            return JSON.parse(stored);

        }
        catch (error) {

            console.error(
                "Unable to read viewed notifications.",
                error
            );

            return [];
        }
    }


    // =========================================================
    // SAVE VIEWED NOTIFICATION IDS
    // =========================================================

    function saveViewedNotifications(ids) {

        try {

            localStorage.setItem(
                viewedStorageKey,
                JSON.stringify(ids)
            );

        }
        catch (error) {

            console.error(
                "Unable to save viewed notifications.",
                error
            );
        }
    }


    // =========================================================
    // MARK ONE NOTIFICATION AS VIEWED
    // =========================================================

    function markNotificationViewed(id) {

        let viewed = getViewedNotifications();

        id = Number(id);

        if (!viewed.includes(id)) {

            viewed.push(id);

            saveViewedNotifications(viewed);
        }
    }


    // =========================================================
    // LOAD NOTIFICATIONS
    // =========================================================

    async function loadNotifications() {

        try {

            const response =
                await fetch("/Notice/GetNotifications", {
                    method: "GET",
                    headers: {
                        "X-Requested-With": "XMLHttpRequest"
                    },
                    cache: "no-store"
                });


            if (!response.ok) {

                throw new Error(
                    "Notification API failed."
                );
            }


            const notifications =
                await response.json();


            renderNotifications(notifications);

        }
        catch (error) {

            console.error(
                "Notification error:",
                error
            );

            list.innerHTML = `
    < div class="fb-notification-empty" >

                    <div class="fb-notification-empty-icon">
                        <i class="fa-solid fa-triangle-exclamation"></i>
                    </div>

                    <h5>
                        Unable to load notifications
                    </h5>

                    <p>
                        Please try again later.
                    </p>

                </div >
    `;
        }
    }


    // =========================================================
    // RENDER NOTIFICATIONS
    // =========================================================

    function renderNotifications(notifications) {

        list.innerHTML = "";


        /* =====================================================
           GET VIEWED NOTIFICATION IDS
        ===================================================== */

        const viewedIds = JSON.parse(
            localStorage.getItem("firstbloom_viewed_notices") || "[]"
        );


        /* =====================================================
           REMOVE NOTICES ALREADY VIEWED
        ===================================================== */

        const unreadNotifications = notifications.filter(function (notice) {

            return !viewedIds.includes(Number(notice.id));

        });


        /* =====================================================
           NO NEW NOTIFICATIONS
        ===================================================== */

        if (
            !unreadNotifications ||
            unreadNotifications.length === 0
        ) {

            count.classList.add("hidden");

            subtitle.textContent = "All notices viewed";


            markAll.classList.add("viewed");

            markAll.innerHTML = `
            <i class="fa-solid fa-check"></i>
            <span>All viewed</span>
        `;


            list.innerHTML = `
            <div class="fb-notification-empty">

                <div class="fb-notification-empty-icon">

                    <i class="fa-regular fa-bell-slash"></i>

                </div>

                <h5>
                    You're all caught up!
                </h5>

                <p>
                    There are no new notices right now.
                </p>

            </div>
        `;

            return;
        }


        /* =====================================================
           RESET BUTTON
        ===================================================== */

        markAll.classList.remove("viewed");

        markAll.innerHTML = `
        <i class="fa-solid fa-check-double"></i>
        <span>Mark viewed</span>
    `;


        /* =====================================================
           COUNT
        ===================================================== */

        count.textContent =
            unreadNotifications.length;

        count.classList.remove("hidden");


        subtitle.textContent =
            unreadNotifications.length +
            (
                unreadNotifications.length === 1
                    ? " new notice"
                    : " new notices"
            );


        /* =====================================================
           CREATE NOTIFICATIONS
        ===================================================== */

        unreadNotifications.forEach(function (notice) {

            const item =
                document.createElement("a");

            item.className =
                "fb-notification-item unread";

            item.href =
                "/Notice/Details/" + notice.id;


            /* ICON */

            const icon =
                document.createElement("div");

            icon.className =
                "fb-notification-icon";

            icon.innerHTML =
                '<i class="fa-solid fa-bullhorn"></i>';


            /* CONTENT */

            const content =
                document.createElement("div");

            content.className =
                "fb-notification-content";


            /* TITLE */

            const title =
                document.createElement("h5");

            title.className =
                "fb-notification-title";

            title.textContent =
                notice.title;


            /* MESSAGE */

            const message =
                document.createElement("p");

            message.className =
                "fb-notification-message";

            message.textContent =
                notice.message;


            /* TIME */

            const time =
                document.createElement("small");

            time.className =
                "fb-notification-time";

            time.textContent =
                formatDate(
                    notice.publishedAt ||
                    notice.createdAt
                );


            content.appendChild(title);
            content.appendChild(message);
            content.appendChild(time);


            /* UNREAD DOT */

            const unread =
                document.createElement("span");

            unread.className =
                "fb-notification-unread";


            /* ADD */

            item.appendChild(icon);
            item.appendChild(content);
            item.appendChild(unread);

            list.appendChild(item);

        });

    }


    // =========================================================
    // DATE FORMAT
    // =========================================================

    function formatDate(dateValue) {

        if (!dateValue) {

            return "Recently";
        }


        const date =
            new Date(dateValue);


        if (isNaN(date.getTime())) {

            return "Recently";
        }


        const now =
            new Date();


        const difference =
            now - date;


        const minutes =
            Math.floor(
                difference /
                (1000 * 60)
            );


        if (minutes < 1) {

            return "Just now";
        }


        if (minutes < 60) {

            return minutes +
                (
                    minutes === 1
                        ? " minute ago"
                        : " minutes ago"
                );
        }


        const hours =
            Math.floor(
                minutes / 60
            );


        if (hours < 24) {

            return hours +
                (
                    hours === 1
                        ? " hour ago"
                        : " hours ago"
                );
        }


        const days =
            Math.floor(
                hours / 24
            );


        if (days < 7) {

            return days +
                (
                    days === 1
                        ? " day ago"
                        : " days ago"
                );
        }


        return date.toLocaleDateString(
            "en-IN",
            {
                day: "2-digit",
                month: "short",
                year: "numeric"
            }
        );
    }


    // =========================================================
    // OPEN / CLOSE DROPDOWN
    // =========================================================

    button.addEventListener(
        "click",
        function (event) {

            event.stopPropagation();


            const isOpen =
                wrapper.classList.contains("open");


            if (isOpen) {

                wrapper.classList.remove("open");

                button.setAttribute(
                    "aria-expanded",
                    "false"
                );

            }
            else {

                wrapper.classList.add("open");

                button.setAttribute(
                    "aria-expanded",
                    "true"
                );
            }

        }
    );


    // =========================================================
    // CLOSE WHEN CLICKING OUTSIDE
    // =========================================================

    document.addEventListener(
        "click",
        function (event) {

            if (!wrapper.contains(event.target)) {

                wrapper.classList.remove("open");

                button.setAttribute(
                    "aria-expanded",
                    "false"
                );
            }

        }
    );


    /* =====================================================
   MARK ALL AS VIEWED
===================================================== */

    markAll.addEventListener("click", function (event) {

        event.preventDefault();
        event.stopPropagation();

        const notificationItems =
            document.querySelectorAll(".fb-notification-item");

        const viewedIds = JSON.parse(
            localStorage.getItem("firstbloom_viewed_notices") || "[]"
        );

        notificationItems.forEach(function (item) {

            const href = item.getAttribute("href");

            if (!href) {
                return;
            }

            const parts = href.split("/");
            const id = parseInt(parts[parts.length - 1]);

            if (!isNaN(id) && !viewedIds.includes(id)) {
                viewedIds.push(id);
            }

            item.classList.remove("unread");

            const dot =
                item.querySelector(".fb-notification-unread");

            if (dot) {
                dot.style.display = "none";
            }
        });


        /* Save viewed notification IDs */

        localStorage.setItem(
            "firstbloom_viewed_notices",
            JSON.stringify(viewedIds)
        );


        /* Update count */

        count.classList.add("hidden");
        count.textContent = "0";


        /* Update subtitle */

        subtitle.textContent = "All notices viewed";


        /* Change button appearance */

        markAll.classList.add("viewed");

        markAll.innerHTML = `
        <i class="fa-solid fa-check"></i>
        <span>All viewed</span>
    `;

    });

    // =========================================================
    // INITIAL LOAD
    // =========================================================

    loadNotifications();


    // =========================================================
    // QUICK REFRESH
    // Check every 30 seconds for NEW notices
    // =========================================================

    setInterval(
        loadNotifications,
        30000
    );

});
