const { createApp } = Vue;

const choice = (values) => values.map((value) => ({ label: value, value }));
const maybeChoice = (values) => [{ label: "(Not set)", value: "" }, ...choice(values)];
const boolChoice = [
  { label: "True", value: "true" },
  { label: "False", value: "false" },
];
const maybeBoolChoice = [{ label: "(Not set)", value: "" }, ...boolChoice];

const field = (key, label, type = "text", required = false, defaultValue = "") => ({
  key,
  label,
  type,
  required,
  default: defaultValue,
});

const selectField = (key, label, options, required = false, defaultValue = "", valueType = "string") => ({
  key,
  label,
  type: "select",
  options,
  required,
  default: defaultValue,
  valueType,
});

const now = new Date();
const shiftHours = (hours) => new Date(now.getTime() + hours * 60 * 60 * 1000);
const shiftDays = (days) => new Date(now.getTime() + days * 24 * 60 * 60 * 1000);
const toInputDateTime = (date) => {
  const pad = (value) => value.toString().padStart(2, "0");
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
};

const eventBody = (defaults) => [
  field("calendarId", "Calendar Id", "number", true, defaults.calendarId),
  field("createdByUserId", "Created By User Id", "number", true, defaults.createdByUserId),
  field("title", "Title", "text", true, defaults.title),
  field("description", "Description", "textarea", false, defaults.description),
  field("location", "Location", "text", false, defaults.location),
  field("startUtc", "Start UTC", "datetime", true, defaults.startUtc),
  field("endUtc", "End UTC", "datetime", true, defaults.endUtc),
  selectField("isAllDay", "Is All Day", boolChoice, true, defaults.isAllDay, "boolean"),
  selectField("repeatType", "Repeat Type", choice(["None", "Daily", "Weekly", "Monthly"]), true, defaults.repeatType),
  field("reminderMinutesBefore", "Reminder Minutes Before", "number", false, defaults.reminderMinutesBefore),
  selectField("status", "Status", choice(["Planned", "Cancelled"]), true, defaults.status),
];

const taskBody = (defaults) => [
  field("calendarId", "Calendar Id", "number", true, defaults.calendarId),
  field("createdByUserId", "Created By User Id", "number", true, defaults.createdByUserId),
  field("title", "Title", "text", true, defaults.title),
  field("description", "Description", "textarea", false, defaults.description),
  field("dueUtc", "Due UTC", "datetime", false, defaults.dueUtc),
  selectField("priority", "Priority", choice(["Low", "Medium", "High"]), true, defaults.priority),
  selectField("status", "Status", choice(["Todo", "InProgress", "Done"]), true, defaults.status),
  field("completedAtUtc", "Completed At UTC", "datetime", false, defaults.completedAtUtc),
  field("reminderMinutesBefore", "Reminder Minutes Before", "number", false, defaults.reminderMinutesBefore),
];

const sessionBody = (defaults) => [
  field("calendarId", "Calendar Id", "number", true, defaults.calendarId),
  field("createdByUserId", "Created By User Id", "number", true, defaults.createdByUserId),
  field("studentName", "Student Name", "text", true, defaults.studentName),
  field("studentContact", "Student Contact", "text", false, defaults.studentContact),
  field("sessionStartUtc", "Session Start UTC", "datetime", true, defaults.sessionStartUtc),
  field("sessionEndUtc", "Session End UTC", "datetime", true, defaults.sessionEndUtc),
  field("topicPlanned", "Topic Planned", "textarea", false, defaults.topicPlanned),
  field("topicDone", "Topic Done", "textarea", false, defaults.topicDone),
  field("homeworkAssigned", "Homework Assigned", "textarea", false, defaults.homeworkAssigned),
  field("priceAmount", "Price Amount", "decimal", true, defaults.priceAmount),
  field("currencyCode", "Currency Code", "text", true, defaults.currencyCode),
  selectField("isPaid", "Is Paid", boolChoice, true, defaults.isPaid, "boolean"),
  field("paidAtUtc", "Paid At UTC", "datetime", false, defaults.paidAtUtc),
  selectField("paymentMethod", "Payment Method", maybeChoice(["Cash", "Card", "Transfer"]), false, defaults.paymentMethod),
  field("paymentNote", "Payment Note", "textarea", false, defaults.paymentNote),
  selectField("status", "Status", choice(["Scheduled", "Completed", "Cancelled", "NoShow"]), true, defaults.status),
];

