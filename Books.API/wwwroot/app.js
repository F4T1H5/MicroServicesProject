(function () {
    const state = {
        authors: [],
        genres: [],
        books: []
    };

    const elements = {
        messageBanner: document.getElementById("messageBanner"),
        refreshButton: document.getElementById("refreshButton"),
        seedButton: document.getElementById("seedButton"),
        authorCount: document.getElementById("authorCount"),
        genreCount: document.getElementById("genreCount"),
        bookCount: document.getElementById("bookCount"),
        topSellerCount: document.getElementById("topSellerCount"),
        authorsTableBody: document.getElementById("authorsTableBody"),
        genresTableBody: document.getElementById("genresTableBody"),
        booksTableBody: document.getElementById("booksTableBody"),
        authorForm: document.getElementById("authorForm"),
        genreForm: document.getElementById("genreForm"),
        bookForm: document.getElementById("bookForm"),
        authorId: document.getElementById("authorId"),
        authorFirstName: document.getElementById("authorFirstName"),
        authorLastName: document.getElementById("authorLastName"),
        genreId: document.getElementById("genreId"),
        genreName: document.getElementById("genreName"),
        bookId: document.getElementById("bookId"),
        bookName: document.getElementById("bookName"),
        bookAuthorId: document.getElementById("bookAuthorId"),
        bookNumberOfPages: document.getElementById("bookNumberOfPages"),
        bookPublishDate: document.getElementById("bookPublishDate"),
        bookPrice: document.getElementById("bookPrice"),
        bookIsTopSeller: document.getElementById("bookIsTopSeller"),
        bookGenres: document.getElementById("bookGenres"),
        authorSubmitButton: document.getElementById("authorSubmitButton"),
        genreSubmitButton: document.getElementById("genreSubmitButton"),
        bookSubmitButton: document.getElementById("bookSubmitButton"),
        resetAuthorFormButton: document.getElementById("resetAuthorFormButton"),
        resetGenreFormButton: document.getElementById("resetGenreFormButton"),
        resetBookFormButton: document.getElementById("resetBookFormButton")
    };

    document.addEventListener("DOMContentLoaded", () => {
        wireEvents();
        resetAuthorForm();
        resetGenreForm();
        resetBookForm();
        loadAll();
    });

    function wireEvents() {
        elements.refreshButton.addEventListener("click", () => loadAll("Data refreshed."));
        elements.seedButton.addEventListener("click", seedDatabase);
        elements.authorForm.addEventListener("submit", submitAuthorForm);
        elements.genreForm.addEventListener("submit", submitGenreForm);
        elements.bookForm.addEventListener("submit", submitBookForm);
        elements.resetAuthorFormButton.addEventListener("click", resetAuthorForm);
        elements.resetGenreFormButton.addEventListener("click", resetGenreForm);
        elements.resetBookFormButton.addEventListener("click", resetBookForm);
    }

    async function loadAll(successMessage) {
        try {
            const [authors, genres, books] = await Promise.all([
                fetchCollection("/api/authors"),
                fetchCollection("/api/genres"),
                fetchCollection("/api/books")
            ]);

            state.authors = authors;
            state.genres = genres;
            state.books = books;

            renderAuthorOptions();
            renderGenreOptions();
            renderStats();
            renderAuthorsTable();
            renderGenresTable();
            renderBooksTable();
            syncBookFormSelections();

            if (successMessage) {
                showMessage(successMessage, "success");
            }
        } catch (error) {
            showMessage(error.message || "Unable to load data.", "error");
        }
    }

    async function fetchCollection(url) {
        const response = await fetch(url, {
            headers: {
                Accept: "application/json"
            }
        });

        if (response.status === 204) {
            return [];
        }

        const payload = await readJson(response);
        if (!response.ok) {
            throw new Error(extractMessage(payload, "Request failed."));
        }

        return Array.isArray(payload) ? payload : [];
    }

    async function submitAuthorForm(event) {
        event.preventDefault();

        const isEdit = Boolean(elements.authorId.value);
        const payload = {
            id: isEdit ? Number(elements.authorId.value) : 0,
            firstName: elements.authorFirstName.value.trim(),
            lastName: elements.authorLastName.value.trim()
        };

        await sendCommand({
            url: "/api/authors",
            method: isEdit ? "PUT" : "POST",
            payload,
            successMessage: isEdit ? "Author updated." : "Author created.",
            afterSuccess: resetAuthorForm
        });
    }

    async function submitGenreForm(event) {
        event.preventDefault();

        const isEdit = Boolean(elements.genreId.value);
        const payload = {
            id: isEdit ? Number(elements.genreId.value) : 0,
            name: elements.genreName.value.trim()
        };

        await sendCommand({
            url: "/api/genres",
            method: isEdit ? "PUT" : "POST",
            payload,
            successMessage: isEdit ? "Genre updated." : "Genre created.",
            afterSuccess: resetGenreForm
        });
    }

    async function submitBookForm(event) {
        event.preventDefault();

        const isEdit = Boolean(elements.bookId.value);
        const payload = {
            id: isEdit ? Number(elements.bookId.value) : 0,
            name: elements.bookName.value.trim(),
            authorId: Number(elements.bookAuthorId.value),
            numberOfPages: elements.bookNumberOfPages.value ? Number(elements.bookNumberOfPages.value) : null,
            publishDate: elements.bookPublishDate.value,
            price: Number(elements.bookPrice.value),
            isTopSeller: elements.bookIsTopSeller.checked,
            genreIds: getSelectedGenreIds()
        };

        await sendCommand({
            url: "/api/books",
            method: isEdit ? "PUT" : "POST",
            payload,
            successMessage: isEdit ? "Book updated." : "Book created.",
            afterSuccess: resetBookForm
        });
    }

    async function seedDatabase() {
        const confirmed = window.confirm("This will replace current book, author, and genre data with demo records. Continue?");
        if (!confirmed) {
            return;
        }

        try {
            const response = await fetch("/api/SeedDb", {
                headers: {
                    Accept: "application/json"
                }
            });

            if (!response.ok) {
                const payload = await readJson(response);
                throw new Error(extractMessage(payload, "Seeding failed."));
            }

            resetAuthorForm();
            resetGenreForm();
            resetBookForm();
            await loadAll("Demo data loaded.");
        } catch (error) {
            showMessage(error.message || "Seeding failed.", "error");
        }
    }

    async function sendCommand({ url, method, payload, successMessage, afterSuccess }) {
        try {
            const response = await fetch(url, {
                method,
                headers: {
                    "Content-Type": "application/json",
                    Accept: "application/json"
                },
                body: JSON.stringify(payload)
            });

            const result = await readJson(response);
            if (!response.ok) {
                throw new Error(extractMessage(result, "Request failed."));
            }

            if (afterSuccess) {
                afterSuccess();
            }

            await loadAll(extractMessage(result, successMessage));
        } catch (error) {
            showMessage(error.message || "Request failed.", "error");
        }
    }

    async function deleteEntity(url, label) {
        const confirmed = window.confirm(`Delete this ${label}?`);
        if (!confirmed) {
            return;
        }

        try {
            const response = await fetch(url, {
                method: "DELETE",
                headers: {
                    Accept: "application/json"
                }
            });

            const result = await readJson(response);
            if (!response.ok) {
                throw new Error(extractMessage(result, `Unable to delete ${label}.`));
            }

            await loadAll(extractMessage(result, `${capitalize(label)} deleted.`));
        } catch (error) {
            showMessage(error.message || `Unable to delete ${label}.`, "error");
        }
    }

    function renderStats() {
        elements.authorCount.textContent = String(state.authors.length);
        elements.genreCount.textContent = String(state.genres.length);
        elements.bookCount.textContent = String(state.books.length);
        elements.topSellerCount.textContent = String(state.books.filter(book => book.isTopSeller).length);
    }

    function renderAuthorOptions() {
        const currentValue = elements.bookAuthorId.value;
        const hasAuthors = state.authors.length > 0;

        elements.bookAuthorId.innerHTML = hasAuthors
            ? state.authors.map(author => `<option value="${author.id}">${escapeHtml(author.firstName)} ${escapeHtml(author.lastName)}</option>`).join("")
            : `<option value="">Create an author first</option>`;

        elements.bookAuthorId.disabled = !hasAuthors;

        if (hasAuthors) {
            const exists = state.authors.some(author => String(author.id) === currentValue);
            elements.bookAuthorId.value = exists ? currentValue : String(state.authors[0].id);
        }
    }

    function renderGenreOptions() {
        const selectedGenreIds = new Set(getSelectedGenreIds());

        elements.bookGenres.innerHTML = state.genres.length === 0
            ? `<p class="empty-state">Create at least one genre to tag books.</p>`
            : state.genres.map(genre => `
                <label class="checkbox-option">
                    <input type="checkbox" value="${genre.id}" ${selectedGenreIds.has(genre.id) ? "checked" : ""} />
                    <span>${escapeHtml(genre.name)}</span>
                </label>
            `).join("");
    }

    function renderAuthorsTable() {
        if (state.authors.length === 0) {
            elements.authorsTableBody.innerHTML = `<tr><td colspan="2" class="muted-text">No authors yet.</td></tr>`;
            return;
        }

        elements.authorsTableBody.innerHTML = state.authors.map(author => `
            <tr>
                <td>${escapeHtml(author.firstName)} ${escapeHtml(author.lastName)}</td>
                <td>
                    <div class="table-actions">
                        <button class="secondary-button" type="button" data-action="edit-author" data-id="${author.id}">Edit</button>
                        <button class="danger-button" type="button" data-action="delete-author" data-id="${author.id}">Delete</button>
                    </div>
                </td>
            </tr>
        `).join("");

        bindTableActions();
    }

    function renderGenresTable() {
        if (state.genres.length === 0) {
            elements.genresTableBody.innerHTML = `<tr><td colspan="3" class="muted-text">No genres yet.</td></tr>`;
            return;
        }

        elements.genresTableBody.innerHTML = state.genres.map(genre => `
            <tr>
                <td>${escapeHtml(genre.name)}</td>
                <td>${escapeHtml(genre.booksF || "No books")}</td>
                <td>
                    <div class="table-actions">
                        <button class="secondary-button" type="button" data-action="edit-genre" data-id="${genre.id}">Edit</button>
                        <button class="danger-button" type="button" data-action="delete-genre" data-id="${genre.id}">Delete</button>
                    </div>
                </td>
            </tr>
        `).join("");

        bindTableActions();
    }

    function renderBooksTable() {
        if (state.books.length === 0) {
            elements.booksTableBody.innerHTML = `<tr><td colspan="7" class="muted-text">No books yet.</td></tr>`;
            return;
        }

        elements.booksTableBody.innerHTML = state.books.map(book => `
            <tr>
                <td>
                    <strong>${escapeHtml(book.name)}</strong>
                    <div class="muted-text">${book.numberOfPages ? `${book.numberOfPages} pages` : "Page count not set"}</div>
                </td>
                <td>${escapeHtml(book.authorFullName || "Unknown")}</td>
                <td>${escapeHtml((book.genresF || []).join(", ") || "Uncategorized")}</td>
                <td>${escapeHtml(book.publishDateF || "")}</td>
                <td>${escapeHtml(book.priceF || "")}</td>
                <td><span class="badge">${escapeHtml(book.isTopSellerF || "Standard")}</span></td>
                <td>
                    <div class="table-actions">
                        <button class="secondary-button" type="button" data-action="edit-book" data-id="${book.id}">Edit</button>
                        <button class="danger-button" type="button" data-action="delete-book" data-id="${book.id}">Delete</button>
                    </div>
                </td>
            </tr>
        `).join("");

        bindTableActions();
    }

    function bindTableActions() {
        document.querySelectorAll("[data-action]").forEach(button => {
            button.onclick = () => {
                const action = button.dataset.action;
                const id = Number(button.dataset.id);

                if (action === "edit-author") {
                    editAuthor(id);
                } else if (action === "delete-author") {
                    deleteEntity(`/api/authors/${id}`, "author");
                } else if (action === "edit-genre") {
                    editGenre(id);
                } else if (action === "delete-genre") {
                    deleteEntity(`/api/genres/${id}`, "genre");
                } else if (action === "edit-book") {
                    editBook(id);
                } else if (action === "delete-book") {
                    deleteEntity(`/api/books/${id}`, "book");
                }
            };
        });
    }

    function editAuthor(id) {
        const author = state.authors.find(item => item.id === id);
        if (!author) {
            return;
        }

        elements.authorId.value = String(author.id);
        elements.authorFirstName.value = author.firstName || "";
        elements.authorLastName.value = author.lastName || "";
        elements.authorSubmitButton.textContent = "Update Author";
        elements.authorFirstName.focus();
    }

    function editGenre(id) {
        const genre = state.genres.find(item => item.id === id);
        if (!genre) {
            return;
        }

        elements.genreId.value = String(genre.id);
        elements.genreName.value = genre.name || "";
        elements.genreSubmitButton.textContent = "Update Genre";
        elements.genreName.focus();
    }

    function editBook(id) {
        const book = state.books.find(item => item.id === id);
        if (!book) {
            return;
        }

        elements.bookId.value = String(book.id);
        elements.bookName.value = book.name || "";
        elements.bookAuthorId.value = String(book.authorId || "");
        elements.bookNumberOfPages.value = book.numberOfPages ?? "";
        elements.bookPublishDate.value = normalizeDateInput(book.publishDate);
        elements.bookPrice.value = Number(book.price || 0).toFixed(2);
        elements.bookIsTopSeller.checked = Boolean(book.isTopSeller);
        syncBookFormSelections(book.genreIds || []);
        elements.bookSubmitButton.textContent = "Update Book";
        elements.bookName.focus();
    }

    function resetAuthorForm() {
        elements.authorForm.reset();
        elements.authorId.value = "";
        elements.authorSubmitButton.textContent = "Create Author";
    }

    function resetGenreForm() {
        elements.genreForm.reset();
        elements.genreId.value = "";
        elements.genreSubmitButton.textContent = "Create Genre";
    }

    function resetBookForm() {
        elements.bookForm.reset();
        elements.bookId.value = "";
        elements.bookSubmitButton.textContent = "Create Book";

        if (state.authors.length > 0) {
            elements.bookAuthorId.value = String(state.authors[0].id);
        }

        if (!elements.bookPublishDate.value) {
            elements.bookPublishDate.value = new Date().toISOString().slice(0, 10);
        }

        syncBookFormSelections([]);
        toggleBookFormAvailability();
    }

    function toggleBookFormAvailability() {
        const canManageBooks = state.authors.length > 0;

        elements.bookName.disabled = !canManageBooks;
        elements.bookAuthorId.disabled = !canManageBooks;
        elements.bookNumberOfPages.disabled = !canManageBooks;
        elements.bookPublishDate.disabled = !canManageBooks;
        elements.bookPrice.disabled = !canManageBooks;
        elements.bookIsTopSeller.disabled = !canManageBooks;
        elements.bookSubmitButton.disabled = !canManageBooks;
    }

    function syncBookFormSelections(selectedIds) {
        const targetIds = new Set(Array.isArray(selectedIds) ? selectedIds : getSelectedGenreIds());
        renderGenreOptions();
        elements.bookGenres.querySelectorAll("input[type='checkbox']").forEach(checkbox => {
            checkbox.checked = targetIds.has(Number(checkbox.value));
        });
        toggleBookFormAvailability();
    }

    function getSelectedGenreIds() {
        return Array.from(elements.bookGenres.querySelectorAll("input[type='checkbox']:checked"))
            .map(checkbox => Number(checkbox.value));
    }

    function showMessage(message, type) {
        elements.messageBanner.textContent = message;
        elements.messageBanner.className = `message-banner ${type}`;
    }

    function extractMessage(payload, fallback) {
        if (!payload) {
            return fallback;
        }

        if (typeof payload === "string") {
            return payload;
        }

        if (payload.message) {
            return payload.message;
        }

        return fallback;
    }

    async function readJson(response) {
        const contentType = response.headers.get("content-type") || "";
        if (contentType.includes("application/json")) {
            return response.json();
        }

        const text = await response.text();
        return text || null;
    }

    function normalizeDateInput(value) {
        if (!value) {
            return "";
        }

        return String(value).slice(0, 10);
    }

    function escapeHtml(value) {
        return String(value ?? "")
            .replaceAll("&", "&amp;")
            .replaceAll("<", "&lt;")
            .replaceAll(">", "&gt;")
            .replaceAll("\"", "&quot;")
            .replaceAll("'", "&#39;");
    }

    function capitalize(value) {
        return value.charAt(0).toUpperCase() + value.slice(1);
    }
})();
