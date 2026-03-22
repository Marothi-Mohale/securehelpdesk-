const state = {
  token: sessionStorage.getItem("secureHelpdesk.token") || "",
  user: loadJson("secureHelpdesk.user"),
  tickets: [],
  selectedTicketId: null,
  selectedTicket: null,
  agents: [],
  pageNumber: 1,
  pageSize: 10,
  totalPages: 1,
  filters: {
    search: "",
    status: "",
    priority: "",
    sortBy: "CreatedAt",
    sortDirection: "Desc"
  }
};

const elements = {
  loginForm: document.getElementById("login-form"),
  email: document.getElementById("email"),
  password: document.getElementById("password"),
  sessionSummary: document.getElementById("session-summary"),
  logoutButton: document.getElementById("logout-button"),
  refreshButton: document.getElementById("refresh-button"),
  flash: document.getElementById("flash"),
  filterForm: document.getElementById("filter-form"),
  search: document.getElementById("search"),
  statusFilter: document.getElementById("status-filter"),
  priorityFilter: document.getElementById("priority-filter"),
  sortBy: document.getElementById("sort-by"),
  ticketList: document.getElementById("ticket-list"),
  pagination: document.getElementById("pagination"),
  ticketCount: document.getElementById("ticket-count"),
  detailStatus: document.getElementById("detail-status"),
  ticketDetail: document.getElementById("ticket-detail"),
  createTicketForm: document.getElementById("create-ticket-form"),
  createTitle: document.getElementById("create-title"),
  createDescription: document.getElementById("create-description"),
  createPriority: document.getElementById("create-priority")
};

document.querySelectorAll(".credential").forEach((button) => {
  button.addEventListener("click", () => {
    elements.email.value = button.dataset.email ?? "";
    elements.password.value = button.dataset.password ?? "";
  });
});

elements.loginForm.addEventListener("submit", handleLogin);
elements.logoutButton.addEventListener("click", logout);
elements.refreshButton.addEventListener("click", () => loadTickets(true));
elements.filterForm.addEventListener("submit", handleFilterSubmit);
elements.createTicketForm.addEventListener("submit", handleCreateTicket);

initialize();

async function initialize() {
  renderSession();
  syncFiltersToInputs();

  if (!state.token) {
    renderEmptyTickets("Sign in with a demo account to load tickets.");
    renderTicketDetail(null);
    return;
  }

  try {
    await hydrateSession();
    await loadTickets(true);
  } catch (error) {
    logout();
    showFlash(readError(error), true);
  }
}

async function hydrateSession() {
  state.user = await api("/api/auth/me");
  persistSession();

  if (isAdmin()) {
    state.agents = await api("/api/users/agents");
  } else {
    state.agents = [];
  }

  renderSession();
}

async function handleLogin(event) {
  event.preventDefault();

  try {
    const response = await api("/api/auth/login", {
      method: "POST",
      body: {
        email: elements.email.value,
        password: elements.password.value
      },
      authenticated: false
    });

    state.token = response.token;
    state.user = response.user;
    state.selectedTicketId = null;
    state.selectedTicket = null;
    persistSession();

    if (isAdmin()) {
      state.agents = await api("/api/users/agents");
    } else {
      state.agents = [];
    }

    renderSession();
    showFlash(`Signed in as ${state.user.fullName}.`);
    await loadTickets(true);
  } catch (error) {
    showFlash(readError(error), true);
  }
}

function logout() {
  state.token = "";
  state.user = null;
  state.tickets = [];
  state.selectedTicketId = null;
  state.selectedTicket = null;
  state.agents = [];
  state.pageNumber = 1;
  sessionStorage.removeItem("secureHelpdesk.token");
  sessionStorage.removeItem("secureHelpdesk.user");
  renderSession();
  renderEmptyTickets("Sign in with a demo account to load tickets.");
  renderTicketDetail(null);
  showFlash("Signed out.");
}