const op = (id, title, method, pathTemplate, description, pathParams = [], queryParams = [], bodyParams = []) => ({
  id,
  title,
  method,
  pathTemplate,
  description,
  pathParams,
  queryParams,
  bodyParams,
});

const endpointGroups = [
  {
    key: "users",
    title: "Users",
    description: "Registration, login, profile updates, and password operations.",
    endpoints: [
      op("users-get-all", "Get All Users", "GET", "/api/users", "Returns all users."),
      op("users-get-by-id", "Get User By Id", "GET", "/api/users/{id}", "Returns one user or 404.", [field("id", "User Id", "number", true, "1")]),
      op("users-register", "Register User", "POST", "/api/users/register", "Creates a new active user.", [], [], [
        field("email", "Email", "email", true, "student@example.com"),
        field("password", "Password", "password", true, "Pass123!"),
        field("firstName", "First Name", "text", true, "Student"),
        field("lastName", "Last Name", "text", true, "Demo"),
        field("timeZoneId", "Time Zone Id", "text", false, "UTC"),
      ]),
      op("users-login", "Login User", "POST", "/api/users/login", "Validates credentials.", [], [], [
        field("email", "Email", "email", true, "ana@example.com"),
        field("password", "Password", "password", true, "Pass123!"),
      ]),
      op("users-update", "Update User", "PUT", "/api/users/{id}", "Updates profile + active flag.", [field("id", "User Id", "number", true, "1")], [], [
        field("email", "Email", "email", true, "ana@example.com"),
        field("firstName", "First Name", "text", true, "Ana"),
        field("lastName", "Last Name", "text", true, "Ilic"),
        field("timeZoneId", "Time Zone Id", "text", false, "UTC"),
        selectField("isActive", "Is Active", boolChoice, true, "true", "boolean"),
      ]),
      op("users-change-password", "Change Password", "PUT", "/api/users/{id}/password", "Changes password for existing user.", [field("id", "User Id", "number", true, "1")], [], [
        field("currentPassword", "Current Password", "password", true, "Pass123!"),
        field("newPassword", "New Password", "password", true, "Pass123!1"),
      ]),
      op("users-delete", "Delete User", "DELETE", "/api/users/{id}", "Deletes user by id.", [field("id", "User Id", "number", true, "1")]),
    ],
  },
  {
    key: "calendars",
    title: "Calendars",
    description: "CRUD operations for calendar entities.",
    endpoints: [
      op("calendars-get-all", "Get Calendars", "GET", "/api/calendars", "List calendars with optional owner filter.", [], [field("ownerUserId", "Owner User Id", "number", false, "1")]),
      op("calendars-get-by-id", "Get Calendar By Id", "GET", "/api/calendars/{id}", "Returns single calendar.", [field("id", "Calendar Id", "number", true, "1")]),
      op("calendars-create", "Create Calendar", "POST", "/api/calendars", "Creates calendar.", [], [], [
        field("ownerUserId", "Owner User Id", "number", true, "1"),
        field("name", "Name", "text", true, "Semester Planner"),
        field("description", "Description", "textarea", false, "Calendar for test scenarios."),
        field("colorHex", "Color Hex", "text", true, "#0F766E"),
        selectField("isDefault", "Is Default", boolChoice, true, "false", "boolean"),
      ]),
      op("calendars-update", "Update Calendar", "PUT", "/api/calendars/{id}", "Updates calendar values.", [field("id", "Calendar Id", "number", true, "1")], [], [
        field("ownerUserId", "Owner User Id", "number", true, "1"),
        field("name", "Name", "text", true, "Main Calendar"),
        field("description", "Description", "textarea", false, "Default personal calendar."),
        field("colorHex", "Color Hex", "text", true, "#2563EB"),
        selectField("isDefault", "Is Default", boolChoice, true, "true", "boolean"),
      ]),
      op("calendars-delete", "Delete Calendar", "DELETE", "/api/calendars/{id}", "Deletes by id.", [field("id", "Calendar Id", "number", true, "1")]),
    ],
  },
  {
    key: "events",
    title: "Events",
    description: "Event CRUD with filters and status/repeat controls.",
    endpoints: [
      op("events-get-all", "Get Events", "GET", "/api/events", "Filter by calendar and date range.", [], [
        field("calendarId", "Calendar Id", "number", false, "1"),
        field("fromUtc", "From UTC", "datetime", false, toInputDateTime(shiftDays(-1))),
        field("toUtc", "To UTC", "datetime", false, toInputDateTime(shiftDays(7))),
      ]),
      op("events-get-by-id", "Get Event By Id", "GET", "/api/events/{id}", "Returns event by id.", [field("id", "Event Id", "number", true, "1")]),
      op("events-create", "Create Event", "POST", "/api/events", "Creates new event.", [], [], eventBody({
        calendarId: "1",
        createdByUserId: "1",
        title: "Project Sync Meeting",
        description: "Plan testing milestones and endpoint coverage.",
        location: "Lab 3",
        startUtc: toInputDateTime(shiftHours(2)),
        endUtc: toInputDateTime(shiftHours(3)),
        isAllDay: "false",
        repeatType: "None",
        reminderMinutesBefore: "30",
        status: "Planned",
      })),
      op("events-update", "Update Event", "PUT", "/api/events/{id}", "Updates existing event.", [field("id", "Event Id", "number", true, "1")], [], eventBody({
        calendarId: "1",
        createdByUserId: "1",
        title: "Updated Study Session",
        description: "Updated event details for QA demo.",
        location: "Home Office",
        startUtc: toInputDateTime(shiftHours(5)),
        endUtc: toInputDateTime(shiftHours(6)),
        isAllDay: "false",
        repeatType: "Weekly",
        reminderMinutesBefore: "45",
        status: "Planned",
      })),
      op("events-delete", "Delete Event", "DELETE", "/api/events/{id}", "Deletes event.", [field("id", "Event Id", "number", true, "1")]),
    ],
  },
  {
    key: "tasks",
    title: "Tasks",
    description: "Task CRUD and status updates.",
    endpoints: [
      op("tasks-get-all", "Get Tasks", "GET", "/api/tasks", "Filter by calendar, status, due date.", [], [
        field("calendarId", "Calendar Id", "number", false, "1"),
        selectField("status", "Status", maybeChoice(["Todo", "InProgress", "Done"]), false, ""),
        field("dueBeforeUtc", "Due Before UTC", "datetime", false, toInputDateTime(shiftDays(4))),
      ]),
      op("tasks-get-by-id", "Get Task By Id", "GET", "/api/tasks/{id}", "Returns task by id.", [field("id", "Task Id", "number", true, "1")]),
      op("tasks-create", "Create Task", "POST", "/api/tasks", "Creates task.", [], [], taskBody({
        calendarId: "1",
        createdByUserId: "1",
        title: "Prepare Playwright scenarios",
        description: "Cover all CRUD operations from the brief.",
        dueUtc: toInputDateTime(shiftDays(2)),
        priority: "High",
        status: "Todo",
        completedAtUtc: "",
        reminderMinutesBefore: "60",
      })),
      op("tasks-update", "Update Task", "PUT", "/api/tasks/{id}", "Updates task.", [field("id", "Task Id", "number", true, "1")], [], taskBody({
        calendarId: "1",
        createdByUserId: "1",
        title: "Write NUnit + Playwright tests",
        description: "Updated task body from Vue client.",
        dueUtc: toInputDateTime(shiftDays(3)),
        priority: "Medium",
        status: "InProgress",
        completedAtUtc: "",
        reminderMinutesBefore: "45",
      })),
      op("tasks-update-status", "Update Task Status", "PUT", "/api/tasks/{id}/status", "Updates only status.", [field("id", "Task Id", "number", true, "1")], [], [
        selectField("status", "Status", choice(["Todo", "InProgress", "Done"]), true, "Done"),
        field("completedAtUtc", "Completed At UTC", "datetime", false, toInputDateTime(shiftHours(-1))),
      ]),
      op("tasks-delete", "Delete Task", "DELETE", "/api/tasks/{id}", "Deletes task by id.", [field("id", "Task Id", "number", true, "1")]),
    ],
  },
  {
    key: "private-class-sessions",
    title: "Private Class Sessions",
    description: "CRUD sessions plus unpaid and monthly summary.",
    endpoints: [
      op("sessions-get-all", "Get Sessions", "GET", "/api/private-class-sessions", "Filter by calendar, payment, and range.", [], [
        field("calendarId", "Calendar Id", "number", false, "2"),
        selectField("isPaid", "Is Paid", maybeBoolChoice, false, "", "boolean"),
        field("fromUtc", "From UTC", "datetime", false, toInputDateTime(shiftDays(-7))),
        field("toUtc", "To UTC", "datetime", false, toInputDateTime(shiftDays(7))),
      ]),
      op("sessions-get-by-id", "Get Session By Id", "GET", "/api/private-class-sessions/{id}", "Returns one session.", [field("id", "Session Id", "number", true, "1")]),
      op("sessions-get-unpaid", "Get Unpaid Sessions", "GET", "/api/private-class-sessions/unpaid", "Returns unpaid sessions.", [], [field("calendarId", "Calendar Id", "number", false, "2")]),
      op("sessions-monthly-summary", "Get Monthly Summary", "GET", "/api/private-class-sessions/monthly-summary", "Returns monthly payment summary.", [], [
        field("calendarId", "Calendar Id", "number", true, "2"),
        field("year", "Year", "number", true, now.getFullYear().toString()),
        field("month", "Month", "number", true, (now.getMonth() + 1).toString()),
      ]),
      op("sessions-create", "Create Session", "POST", "/api/private-class-sessions", "Creates private session.", [], [], sessionBody({
        calendarId: "2",
        createdByUserId: "1",
        studentName: "Test Student",
        studentContact: "student@mail.com",
        sessionStartUtc: toInputDateTime(shiftHours(1)),
        sessionEndUtc: toInputDateTime(shiftHours(2)),
        topicPlanned: "Vue CRUD forms and Playwright scripts.",
        topicDone: "",
        homeworkAssigned: "Add edge-case API tests.",
        priceAmount: "2500.00",
        currencyCode: "RSD",
        isPaid: "false",
        paidAtUtc: "",
        paymentMethod: "",
        paymentNote: "",
        status: "Scheduled",
      })),
      op("sessions-update", "Update Session", "PUT", "/api/private-class-sessions/{id}", "Updates session.", [field("id", "Session Id", "number", true, "1")], [], sessionBody({
        calendarId: "2",
        createdByUserId: "1",
        studentName: "Marko Markovic",
        studentContact: "marko@mail.com",
        sessionStartUtc: toInputDateTime(shiftHours(3)),
        sessionEndUtc: toInputDateTime(shiftHours(4)),
        topicPlanned: "Mock interviews and SQL joins.",
        topicDone: "Worked through practical tasks.",
        homeworkAssigned: "Finish three endpoint test specs.",
        priceAmount: "2000.00",
        currencyCode: "RSD",
        isPaid: "true",
        paidAtUtc: toInputDateTime(shiftHours(4)),
        paymentMethod: "Cash",
        paymentNote: "Paid immediately after class.",
        status: "Completed",
      })),
      op("sessions-mark-paid", "Mark Session Paid", "PUT", "/api/private-class-sessions/{id}/mark-paid", "Marks selected session as paid.", [field("id", "Session Id", "number", true, "1")], [], [
        selectField("paymentMethod", "Payment Method", maybeChoice(["Cash", "Card", "Transfer"]), false, "Card"),
        field("paymentNote", "Payment Note", "textarea", false, "Paid via card terminal."),
        field("paidAtUtc", "Paid At UTC", "datetime", false, toInputDateTime(now)),
      ]),
      op("sessions-mark-unpaid", "Mark Session Unpaid", "PUT", "/api/private-class-sessions/{id}/mark-unpaid", "Clears payment data.", [field("id", "Session Id", "number", true, "1")]),
      op("sessions-delete", "Delete Session", "DELETE", "/api/private-class-sessions/{id}", "Deletes session by id.", [field("id", "Session Id", "number", true, "1")]),
    ],
  },
];

