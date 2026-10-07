/* =========================================================
   FIRSTBLOOM PROGRAM SEARCH
========================================================= */

document.addEventListener("DOMContentLoaded", function () {

    const searchBox = document.getElementById("programSearchBox");
    const searchInput = document.getElementById("programSearchInput");
    const searchButton = document.getElementById("programSearchButton");
    const searchResults = document.getElementById("programSearchResults");

    if (!searchBox || !searchInput || !searchResults) {
        console.warn("Program search elements not found.");
        return;
    }


    /* =====================================================
       SEARCH PROGRAMS
    ===================================================== */

    async function searchPrograms() {

        const searchText = searchInput.value.trim();


        /* -------------------------------------------------
           EMPTY SEARCH
        ------------------------------------------------- */

        if (searchText.length === 0) {

            searchResults.innerHTML = "";
            searchResults.classList.remove("show");

            return;
        }


        /* -------------------------------------------------
           LOADING
        ------------------------------------------------- */

        searchResults.innerHTML = `
            <div class="program-search-loading">

                <i class="fa-solid fa-spinner fa-spin"></i>

                <span>Searching programs...</span>

            </div>
        `;

        searchResults.classList.add("show");


        try {

            /* -------------------------------------------------
               IMPORTANT:
               Controller action is LiveSearch
            ------------------------------------------------- */

            const response = await fetch(
                `/Program/LiveSearch?query=${encodeURIComponent(searchText)}`
            );


            if (!response.ok) {
                throw new Error(
                    `Search request failed: ${response.status}`
                );
            }


            const programs = await response.json();


            /* -------------------------------------------------
               NO RESULTS
            ------------------------------------------------- */

            if (!programs || programs.length === 0) {

                searchResults.innerHTML = `

                    <div class="program-search-no-result">

                        <div class="program-search-no-result-icon">

                            <i class="fa-solid fa-magnifying-glass"></i>

                        </div>

                        <div class="program-search-no-result-text">

                            <strong>
                                No programs found
                            </strong>

                            <span>
                                Try another program name.
                            </span>

                        </div>

                    </div>

                `;

                searchResults.classList.add("show");

                return;
            }


            /* -------------------------------------------------
               DISPLAY PROGRAMS
            ------------------------------------------------- */

            searchResults.innerHTML = programs.map(program => {

                const image = program.imageUrl
                    ? `
                        <img
                            src="${escapeHtml(program.imageUrl)}"
                            alt="${escapeHtml(program.name)}">
                      `
                    : `
                        <i class="fa-solid fa-puzzle-piece"></i>
                      `;


                return `

                    <a href="${escapeHtml(program.url)}"
                       class="program-search-item">

                        <div class="program-search-image">

                            ${image}

                        </div>


                        <div class="program-search-content">

                            <div class="program-search-name">

                                ${escapeHtml(program.name)}

                            </div>


                            <div class="program-search-meta">

                                ${program.ageGroup
                        ? `<span>
                                               ${escapeHtml(program.ageGroup)}
                                           </span>`
                        : ""
                    }


                                ${program.duration
                        ? `
                                            <span>•</span>

                                            <span>
                                                ${escapeHtml(program.duration)}
                                            </span>
                                          `
                        : ""
                    }

                            </div>

                        </div>


                        <i class="fa-solid fa-chevron-right
                                  program-search-arrow">
                        </i>

                    </a>

                `;

            }).join("");


            searchResults.classList.add("show");

        }
        catch (error) {

            console.error("Program search error:", error);


            searchResults.innerHTML = `

                <div class="program-search-no-result">

                    <div class="program-search-no-result-icon">

                        <i class="fa-solid fa-circle-exclamation"></i>

                    </div>


                    <div class="program-search-no-result-text">

                        <strong>
                            Search unavailable
                        </strong>

                        <span>
                            Please try again.
                        </span>

                    </div>

                </div>

            `;

            searchResults.classList.add("show");
        }

    }


    /* =====================================================
       ESCAPE HTML
    ===================================================== */

    function escapeHtml(value) {

        if (value === null || value === undefined) {
            return "";
        }

        return String(value)
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;");
    }


    /* =====================================================
       LIVE SEARCH WHILE TYPING
    ===================================================== */

    let searchTimer;


    searchInput.addEventListener("input", function () {

        clearTimeout(searchTimer);


        searchTimer = setTimeout(function () {

            searchPrograms();

        }, 250);

    });


    /* =====================================================
       SEARCH BUTTON
    ===================================================== */

    if (searchButton) {

        searchButton.addEventListener("click", function () {

            searchPrograms();

        });

    }


    /* =====================================================
       ENTER KEY
    ===================================================== */

    searchInput.addEventListener("keydown", function (event) {

        if (event.key === "Enter") {

            event.preventDefault();

            searchPrograms();

        }

    });


    /* =====================================================
       CLOSE WHEN CLICKING OUTSIDE
    ===================================================== */

    document.addEventListener("click", function (event) {

        if (!searchBox.contains(event.target)) {

            searchResults.classList.remove("show");

        }

    });


    /* =====================================================
       SHOW AGAIN WHEN INPUT GETS FOCUS
    ===================================================== */

    searchInput.addEventListener("focus", function () {

        if (searchInput.value.trim() !== "") {

            searchResults.classList.add("show");

        }

    });

});