async function handleFilterSubmit(event) {
  event.preventDefault();
  state.filters.search = elements.search.value.trim();
  state.filters.status = elements.statusFilter.value;
  state.filters.priority = elements.priorityFilter.value;
  state.filters.sortBy = elements.sortBy.value;
  state.filters.sortDirection = "Desc";
  state.pageNumber = 1;
  await loadTickets(true);
}

async function handleCreateTicket(event) {
  event.preventDefault();

  if (!state.token) {
    showFlash("Sign in before creating a ticket.", true);
    return;
  }

  try {
    const ticket = await api("/api/tickets", {
      method: "POST",
      body: {
        title: elements.createTitle.value,
        description: elements.createDescription.value,
        priority: elements.createPriority.value
      }
    });

    elements.createTicketForm.reset();
    elements.createPriority.value = "Medium";
    showFlash("Ticket created successfully.");
    state.selectedTicketId = ticket.id;
    await loadTickets(true);
    await loadTicketDetail(ticket.id);
  } catch (error) {
    showFlash(readError(error), true);
  }
}

async function loadTickets(resetSelection = false) {
  if (!state.token) {
    return;
  }

  try {
    const query = new URLSearchParams({
      pageNumber: String(state.pageNumber),
      pageSize: String(state.pageSize),
      sortBy: state.filters.sortBy,
      sortDirection: state.filters.sortDirection
    });

    appendIfValue(query, "search", state.filters.search);
    appendIfValue(query, "status", state.filters.status);
    appendIfValue(query, "priority", state.filters.priority);

    const response = await api(`/api/tickets?${query.toString()}`);

    state.tickets = response.items;
    state.totalPages = response.totalPages || 1;
    renderTicketList();
    renderPagination();
    elements.ticketCount.textContent = `${response.totalCount} ticket${response.totalCount === 1 ? "" : "s"}`;

    if (state.tickets.length === 0) {
      renderTicketDetail(null);
      return;
    }

    if (resetSelection || !state.selectedTicketId || !state.tickets.some((ticket) => ticket.id === state.selectedTicketId)) {
      state.selectedTicketId = state.tickets[0].id;
    }

    await loadTicketDetail(state.selectedTicketId);
  } catch (error) {
    renderEmptyTickets("Unable to load tickets.");
    renderTicketDetail(null);
    showFlash(readError(error), true);
  }
}

async function loadTicketDetail(ticketId) {
  if (!ticketId || !state.token) {
    renderTicketDetail(null);
    return;
  }

  try {
    state.selectedTicket = await api(`/api/tickets/${ticketId}`);
    state.selectedTicketId = ticketId;
    renderTicketList();
    renderTicketDetail(state.selectedTicket);
  } catch (error) {
    showFlash(readError(error), true);
    renderTicketDetail(null);
  }
}

async function submitComment(ticketId, content) {
  try {
    await api(`/api/tickets/${ticketId}/comments`, {
      method: "POST",
      body: { content }
    });

    showFlash("Comment added.");
    await loadTicketDetail(ticketId);
    await loadTickets(false);
  } catch (error) {
    showFlash(readError(error), true);
  }
}

async function submitAssignment(ticketId, agentUserId) {
  try {
    await api(`/api/tickets/${ticketId}/assignee`, {
      method: "PATCH",
      body: { agentUserId }
    });

    showFlash("Ticket assigned.");
    await loadTickets(false);
    await loadTicketDetail(ticketId);
  } catch (error) {
    showFlash(readError(error), true);
  }
}

function renderSession() {
  if (!state.user) {
    elements.sessionSummary.innerHTML = "<p class=\"muted\">Not signed in.</p>";
    return;
  }

  const roles = (state.user.roles || []).join(", ");
  elements.sessionSummary.innerHTML = `
    <strong>${escapeHtml(state.user.fullName)}</strong>
    <span class="small muted">${escapeHtml(state.user.email)}</span>
    <span class="small">Roles: ${escapeHtml(roles)}</span>
  `;
}