const defaultBaseUrl = () => {
  const servedViaHttp = window.location.protocol === "http:" || window.location.protocol === "https:";
  return servedViaHttp ? window.location.origin : "http://localhost:5170";
};

const parseFieldValue = (raw, definition) => {
  const text = typeof raw === "string" ? raw.trim() : raw;
  const isEmpty = text === "" || text === null || text === undefined;
  if (isEmpty) {
    if (definition.required) {
      throw new Error(`${definition.label} is required.`);
    }
    return { include: false, value: null };
  }

  if (definition.type === "number") {
    const num = Number(text);
    if (!Number.isFinite(num) || !Number.isInteger(num)) {
      throw new Error(`${definition.label} must be a whole number.`);
    }
    return { include: true, value: num };
  }

  if (definition.type === "decimal") {
    const num = Number(text);
    if (!Number.isFinite(num)) {
      throw new Error(`${definition.label} must be numeric.`);
    }
    return { include: true, value: num };
  }

  if (definition.type === "datetime") {
    const date = new Date(text);
    if (Number.isNaN(date.getTime())) {
      throw new Error(`${definition.label} must be valid date-time.`);
    }
    return { include: true, value: date.toISOString() };
  }

  if (definition.type === "select") {
    if (definition.valueType === "boolean") {
      return { include: true, value: text === "true" };
    }
    if (definition.valueType === "number") {
      const num = Number(text);
      if (!Number.isFinite(num)) {
        throw new Error(`${definition.label} must be numeric.`);
      }
      return { include: true, value: num };
    }
  }

  return { include: true, value: text };
};

