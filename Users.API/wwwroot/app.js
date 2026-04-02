(function () {
    const state = {
        groups: [],
        roles: [],
        users: []
    };

    const elements = {
        messageBanner: document.getElementById("messageBanner"),
        refreshButton: document.getElementById("refreshButton"),
        seedButton: document.getElementById("seedButton"),
        groupCount: document.getElementById("groupCount"),
        roleCount: document.getElementById("roleCount"),
        userCount: document.getElementById("userCount"),
        activeUserCount: document.getElementById("activeUserCount"),
        groupsTableBody: document.getElementById("groupsTableBody"),
        rolesTableBody: document.getElementById("rolesTableBody"),
        usersTableBody: document.getElementById("usersTableBody"),
        groupForm: document.getElementById("groupForm"),
        roleForm: document.getElementById("roleForm"),
        userForm: document.getElementById("userForm"),
        groupId: document.getElementById("groupId"),
        groupTitle: document.getElementById("groupTitle"),
        roleId: document.getElementById("roleId"),
        roleName: document.getElementById("roleName"),
        userId: document.getElementById("userId"),
        userUserName: document.getElementById("userUserName"),
        userPassword: document.getElementById("userPassword"),
        userFirstName: document.getElementById("userFirstName"),
        userLastName: document.getElementById("userLastName"),
        userGender: document.getElementById("userGender"),
        userBirthDate: document.getElementById("userBirthDate"),
        userScore: document.getElementById("userScore"),
        userAddress: document.getElementById("userAddress"),
        userCountryId: document.getElementById("userCountryId"),
        userCityId: document.getElementById("userCityId"),
        userGroupId: document.getElementById("userGroupId"),
        userIsActive: document.getElementById("userIsActive"),
        userRoles: document.getElementById("userRoles"),
        groupSubmitButton: document.getElementById("groupSubmitButton"),
        roleSubmitButton: document.getElementById("roleSubmitButton"),
        userSubmitButton: document.getElementById("userSubmitButton"),
        resetGroupFormButton: document.getElementById("resetGroupFormButton"),
        resetRoleFormButton: document.getElementById("resetRoleFormButton"),
        resetUserFormButton: document.getElementById("resetUserFormButton")
    };

    document.addEventListener("DOMContentLoaded", () => {
        wireEvents();
        resetGroupForm();
        resetRoleForm();
        resetUserForm();
        loadAll();
    });

    function wireEvents() {
        elements.refreshButton.addEventListener("click", () => loadAll("Data refreshed."));
        elements.seedButton.addEventListener("click", seedDatabase);
        elements.groupForm.addEventListener("submit", submitGroupForm);
        elements.roleForm.addEventListener("submit", submitRoleForm);
        elements.userForm.addEventListener("submit", submitUserForm);
        elements.resetGroupFormButton.addEventListener("click", resetGroupForm);
        elements.resetRoleFormButton.addEventListener("click", resetRoleForm);
        elements.resetUserFormButton.addEventListener("click", resetUserForm);
    }

    async function loadAll(successMessage) {
        try {
            const [groups, roles, users] = await Promise.all([
                fetchCollection("/api/groups"),
                fetchCollection("/api/roles"),
                fetchCollection("/api/users")
            ]);

            state.groups = groups;
            state.roles = roles;
            state.users = users;

            renderUserFormOptions();
            renderStats();
            renderGroupsTable();
            renderRolesTable();
            renderUsersTable();

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

    async function submitGroupForm(event) {
        event.preventDefault();

        const isEdit = Boolean(elements.groupId.value);
        const payload = {
            id: isEdit ? Number(elements.groupId.value) : 0,
            title: elements.groupTitle.value.trim()
        };

        await sendCommand({
            url: "/api/groups",
            method: isEdit ? "PUT" : "POST",
            payload,
            successMessage: isEdit ? "Group updated." : "Group created.",
            afterSuccess: resetGroupForm
        });
    }

    async function submitRoleForm(event) {
        event.preventDefault();

        const isEdit = Boolean(elements.roleId.value);
        const payload = {
            id: isEdit ? Number(elements.roleId.value) : 0,
            name: elements.roleName.value.trim()
        };

        await sendCommand({
            url: "/api/roles",
            method: isEdit ? "PUT" : "POST",
            payload,
            successMessage: isEdit ? "Role updated." : "Role created.",
            afterSuccess: resetRoleForm
        });
    }

    async function submitUserForm(event) {
        event.preventDefault();

        const isEdit = Boolean(elements.userId.value);
        const payload = {
            id: isEdit ? Number(elements.userId.value) : 0,
            userName: elements.userUserName.value.trim(),
            password: elements.userPassword.value,
            firstName: elements.userFirstName.value.trim() || null,
            lastName: elements.userLastName.value.trim() || null,
            gender: Number(elements.userGender.value),
            birthDate: elements.userBirthDate.value || null,
            score: elements.userScore.value ? Number(elements.userScore.value) : 0,
            isActive: elements.userIsActive.checked,
            address: elements.userAddress.value.trim() || null,
            countryId: elements.userCountryId.value ? Number(elements.userCountryId.value) : null,
            cityId: elements.userCityId.value ? Number(elements.userCityId.value) : null,
            groupId: elements.userGroupId.value ? Number(elements.userGroupId.value) : null,
            roleIds: getSelectedRoleIds()
        };

        await sendCommand({
            url: "/api/users",
            method: isEdit ? "PUT" : "POST",
            payload,
            successMessage: isEdit ? "User updated." : "User created.",
            afterSuccess: resetUserForm
        });
    }

    async function seedDatabase() {
        const confirmed = window.confirm("This will replace current users, groups, and roles with demo records. Continue?");
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

            resetGroupForm();
            resetRoleForm();
            resetUserForm();
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
        elements.groupCount.textContent = String(state.groups.length);
        elements.roleCount.textContent = String(state.roles.length);
        elements.userCount.textContent = String(state.users.length);
        elements.activeUserCount.textContent = String(state.users.filter(user => user.isActive).length);
    }

    function renderUserFormOptions() {
        const currentGroupValue = elements.userGroupId.value;
        const selectedRoleIds = new Set(getSelectedRoleIds());

        elements.userGroupId.innerHTML = `
            <option value="">No Group</option>
            ${state.groups.map(group => `<option value="${group.id}">${escapeHtml(group.title)}</option>`).join("")}
        `;

        if (currentGroupValue && state.groups.some(group => String(group.id) === currentGroupValue)) {
            elements.userGroupId.value = currentGroupValue;
        }

        elements.userRoles.innerHTML = state.roles.length === 0
            ? `<p class="empty-state">Create at least one role to assign users.</p>`
            : state.roles.map(role => `
                <label class="checkbox-option">
                    <input type="checkbox" value="${role.id}" ${selectedRoleIds.has(role.id) ? "checked" : ""} />
                    <span>${escapeHtml(role.name)}</span>
                </label>
            `).join("");
    }

    function renderGroupsTable() {
        if (state.groups.length === 0) {
            elements.groupsTableBody.innerHTML = `<tr><td colspan="2" class="muted-text">No groups yet.</td></tr>`;
            return;
        }

        elements.groupsTableBody.innerHTML = state.groups.map(group => `
            <tr>
                <td>${escapeHtml(group.title)}</td>
                <td>
                    <div class="table-actions">
                        <button class="secondary-button" type="button" data-action="edit-group" data-id="${group.id}">Edit</button>
                        <button class="danger-button" type="button" data-action="delete-group" data-id="${group.id}">Delete</button>
                    </div>
                </td>
            </tr>
        `).join("");

        bindTableActions();
    }

    function renderRolesTable() {
        if (state.roles.length === 0) {
            elements.rolesTableBody.innerHTML = `<tr><td colspan="3" class="muted-text">No roles yet.</td></tr>`;
            return;
        }

        elements.rolesTableBody.innerHTML = state.roles.map(role => `
            <tr>
                <td>${escapeHtml(role.name)}</td>
                <td>${escapeHtml(role.usersF || "No users")}</td>
                <td>
                    <div class="table-actions">
                        <button class="secondary-button" type="button" data-action="edit-role" data-id="${role.id}">Edit</button>
                        <button class="danger-button" type="button" data-action="delete-role" data-id="${role.id}">Delete</button>
                    </div>
                </td>
            </tr>
        `).join("");

        bindTableActions();
    }

    function renderUsersTable() {
        if (state.users.length === 0) {
            elements.usersTableBody.innerHTML = `<tr><td colspan="6" class="muted-text">No users yet.</td></tr>`;
            return;
        }

        elements.usersTableBody.innerHTML = state.users.map(user => `
            <tr>
                <td>
                    <strong>${escapeHtml(user.userName)}</strong>
                    <div class="muted-text">${escapeHtml(user.fullName || "-")}</div>
                </td>
                <td>${escapeHtml(user.groupF || "No group")}</td>
                <td>${escapeHtml((user.rolesF || []).join(", ") || "No roles")}</td>
                <td><span class="badge">${escapeHtml(user.isActiveF || (user.isActive ? "Active" : "Inactive"))}</span></td>
                <td>${escapeHtml(user.scoreF || String(user.score ?? 0))}</td>
                <td>
                    <div class="table-actions">
                        <button class="secondary-button" type="button" data-action="edit-user" data-id="${user.id}">Edit</button>
                        <button class="danger-button" type="button" data-action="delete-user" data-id="${user.id}">Delete</button>
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

                if (action === "edit-group") {
                    editGroup(id);
                } else if (action === "delete-group") {
                    deleteEntity(`/api/groups/${id}`, "group");
                } else if (action === "edit-role") {
                    editRole(id);
                } else if (action === "delete-role") {
                    deleteEntity(`/api/roles/${id}`, "role");
                } else if (action === "edit-user") {
                    editUser(id);
                } else if (action === "delete-user") {
                    deleteEntity(`/api/users/${id}`, "user");
                }
            };
        });
    }

    function editGroup(id) {
        const group = state.groups.find(item => item.id === id);
        if (!group) {
            return;
        }

        elements.groupId.value = String(group.id);
        elements.groupTitle.value = group.title || "";
        elements.groupSubmitButton.textContent = "Update Group";
        elements.groupTitle.focus();
    }

    function editRole(id) {
        const role = state.roles.find(item => item.id === id);
        if (!role) {
            return;
        }

        elements.roleId.value = String(role.id);
        elements.roleName.value = role.name || "";
        elements.roleSubmitButton.textContent = "Update Role";
        elements.roleName.focus();
    }

    function editUser(id) {
        const user = state.users.find(item => item.id === id);
        if (!user) {
            return;
        }

        elements.userId.value = String(user.id);
        elements.userUserName.value = user.userName || "";
        elements.userPassword.value = user.password || "";
        elements.userFirstName.value = user.firstName || "";
        elements.userLastName.value = user.lastName || "";
        elements.userGender.value = String(user.gender || 1);
        elements.userBirthDate.value = normalizeDateInput(user.birthDate);
        elements.userScore.value = user.score ?? 0;
        elements.userAddress.value = user.address || "";
        elements.userCountryId.value = user.countryId ?? "";
        elements.userCityId.value = user.cityId ?? "";
        elements.userGroupId.value = user.groupId ? String(user.groupId) : "";
        elements.userIsActive.checked = Boolean(user.isActive);
        syncRoleSelections(user.roleIds || []);
        elements.userSubmitButton.textContent = "Update User";
        elements.userUserName.focus();
    }

    function resetGroupForm() {
        elements.groupForm.reset();
        elements.groupId.value = "";
        elements.groupSubmitButton.textContent = "Create Group";
    }

    function resetRoleForm() {
        elements.roleForm.reset();
        elements.roleId.value = "";
        elements.roleSubmitButton.textContent = "Create Role";
    }

    function resetUserForm() {
        elements.userForm.reset();
        elements.userId.value = "";
        elements.userGender.value = "1";
        elements.userScore.value = "0";
        elements.userSubmitButton.textContent = "Create User";
        syncRoleSelections([]);
    }

    function syncRoleSelections(selectedIds) {
        const targetIds = new Set(Array.isArray(selectedIds) ? selectedIds : getSelectedRoleIds());
        renderUserFormOptions();
        elements.userRoles.querySelectorAll("input[type='checkbox']").forEach(checkbox => {
            checkbox.checked = targetIds.has(Number(checkbox.value));
        });
    }

    function getSelectedRoleIds() {
        return Array.from(elements.userRoles.querySelectorAll("input[type='checkbox']:checked"))
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