function renderTicketList() {
  if (!state.tickets.length) {
    renderEmptyTickets("No tickets match the current filters.");
    return;
  }

  elements.ticketList.innerHTML = state.tickets.map((ticket) => `
    <article class="ticket-card ${ticket.id === state.selectedTicketId ? "active" : ""}" data-ticket-id="${ticket.id}">
      <h4>${escapeHtml(ticket.title)}</h4>
      <div class="chips">
        <span class="chip">${escapeHtml(ticket.status)}</span>
        <span class="chip">${escapeHtml(ticket.priority)}</span>
      </div>
      <div class="ticket-meta small muted">
        <span>Creator: ${escapeHtml(ticket.createdByName)}</span>
        <span>Assignee: ${escapeHtml(ticket.assignedToName || "Unassigned")}</span>
        <span>Comments: ${ticket.commentCount}</span>
      </div>
    </article>
  `).join("");

  elements.ticketList.querySelectorAll("[data-ticket-id]").forEach((card) => {
    card.addEventListener("click", () => loadTicketDetail(card.dataset.ticketId));
  });
}

function renderEmptyTickets(message) {
  elements.ticketList.innerHTML = `<div class="empty-state">${escapeHtml(message)}</div>`;
  elements.ticketCount.textContent = "0 tickets";
  elements.pagination.innerHTML = "";
}

function renderPagination() {
  if (state.totalPages <= 1) {
    elements.pagination.innerHTML = "";
    return;
  }

  elements.pagination.innerHTML = `
    <button class="ghost" type="button" ${state.pageNumber <= 1 ? "disabled" : ""} data-page="${state.pageNumber - 1}">Previous</button>
    <span class="pill subtle">Page ${state.pageNumber} of ${state.totalPages}</span>
    <button class="ghost" type="button" ${state.pageNumber >= state.totalPages ? "disabled" : ""} data-page="${state.pageNumber + 1}">Next</button>
  `;

  elements.pagination.querySelectorAll("[data-page]").forEach((button) => {
    button.addEventListener("click", async () => {
      state.pageNumber = Number(button.dataset.page);
      await loadTickets(true);
    });
  });
}

function renderTicketDetail(ticket) {
  if (!ticket) {
    elements.detailStatus.textContent = "No selection";
    elements.ticketDetail.innerHTML = `
      <div class="detail-empty">
        <p class="muted">Choose a ticket to view comments, audit history, and assignment controls.</p>
      </div>
    `;
    return;
  }

  elements.detailStatus.textContent = ticket.status;

  const assignmentSection = isAdmin()
    ? `
      <section class="detail-section">
        <h5>Assign Ticket</h5>
        <form id="assignment-form" class="action-row">
          <select id="assignment-agent" ${state.agents.length ? "" : "disabled"}>
            <option value="">Select an agent</option>
            ${state.agents.map((agent) => `
              <option value="${escapeHtml(agent.id)}" ${agent.id === ticket.assignedToUserId ? "selected" : ""}>
                ${escapeHtml(agent.fullName)} (${escapeHtml(agent.email)})
              </option>
            `).join("")}
          </select>
          <button type="submit">Assign</button>
        </form>
      </section>
    `
    : "";

  elements.ticketDetail.innerHTML = `
    <div class="detail-header">
      <h4>${escapeHtml(ticket.title)}</h4>
      <div class="chips">
        <span class="chip">${escapeHtml(ticket.priority)}</span>
        <span class="chip">${escapeHtml(ticket.status)}</span>
      </div>
      <p>${escapeHtml(ticket.description)}</p>
      <div class="detail-meta small muted">
        <span>Creator: ${escapeHtml(ticket.createdByName)}</span>
        <span>Assignee: ${escapeHtml(ticket.assignedToName || "Unassigned")}</span>
        <span>Created: ${formatDate(ticket.createdAtUtc)}</span>
        <span>Updated: ${formatDate(ticket.updatedAtUtc || ticket.createdAtUtc)}</span>
      </div>
    </div>

    ${assignmentSection}

    <section class="detail-section">
      <h5>Add Comment</h5>
      <form id="comment-form" class="stack">
        <textarea id="comment-content" rows="4" maxlength="1000" placeholder="Add a concise, useful update"></textarea>
        <button type="submit">Post Comment</button>
      </form>
    </section>

    <section class="detail-section">
      <h5>Comments</h5>
      <div class="timeline">
        ${ticket.comments.length ? ticket.comments.map((comment) => `
          <article class="timeline-item">
            <div class="comment-head small muted">
              <span>${escapeHtml(comment.authorName)}</span>
              <span>${formatDate(comment.createdAtUtc)}</span>
            </div>
            <p>${escapeHtml(comment.content)}</p>
          </article>
        `).join("") : `<div class="empty-state">No comments yet.</div>`}
      </div>
    </section>

    <section class="detail-section">
      <h5>Audit History</h5>
      <div class="timeline">
        ${ticket.auditLogs.length ? ticket.auditLogs.map((entry) => `
          <article class="timeline-item">
            <div class="audit-head small muted">
              <span>${escapeHtml(entry.changedByName)}</span>
              <span>${escapeHtml(entry.actionType)}</span>
              <span>${formatDate(entry.changedAtUtc)}</span>
            </div>
            <p>${escapeHtml(entry.description)}</p>
          </article>
        `).join("") : `<div class="empty-state">No audit entries found.</div>`}
      </div>
    </section>
  `;

  const commentForm = document.getElementById("comment-form");
  commentForm?.addEventListener("submit", async (event) => {
    event.preventDefault();
    const textarea = document.getElementById("comment-content");
    const content = textarea.value;
    await submitComment(ticket.id, content);
  });

  const assignmentForm = document.getElementById("assignment-form");
  assignmentForm?.addEventListener("submit", async (event) => {
    event.preventDefault();
    const select = document.getElementById("assignment-agent");
    if (!select.value) {
      showFlash("Choose an agent before assigning.", true);
      return;
    }

    await submitAssignment(ticket.id, select.value);
  });
}

