const { createApp } = Vue;

const USER_STORAGE_KEY = "calendar_tasking_user_v1";

const toInputDateTime = (value) => {
  if (!value) return "";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "";
  const pad = (part) => String(part).padStart(2, "0");
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
};

const toIsoOrNull = (value) => {
  if (!value) return null;
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return null;
  return date.toISOString();
};

const currentMonthValue = () => {
  const date = new Date();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  return `${date.getFullYear()}-${month}`;
};

const emptyCalendarForm = () => ({
  calendarId: null,
  name: "",
  description: "",
  colorHex: "#157A6E",
  isDefault: false,
});

const emptyTaskForm = (calendarId = null) => ({
  taskItemId: null,
  calendarId,
  title: "",
  description: "",
  dueLocal: "",
  priority: "Medium",
  status: "Todo",
  reminderMinutesBefore: "",
});

const now = new Date();
const nextHour = new Date(now.getTime() + 60 * 60 * 1000);
const twoHoursLater = new Date(now.getTime() + 2 * 60 * 60 * 1000);

const emptyEventForm = (calendarId = null) => ({
  eventId: null,
  calendarId,
  title: "",
  description: "",
  location: "",
  startLocal: toInputDateTime(nextHour),
  endLocal: toInputDateTime(twoHoursLater),
  isAllDay: false,
  repeatType: "None",
  reminderMinutesBefore: "",
  status: "Planned",
});

const emptySessionForm = (calendarId = null) => ({
  privateClassSessionId: null,
  calendarId,
  studentName: "",
  studentContact: "",
  sessionStartLocal: toInputDateTime(nextHour),
  sessionEndLocal: toInputDateTime(twoHoursLater),
  topicPlanned: "",
  topicDone: "",
  homeworkAssigned: "",
  priceAmount: "0",
  currencyCode: "RSD",
  isPaid: false,
  paymentMethod: "",
  paymentNote: "",
  status: "Scheduled",
});