const makeInitialState = () => {
  const state = {};
  for (const group of endpointGroups) {
    for (const endpoint of group.endpoints) {
      state[endpoint.id] = { path: {}, query: {}, body: {} };
      for (const p of endpoint.pathParams) state[endpoint.id].path[p.key] = p.default ?? "";
      for (const q of endpoint.queryParams) state[endpoint.id].query[q.key] = q.default ?? "";
      for (const b of endpoint.bodyParams) state[endpoint.id].body[b.key] = b.default ?? "";
    }
  }
  return state;
};

const initialFormState = makeInitialState();
const clone = (value) => JSON.parse(JSON.stringify(value));
const normalizeBaseUrl = (value) => {
  const trimmed = value.trim().replace(/\/+$/, "");
  if (!/^https?:\/\//i.test(trimmed)) {
    throw new Error("Base URL must start with http:// or https://");
  }
  return trimmed;
};

const formatPayload = (payload) => {
  if (payload === null || payload === undefined) return "null";
  if (typeof payload === "string") return payload;
  return JSON.stringify(payload, null, 2);
};

const emptyResponse = () => ({
  operation: "-",
  method: "-",
  url: "-",
  status: null,
  statusText: "No request yet",
  durationMs: 0,
  prettyBody: '{\n  "message": "Run an API operation to inspect output."\n}',
});