async function api(url, options = {}) {
  const request = {
    method: options.method || "GET",
    headers: {
      "Content-Type": "application/json",
      ...(options.headers || {})
    }
  };

  const authenticated = options.authenticated !== false;
  if (authenticated && state.token) {
    request.headers.Authorization = `Bearer ${state.token}`;
  }

  if (options.body !== undefined) {
    request.body = JSON.stringify(options.body);
  }

  const response = await fetch(url, request);
  const isJson = response.headers.get("content-type")?.includes("application/json");
  const payload = isJson ? await response.json() : null;

  if (!response.ok) {
    throw payload || { detail: `Request failed with status ${response.status}.` };
  }

  return payload;
}

function showFlash(message, isError = false) {
  elements.flash.hidden = false;
  elements.flash.classList.toggle("error", isError);
  elements.flash.textContent = message;
  window.clearTimeout(showFlash.timeoutId);
  showFlash.timeoutId = window.setTimeout(() => {
    elements.flash.hidden = true;
  }, 3500);
}

function persistSession() {
  sessionStorage.setItem("secureHelpdesk.token", state.token);
  sessionStorage.setItem("secureHelpdesk.user", JSON.stringify(state.user));
}

function syncFiltersToInputs() {
  elements.search.value = state.filters.search;
  elements.statusFilter.value = state.filters.status;
  elements.priorityFilter.value = state.filters.priority;
  elements.sortBy.value = state.filters.sortBy;
}

function isAdmin() {
  return Array.isArray(state.user?.roles) && state.user.roles.includes("Admin");
}

function readError(error) {
  if (!error) {
    return "An unexpected error occurred.";
  }

  if (error.detail) {
    return error.detail;
  }

  if (typeof error === "string") {
    return error;
  }

  return "An unexpected error occurred.";
}

function appendIfValue(searchParams, key, value) {
  if (value) {
    searchParams.set(key, value);
  }
}

function formatDate(value) {
  if (!value) {
    return "n/a";
  }

  return new Date(value).toLocaleString();
}

function escapeHtml(value) {
  return String(value ?? "")
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll("\"", "&quot;")
    .replaceAll("'", "&#39;");
}

function loadJson(key) {
  const value = sessionStorage.getItem(key);
  if (!value) {
    return null;
  }

  try {
    return JSON.parse(value);
  } catch {
    return null;
  }
}