createApp({
  data() {
    return {
      pages: [
        { key: "overview", label: "Overview" },
        { key: "calendars", label: "Calendars" },
        { key: "tasks", label: "Tasks" },
        { key: "events", label: "Events" },
        { key: "sessions", label: "Sessions" },
      ],
      activePage: "overview",
      authMode: "login",
      user: null,
      loginForm: {
        email: "ana@example.com",
        password: "Pass123!",
      },
      registerForm: {
        email: "",
        password: "",
        firstName: "",
        lastName: "",
        timeZoneId: "UTC",
      },
      calendars: [],
      selectedCalendarId: null,
      tasks: [],
      events: [],
      sessions: [],
      monthlySummary: null,
      summaryMonth: currentMonthValue(),
      taskFilter: {
        status: "",
      },
      calendarForm: emptyCalendarForm(),
      taskForm: emptyTaskForm(),
      eventForm: emptyEventForm(),
      sessionForm: emptySessionForm(),
      busy: {
        auth: false,
        calendars: false,
        tasks: false,
        events: false,
        sessions: false,
      },
      toasts: [],
      sidebarOpen: false,
      calViewDate: new Date(),
      selectedDay: null,
    };
  },
  computed: {
    isAuthenticated() {
      return !!this.user;
    },
    userDisplayName() {
      if (!this.user) return "";
      if (this.user.fullName) return this.user.fullName;
      const candidate = `${this.user.firstName || ""} ${this.user.lastName || ""}`.trim();
      return candidate || this.user.email;
    },
    activeCalendarName() {
      const calendar = this.calendars.find((item) => item.calendarId === this.selectedCalendarId);
      return calendar ? calendar.name : "";
    },
    filteredTasks() {
      let list = [...this.tasks];
      if (this.taskFilter.status) {
        list = list.filter((task) => task.status === this.taskFilter.status);
      }

      list.sort((a, b) => {
        const first = a.dueUtc ? new Date(a.dueUtc).getTime() : Number.MAX_SAFE_INTEGER;
        const second = b.dueUtc ? new Date(b.dueUtc).getTime() : Number.MAX_SAFE_INTEGER;
        return first - second;
      });

      return list;
    },
    upcomingEvents() {
      const nowValue = Date.now();
      return [...this.events]
        .filter((event) => new Date(event.startUtc).getTime() >= nowValue)
        .sort((a, b) => new Date(a.startUtc).getTime() - new Date(b.startUtc).getTime())
        .slice(0, 6);
    },
    dueSoonTasks() {
      const nowValue = Date.now();
      const oneWeek = nowValue + 7 * 24 * 60 * 60 * 1000;
      return this.tasks
        .filter((task) => task.status !== "Done" && task.dueUtc && new Date(task.dueUtc).getTime() <= oneWeek)
        .sort((a, b) => new Date(a.dueUtc).getTime() - new Date(b.dueUtc).getTime())
        .slice(0, 6);
    },
    unpaidSessionsCount() {
      return this.sessions.filter((session) => !session.isPaid).length;
    },
    calViewMonth() {
      return this.calViewDate.getMonth();
    },
    calViewYear() {
      return this.calViewDate.getFullYear();
    },
    calViewLabel() {
      return this.calViewDate.toLocaleString("en-US", { month: "long", year: "numeric" });
    },
    calendarDays() {
      const year = this.calViewYear;
      const month = this.calViewMonth;
      const firstDay = new Date(year, month, 1);
      const lastDay = new Date(year, month + 1, 0);
      const startDow = (firstDay.getDay() + 6) % 7;
      const totalDays = lastDay.getDate();

      const today = new Date();
      const todayStr = `${today.getFullYear()}-${String(today.getMonth() + 1).padStart(2, "0")}-${String(today.getDate()).padStart(2, "0")}`;

      const days = [];

      for (let i = 0; i < startDow; i++) {
        const d = new Date(year, month, -startDow + i + 1);
        days.push({ date: d, day: d.getDate(), outside: true, isToday: false, key: this._dateKey(d) });
      }

      for (let d = 1; d <= totalDays; d++) {
        const date = new Date(year, month, d);
        const key = this._dateKey(date);
        days.push({ date, day: d, outside: false, isToday: key === todayStr, key });
      }

      const remaining = 7 - (days.length % 7);
      if (remaining < 7) {
        for (let i = 1; i <= remaining; i++) {
          const d = new Date(year, month + 1, i);
          days.push({ date: d, day: d.getDate(), outside: true, isToday: false, key: this._dateKey(d) });
        }
      }

      return days;
    },
    calendarEventMap() {
      const map = {};
      for (const ev of this.events) {
        const key = this._dateKey(new Date(ev.startUtc));
        if (!map[key]) map[key] = [];
        map[key].push({ title: ev.title, type: "event", color: "var(--neon-cyan)" });
      }
      for (const task of this.tasks) {
        if (!task.dueUtc) continue;
        const key = this._dateKey(new Date(task.dueUtc));
        if (!map[key]) map[key] = [];
        const color = task.status === "Done" ? "var(--neon-green)" : task.priority === "High" ? "var(--neon-red)" : "var(--neon-yellow)";
        map[key].push({ title: task.title, type: "task", color });
      }
      for (const s of this.sessions) {
        const key = this._dateKey(new Date(s.sessionStartUtc));
        if (!map[key]) map[key] = [];
        map[key].push({ title: s.studentName, type: "session", color: "var(--neon-magenta)" });
      }
      return map;
    },
    selectedDayLabel() {
      if (!this.selectedDay) return "";
      return this.selectedDay.date.toLocaleDateString("en-US", {
        weekday: "long",
        year: "numeric",
        month: "long",
        day: "numeric",
      });
    },
    dayViewHours() {
      if (!this.selectedDay) return [];
      const dayKey = this.selectedDay.key;
      const allItems = [];

      for (const ev of this.events) {
        const start = new Date(ev.startUtc);
        const end = new Date(ev.endUtc);
        if (this._dateKey(start) === dayKey) {
          const durMin = Math.round((end - start) / 60000);
          allItems.push({
            title: ev.title,
            type: "Event",
            color: "var(--neon-cyan)",
            chipClass: "cyan",
            startHour: start.getHours(),
            durationSlots: Math.max(1, Math.ceil(durMin / 60)),
            timeLabel: this._fmtTime(start) + " \u2013 " + this._fmtTime(end),
            location: ev.location || "",
          });
        }
      }

      for (const task of this.tasks) {
        if (!task.dueUtc) continue;
        const due = new Date(task.dueUtc);
        if (this._dateKey(due) !== dayKey) continue;
        const color = task.status === "Done" ? "var(--neon-green)" : task.priority === "High" ? "var(--neon-red)" : "var(--neon-yellow)";
        const chipClass = task.status === "Done" ? "green" : task.priority === "High" ? "red" : "yellow";
        allItems.push({
          title: task.title,
          type: "Task",
          color,
          chipClass,
          startHour: due.getHours(),
          durationSlots: 1,
          timeLabel: this._fmtTime(due) + " due",
          location: "",
        });
      }

      for (const s of this.sessions) {
        const start = new Date(s.sessionStartUtc);
        const end = new Date(s.sessionEndUtc);
        if (this._dateKey(start) !== dayKey) continue;
        const durMin = Math.round((end - start) / 60000);
        allItems.push({
          title: s.studentName,
          type: "Session",
          color: "var(--neon-magenta)",
          chipClass: "magenta",
          startHour: start.getHours(),
          durationSlots: Math.max(1, Math.ceil(durMin / 60)),
          timeLabel: this._fmtTime(start) + " \u2013 " + this._fmtTime(end),
          location: s.topicPlanned || "",
        });
      }

      const hours = [];
      for (let h = 0; h < 24; h++) {
        hours.push({
          hour: h,
          label: String(h).padStart(2, "0") + ":00",
          items: allItems.filter((item) => item.startHour === h),
        });
      }
      return hours;
    },
    dayViewHasItems() {
      return this.dayViewHours.some((h) => h.items.length > 0);
    },
  },
  watch: {
    async selectedCalendarId(newValue, oldValue) {
      if (!this.isAuthenticated || !newValue || newValue === oldValue) return;
      this.taskForm.calendarId = newValue;
      this.eventForm.calendarId = newValue;
      this.sessionForm.calendarId = newValue;
      await this.refreshCalendarScopedData();
    },
  },
  async mounted() {
    this.restoreUserFromStorage();
    if (this.user) {
      await this.initializeWorkspace();
    }
  },
  methods: {
    toggleSidebar() {
      this.sidebarOpen = !this.sidebarOpen;
    },
    _dateKey(d) {
      return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}-${String(d.getDate()).padStart(2, "0")}`;
    },
    _fmtTime(d) {
      return String(d.getHours()).padStart(2, "0") + ":" + String(d.getMinutes()).padStart(2, "0");
    },
    selectDay(cell) {
      if (cell.outside) return;
      this.selectedDay = this.selectedDay && this.selectedDay.key === cell.key ? null : cell;
    },
    closeDayView() {
      this.selectedDay = null;
    },
    calPrev() {
      this.calViewDate = new Date(this.calViewYear, this.calViewMonth - 1, 1);
      this.selectedDay = null;
    },
    calNext() {
      this.calViewDate = new Date(this.calViewYear, this.calViewMonth + 1, 1);
      this.selectedDay = null;
    },
    calToday() {
      this.calViewDate = new Date();
      this.selectedDay = null;
    },
    getItemsForDay(dayKey) {
      return this.calendarEventMap[dayKey] || [];
    },
    addToast(message, type = "info") {
      const toast = {
        id: `${Date.now()}-${Math.random()}`,
        message,
        type,
      };
      this.toasts.push(toast);
      window.setTimeout(() => {
        this.toasts = this.toasts.filter((item) => item.id !== toast.id);
      }, 3200);
    },
    async apiRequest(path, { method = "GET", body } = {}) {
      const requestInit = {
        method,
        headers: {},
      };

      if (body !== undefined) {
        requestInit.headers["Content-Type"] = "application/json";
        requestInit.body = JSON.stringify(body);
      }

      const response = await fetch(`${window.location.origin}${path}`, requestInit);
      const textBody = await response.text();
      let payload = null;
      if (textBody) {
        try {
          payload = JSON.parse(textBody);
        } catch {
          payload = textBody;
        }
      }

      if (!response.ok) {
        let message = `${response.status} ${response.statusText}`;
        if (typeof payload === "string" && payload.trim()) {
          message = payload;
        } else if (payload && typeof payload === "object") {
          message = payload.title || payload.message || payload.error || message;
        }
        throw new Error(message);
      }

      return payload;
    },
    persistUser() {
      if (!this.user) return;
      localStorage.setItem(USER_STORAGE_KEY, JSON.stringify(this.user));
    },
    restoreUserFromStorage() {
      try {
        const raw = localStorage.getItem(USER_STORAGE_KEY);
        if (!raw) return;
        const parsed = JSON.parse(raw);
        if (parsed && typeof parsed.userId === "number") {
          this.user = parsed;
        }
      } catch {
        localStorage.removeItem(USER_STORAGE_KEY);
      }
    },
    async activateUser(loginResponse) {
      const baseUser = {
        userId: loginResponse.userId,
        email: loginResponse.email,
        fullName: loginResponse.fullName,
      };

      try {
        const details = await this.apiRequest(`/api/users/${loginResponse.userId}`);
        this.user = {
          ...baseUser,
          firstName: details.firstName,
          lastName: details.lastName,
          timeZoneId: details.timeZoneId,
        };
      } catch {
        this.user = baseUser;
      }

      this.persistUser();
      await this.initializeWorkspace();
    },
    async submitLogin() {
      this.busy.auth = true;
      try {
        const response = await this.apiRequest("/api/users/login", {
          method: "POST",
          body: {
            email: this.loginForm.email,
            password: this.loginForm.password,
          },
        });
        await this.activateUser(response);
        this.addToast("Welcome back.", "success");
      } catch (error) {
        this.addToast(error.message, "error");
      } finally {
        this.busy.auth = false;
      }
    },
    async submitRegister() {
      this.busy.auth = true;
      try {
        const credentials = {
          email: this.registerForm.email,
          password: this.registerForm.password,
        };

        await this.apiRequest("/api/users/register", {
          method: "POST",
          body: {
            email: this.registerForm.email,
            password: this.registerForm.password,
            firstName: this.registerForm.firstName,
            lastName: this.registerForm.lastName,
            timeZoneId: this.registerForm.timeZoneId || "UTC",
          },
        });

        const loginResponse = await this.apiRequest("/api/users/login", {
          method: "POST",
          body: credentials,
        });

        await this.activateUser(loginResponse);
        this.registerForm = {
          email: "",
          password: "",
          firstName: "",
          lastName: "",
          timeZoneId: "UTC",
        };
        this.addToast("Account created and signed in.", "success");
      } catch (error) {
        this.addToast(error.message, "error");
      } finally {
        this.busy.auth = false;
      }
    },
    logout() {
      this.user = null;
      localStorage.removeItem(USER_STORAGE_KEY);
      this.activePage = "overview";
      this.calendars = [];
      this.selectedCalendarId = null;
      this.tasks = [];
      this.events = [];
      this.sessions = [];
      this.monthlySummary = null;
      this.calendarForm = emptyCalendarForm();
      this.taskForm = emptyTaskForm();
      this.eventForm = emptyEventForm();
      this.sessionForm = emptySessionForm();
      this.addToast("Signed out.", "success");
    },
    reconcileSelectedCalendar() {
      if (!this.calendars.length) {
        this.selectedCalendarId = null;
        return;
      }

      const existing = this.calendars.find((item) => item.calendarId === this.selectedCalendarId);
      if (!existing) {
        const preferred = this.calendars.find((item) => item.isDefault) || this.calendars[0];
        this.selectedCalendarId = preferred.calendarId;
      }

      this.taskForm.calendarId = this.selectedCalendarId;
      this.eventForm.calendarId = this.selectedCalendarId;
      this.sessionForm.calendarId = this.selectedCalendarId;
    },
    async initializeWorkspace() {
      this.activePage = "overview";
      await this.refreshCalendars();
      if (!this.calendars.length) {
        await this.createStarterCalendar(true);
      }
      if (this.selectedCalendarId) {
        await this.refreshCalendarScopedData();
      }
    },
    async refreshAll() {
      await this.refreshCalendars();
      await this.refreshCalendarScopedData();
      this.addToast("Workspace refreshed.", "success");
    },
    async refreshCalendarScopedData() {
      if (!this.selectedCalendarId) return;
      await Promise.all([
        this.refreshTasks(),
        this.refreshEvents(),
        this.refreshSessions(),
        this.fetchMonthlySummary(true),
      ]);
    },
    async refreshCalendars() {
      if (!this.user) return;
      this.busy.calendars = true;
      try {
        const response = await this.apiRequest(`/api/calendars?ownerUserId=${this.user.userId}`);
        this.calendars = Array.isArray(response) ? response : [];
        this.reconcileSelectedCalendar();
      } catch (error) {
        this.addToast(error.message, "error");
      } finally {
        this.busy.calendars = false;
      }
    },
    resetCalendarForm() {
      this.calendarForm = emptyCalendarForm();
    },
    editCalendar(calendar) {
      this.calendarForm = {
        calendarId: calendar.calendarId,
        name: calendar.name,
        description: calendar.description || "",
        colorHex: calendar.colorHex || "#157A6E",
        isDefault: !!calendar.isDefault,
      };
      this.activePage = "calendars";
    },
    async submitCalendar() {
      if (!this.user) return;
      this.busy.calendars = true;
      try {
        const payload = {
          ownerUserId: this.user.userId,
          name: this.calendarForm.name,
          description: this.calendarForm.description || null,
          colorHex: this.calendarForm.colorHex.toUpperCase(),
          isDefault: this.calendarForm.isDefault,
        };

        if (this.calendarForm.calendarId) {
          await this.apiRequest(`/api/calendars/${this.calendarForm.calendarId}`, {
            method: "PUT",
            body: payload,
          });
          this.addToast("Calendar updated.", "success");
        } else {
          await this.apiRequest("/api/calendars", {
            method: "POST",
            body: payload,
          });
          this.addToast("Calendar created.", "success");
        }

        this.resetCalendarForm();
        await this.refreshCalendars();
        await this.refreshCalendarScopedData();
      } catch (error) {
        this.addToast(error.message, "error");
      } finally {
        this.busy.calendars = false;
      }
    },
    async createStarterCalendar(silent = false) {
      if (!this.user) return;
      const existingNames = new Set(this.calendars.map((item) => item.name));
      let name = "My Calendar";
      let suffix = 1;
      while (existingNames.has(name)) {
        suffix += 1;
        name = `My Calendar ${suffix}`;
      }

      try {
        await this.apiRequest("/api/calendars", {
          method: "POST",
          body: {
            ownerUserId: this.user.userId,
            name,
            description: "Personal planning space",
            colorHex: "#157A6E",
            isDefault: this.calendars.length === 0,
          },
        });
        await this.refreshCalendars();
        await this.refreshCalendarScopedData();
        if (!silent) this.addToast("Starter calendar added.", "success");
      } catch (error) {
        if (!silent) this.addToast(error.message, "error");
      }
    },
    async deleteCalendar(calendar) {
      if (!window.confirm(`Delete calendar "${calendar.name}"?`)) return;
      this.busy.calendars = true;
      try {
        await this.apiRequest(`/api/calendars/${calendar.calendarId}`, { method: "DELETE" });
        this.addToast("Calendar deleted.", "success");
        await this.refreshCalendars();
        await this.refreshCalendarScopedData();
      } catch (error) {
        this.addToast(error.message, "error");
      } finally {
        this.busy.calendars = false;
      }
    },
    resetTaskForm() {
      this.taskForm = emptyTaskForm(this.selectedCalendarId);
    },
    editTask(task) {
      this.taskForm = {
        taskItemId: task.taskItemId,
        calendarId: task.calendarId,
        title: task.title,
        description: task.description || "",
        dueLocal: toInputDateTime(task.dueUtc),
        priority: task.priority,
        status: task.status,
        reminderMinutesBefore: task.reminderMinutesBefore ?? "",
      };
      this.activePage = "tasks";
    },
    async refreshTasks() {
      if (!this.selectedCalendarId) {
        this.tasks = [];
        return;
      }

      this.busy.tasks = true;
      try {
        const response = await this.apiRequest(`/api/tasks?calendarId=${this.selectedCalendarId}`);
        this.tasks = Array.isArray(response) ? response : [];
      } catch (error) {
        this.addToast(error.message, "error");
      } finally {
        this.busy.tasks = false;
      }
    },
    async submitTask() {
      if (!this.user || !this.taskForm.calendarId) return;
      this.busy.tasks = true;
      try {
        const payload = {
          calendarId: Number(this.taskForm.calendarId),
          createdByUserId: this.user.userId,
          title: this.taskForm.title,
          description: this.taskForm.description || null,
          dueUtc: toIsoOrNull(this.taskForm.dueLocal),
          priority: this.taskForm.priority,
          status: this.taskForm.status,
          completedAtUtc: this.taskForm.status === "Done" ? new Date().toISOString() : null,
          reminderMinutesBefore:
            this.taskForm.reminderMinutesBefore === "" ? null : Number(this.taskForm.reminderMinutesBefore),
        };

        if (this.taskForm.taskItemId) {
          await this.apiRequest(`/api/tasks/${this.taskForm.taskItemId}`, {
            method: "PUT",
            body: payload,
          });
          this.addToast("Task updated.", "success");
        } else {
          await this.apiRequest("/api/tasks", {
            method: "POST",
            body: payload,
          });
          this.addToast("Task created.", "success");
        }

        await this.refreshTasks();
        this.resetTaskForm();
      } catch (error) {
        this.addToast(error.message, "error");
      } finally {
        this.busy.tasks = false;
      }
    },
    async quickTaskStatus(task, status) {
      try {
        await this.apiRequest(`/api/tasks/${task.taskItemId}/status`, {
          method: "PUT",
          body: {
            status,
            completedAtUtc: status === "Done" ? new Date().toISOString() : null,
          },
        });
        await this.refreshTasks();
        this.addToast(`Task moved to ${status}.`, "success");
      } catch (error) {
        this.addToast(error.message, "error");
      }
    },
    async deleteTask(task) {
      if (!window.confirm(`Delete task "${task.title}"?`)) return;
      try {
        await this.apiRequest(`/api/tasks/${task.taskItemId}`, { method: "DELETE" });
        await this.refreshTasks();
        this.addToast("Task deleted.", "success");
      } catch (error) {
        this.addToast(error.message, "error");
      }
    },
    resetEventForm() {
      this.eventForm = emptyEventForm(this.selectedCalendarId);
    },
    editEvent(event) {
      this.eventForm = {
        eventId: event.eventId,
        calendarId: event.calendarId,
        title: event.title,
        description: event.description || "",
        location: event.location || "",
        startLocal: toInputDateTime(event.startUtc),
        endLocal: toInputDateTime(event.endUtc),
        isAllDay: event.isAllDay,
        repeatType: event.repeatType,
        reminderMinutesBefore: event.reminderMinutesBefore ?? "",
        status: event.status,
      };
      this.activePage = "events";
    },
    async refreshEvents() {
      if (!this.selectedCalendarId) {
        this.events = [];
        return;
      }

      this.busy.events = true;
      try {
        const response = await this.apiRequest(`/api/events?calendarId=${this.selectedCalendarId}`);
        this.events = Array.isArray(response)
          ? response.sort((a, b) => new Date(a.startUtc).getTime() - new Date(b.startUtc).getTime())
          : [];
      } catch (error) {
        this.addToast(error.message, "error");
      } finally {
        this.busy.events = false;
      }
    },
    async submitEvent() {
      if (!this.user || !this.eventForm.calendarId) return;
      const startIso = toIsoOrNull(this.eventForm.startLocal);
      const endIso = toIsoOrNull(this.eventForm.endLocal);

      if (!startIso || !endIso || new Date(endIso) <= new Date(startIso)) {
        this.addToast("Event end must be after start.", "error");
        return;
      }

      this.busy.events = true;
      try {
        const payload = {
          calendarId: Number(this.eventForm.calendarId),
          createdByUserId: this.user.userId,
          title: this.eventForm.title,
          description: this.eventForm.description || null,
          location: this.eventForm.location || null,
          startUtc: startIso,
          endUtc: endIso,
          isAllDay: this.eventForm.isAllDay,
          repeatType: this.eventForm.repeatType,
          reminderMinutesBefore:
            this.eventForm.reminderMinutesBefore === "" ? null : Number(this.eventForm.reminderMinutesBefore),
          status: this.eventForm.status,
        };

        if (this.eventForm.eventId) {
          await this.apiRequest(`/api/events/${this.eventForm.eventId}`, {
            method: "PUT",
            body: payload,
          });
          this.addToast("Event updated.", "success");
        } else {
          await this.apiRequest("/api/events", {
            method: "POST",
            body: payload,
          });
          this.addToast("Event created.", "success");
        }

        await this.refreshEvents();
        this.resetEventForm();
      } catch (error) {
        this.addToast(error.message, "error");
      } finally {
        this.busy.events = false;
      }
    },
    async deleteEvent(event) {
      if (!window.confirm(`Delete event "${event.title}"?`)) return;
      try {
        await this.apiRequest(`/api/events/${event.eventId}`, { method: "DELETE" });
        await this.refreshEvents();
        this.addToast("Event deleted.", "success");
      } catch (error) {
        this.addToast(error.message, "error");
      }
    },
    resetSessionForm() {
      this.sessionForm = emptySessionForm(this.selectedCalendarId);
    },
    editSession(session) {
      this.sessionForm = {
        privateClassSessionId: session.privateClassSessionId,
        calendarId: session.calendarId,
        studentName: session.studentName,
        studentContact: session.studentContact || "",
        sessionStartLocal: toInputDateTime(session.sessionStartUtc),
        sessionEndLocal: toInputDateTime(session.sessionEndUtc),
        topicPlanned: session.topicPlanned || "",
        topicDone: session.topicDone || "",
        homeworkAssigned: session.homeworkAssigned || "",
        priceAmount: String(session.priceAmount ?? 0),
        currencyCode: session.currencyCode || "RSD",
        isPaid: !!session.isPaid,
        paymentMethod: session.paymentMethod || "",
        paymentNote: session.paymentNote || "",
        status: session.status,
      };
      this.activePage = "sessions";
    },
    async refreshSessions() {
      if (!this.selectedCalendarId) {
        this.sessions = [];
        return;
      }

      this.busy.sessions = true;
      try {
        const response = await this.apiRequest(`/api/private-class-sessions?calendarId=${this.selectedCalendarId}`);
        this.sessions = Array.isArray(response)
          ? response.sort((a, b) => new Date(a.sessionStartUtc).getTime() - new Date(b.sessionStartUtc).getTime())
          : [];
      } catch (error) {
        this.addToast(error.message, "error");
      } finally {
        this.busy.sessions = false;
      }
    },
    async submitSession() {
      if (!this.user || !this.sessionForm.calendarId) return;
      const startIso = toIsoOrNull(this.sessionForm.sessionStartLocal);
      const endIso = toIsoOrNull(this.sessionForm.sessionEndLocal);

      if (!startIso || !endIso || new Date(endIso) <= new Date(startIso)) {
        this.addToast("Session end must be after start.", "error");
        return;
      }

      this.busy.sessions = true;
      try {
        const payload = {
          calendarId: Number(this.sessionForm.calendarId),
          createdByUserId: this.user.userId,
          studentName: this.sessionForm.studentName,
          studentContact: this.sessionForm.studentContact || null,
          sessionStartUtc: startIso,
          sessionEndUtc: endIso,
          topicPlanned: this.sessionForm.topicPlanned || null,
          topicDone: this.sessionForm.topicDone || null,
          homeworkAssigned: this.sessionForm.homeworkAssigned || null,
          priceAmount: Number(this.sessionForm.priceAmount || 0),
          currencyCode: (this.sessionForm.currencyCode || "RSD").toUpperCase(),
          isPaid: this.sessionForm.isPaid,
          paidAtUtc: this.sessionForm.isPaid ? new Date().toISOString() : null,
          paymentMethod: this.sessionForm.paymentMethod || null,
          paymentNote: this.sessionForm.paymentNote || null,
          status: this.sessionForm.status,
        };

        if (this.sessionForm.privateClassSessionId) {
          await this.apiRequest(`/api/private-class-sessions/${this.sessionForm.privateClassSessionId}`, {
            method: "PUT",
            body: payload,
          });
          this.addToast("Session updated.", "success");
        } else {
          await this.apiRequest("/api/private-class-sessions", {
            method: "POST",
            body: payload,
          });
          this.addToast("Session created.", "success");
        }

        await this.refreshSessions();
        await this.fetchMonthlySummary(true);
        this.resetSessionForm();
      } catch (error) {
        this.addToast(error.message, "error");
      } finally {
        this.busy.sessions = false;
      }
    },
    async markSessionPaid(session) {
      try {
        await this.apiRequest(`/api/private-class-sessions/${session.privateClassSessionId}/mark-paid`, {
          method: "PUT",
          body: {
            paymentMethod: session.paymentMethod || "Cash",
            paymentNote: session.paymentNote || "Marked as paid from user UI.",
            paidAtUtc: new Date().toISOString(),
          },
        });
        await this.refreshSessions();
        await this.fetchMonthlySummary(true);
        this.addToast("Session marked as paid.", "success");
      } catch (error) {
        this.addToast(error.message, "error");
      }
    },
    async markSessionUnpaid(session) {
      try {
        await this.apiRequest(`/api/private-class-sessions/${session.privateClassSessionId}/mark-unpaid`, {
          method: "PUT",
        });
        await this.refreshSessions();
        await this.fetchMonthlySummary(true);
        this.addToast("Session marked as unpaid.", "success");
      } catch (error) {
        this.addToast(error.message, "error");
      }
    },
    async deleteSession(session) {
      if (!window.confirm(`Delete session for "${session.studentName}"?`)) return;
      try {
        await this.apiRequest(`/api/private-class-sessions/${session.privateClassSessionId}`, {
          method: "DELETE",
        });
        await this.refreshSessions();
        await this.fetchMonthlySummary(true);
        this.addToast("Session deleted.", "success");
      } catch (error) {
        this.addToast(error.message, "error");
      }
    },
    async fetchMonthlySummary(silent = false) {
      if (!this.selectedCalendarId) {
        this.monthlySummary = null;
        return;
      }

      const [yearText, monthText] = this.summaryMonth.split("-");
      const year = Number(yearText);
      const month = Number(monthText);
      if (!Number.isInteger(year) || !Number.isInteger(month)) return;

      try {
        const response = await this.apiRequest(
          `/api/private-class-sessions/monthly-summary?calendarId=${this.selectedCalendarId}&year=${year}&month=${month}`
        );
        this.monthlySummary = response;
      } catch (error) {
        if (!silent) this.addToast(error.message, "error");
      }
    },
    formatDate(value) {
      if (!value) return "";
      const date = new Date(value);
      if (Number.isNaN(date.getTime())) return "";
      return new Intl.DateTimeFormat("en-GB", {
        day: "2-digit",
        month: "short",
        year: "numeric",
        hour: "2-digit",
        minute: "2-digit",
      }).format(date);
    },
    formatMoney(value) {
      const amount = Number(value || 0);
      return amount.toFixed(2);
    },
  },
}).mount("#app");