createApp({
  data() {
    const base = defaultBaseUrl();
    return {
      endpointGroups,
      baseUrl: base,
      defaultBaseUrl: base,
      activeGroupKey: endpointGroups[0].key,
      searchText: "",
      formState: clone(initialFormState),
      loadingEndpointId: null,
      response: emptyResponse(),
    };
  },
  computed: {
    activeGroup() {
      return this.endpointGroups.find((group) => group.key === this.activeGroupKey);
    },
    filteredEndpoints() {
      if (!this.activeGroup) return [];
      const query = this.searchText.trim().toLowerCase();
      if (!query) return this.activeGroup.endpoints;
      return this.activeGroup.endpoints.filter((endpoint) => {
        const haystack = `${endpoint.method} ${endpoint.title} ${endpoint.pathTemplate}`.toLowerCase();
        return haystack.includes(query);
      });
    },
    statusClass() {
      if (this.response.status === null || this.response.status === 0) return "";
      return this.response.status >= 200 && this.response.status < 300 ? "status--ok" : "status--error";
    },
  },
  methods: {
    resolveInputType(type) {
      if (type === "datetime") return "datetime-local";
      if (type === "decimal") return "number";
      if (type === "textarea") return "text";
      return type;
    },
    useCurrentOrigin() {
      if (window.location.protocol === "http:" || window.location.protocol === "https:") {
        this.baseUrl = window.location.origin;
      }
    },
    resetBaseUrl() {
      this.baseUrl = this.defaultBaseUrl;
    },
    resetEndpoint(endpointId) {
      this.formState[endpointId] = clone(initialFormState[endpointId]);
    },
    buildPath(endpoint, pathState) {
      let path = endpoint.pathTemplate;
      for (const definition of endpoint.pathParams) {
        const parsed = parseFieldValue(pathState[definition.key], definition);
        if (!parsed.include) {
          throw new Error(`${definition.label} is required.`);
        }
        path = path.replace(`{${definition.key}}`, encodeURIComponent(String(parsed.value)));
      }
      return path;
    },
    buildQuery(endpoint, queryState) {
      const params = new URLSearchParams();
      for (const definition of endpoint.queryParams) {
        const parsed = parseFieldValue(queryState[definition.key], definition);
        if (parsed.include) params.set(definition.key, String(parsed.value));
      }
      const qs = params.toString();
      return qs ? `?${qs}` : "";
    },
    buildBody(endpoint, bodyState) {
      if (!endpoint.bodyParams.length) return undefined;
      const payload = {};
      for (const definition of endpoint.bodyParams) {
        const parsed = parseFieldValue(bodyState[definition.key], definition);
        if (parsed.include) payload[definition.key] = parsed.value;
      }
      return payload;
    },
    async runEndpoint(endpoint) {
      this.loadingEndpointId = endpoint.id;
      const started = performance.now();
      try {
        const localState = this.formState[endpoint.id];
        const url = `${normalizeBaseUrl(this.baseUrl)}${this.buildPath(endpoint, localState.path)}${this.buildQuery(endpoint, localState.query)}`;
        const body = this.buildBody(endpoint, localState.body);
        const requestInit = { method: endpoint.method, headers: {} };
        if (body !== undefined) {
          requestInit.headers["Content-Type"] = "application/json";
          requestInit.body = JSON.stringify(body);
        }

        const response = await fetch(url, requestInit);
        const durationMs = Math.round(performance.now() - started);
        const textBody = await response.text();
        let parsedBody = { message: "No response body (likely 204 No Content)." };
        if (textBody) {
          try {
            parsedBody = JSON.parse(textBody);
          } catch {
            parsedBody = textBody;
          }
        }

        this.response = {
          operation: endpoint.title,
          method: endpoint.method,
          url,
          status: response.status,
          statusText: `${response.status} ${response.statusText}`,
          durationMs,
          prettyBody: formatPayload(parsedBody),
        };
      } catch (error) {
        const durationMs = Math.round(performance.now() - started);
        this.response = {
          operation: endpoint.title,
          method: endpoint.method,
          url: "-",
          status: 0,
          statusText: "Client validation/request error",
          durationMs,
          prettyBody: formatPayload({ error: error.message }),
        };
      } finally {
        this.loadingEndpointId = null;
      }
    },
  },
}).mount("#app");
