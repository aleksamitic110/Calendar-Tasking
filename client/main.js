const { createApp } = Vue;

const USER_STORAGE_KEY = "calendar_tasking_user_v1";
const DAY_SLOT_MINUTES = 15;
const DAY_TIMELINE_PIXELS_PER_MINUTE = 1;
const ISO_HAS_TIMEZONE_RE = /(Z|[+-]\d{2}:\d{2})$/i;

const parseApiDate = (value) => {
  if (!value) return null;
  if (value instanceof Date) return Number.isNaN(value.getTime()) ? null : value;

  let normalized = String(value).trim();
  if (!normalized) return null;

  // API stores UTC in SQL DATETIME2, which may come back without "Z".
  if (!ISO_HAS_TIMEZONE_RE.test(normalized)) {
    normalized = `${normalized}Z`;
  }

  const date = new Date(normalized);
  return Number.isNaN(date.getTime()) ? null : date;
};

const dateEpochOrMax = (value) => {
  const parsed = parseApiDate(value);
  return parsed ? parsed.getTime() : Number.MAX_SAFE_INTEGER;
};

const dateEpochOrNaN = (value) => {
  const parsed = parseApiDate(value);
  return parsed ? parsed.getTime() : Number.NaN;
};

const toInputDateTime = (value) => {
  if (!value) return "";
  const date = parseApiDate(value);
  if (!date) return "";
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
      dragState: null,
      resizeState: null,
      lastDragWasMove: false,
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
        const first = a.dueUtc ? dateEpochOrMax(a.dueUtc) : Number.MAX_SAFE_INTEGER;
        const second = b.dueUtc ? dateEpochOrMax(b.dueUtc) : Number.MAX_SAFE_INTEGER;
        return first - second;
      });

      return list;
    },
    upcomingEvents() {
      const nowValue = Date.now();
      return [...this.events]
        .filter((event) => dateEpochOrNaN(event.startUtc) >= nowValue)
        .sort((a, b) => dateEpochOrMax(a.startUtc) - dateEpochOrMax(b.startUtc))
        .slice(0, 6);
    },
    dueSoonTasks() {
      const nowValue = Date.now();
      const oneWeek = nowValue + 7 * 24 * 60 * 60 * 1000;
      return this.tasks
        .filter((task) => task.status !== "Done" && task.dueUtc && dateEpochOrNaN(task.dueUtc) <= oneWeek)
        .sort((a, b) => dateEpochOrMax(a.dueUtc) - dateEpochOrMax(b.dueUtc))
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
        const startDate = parseApiDate(ev.startUtc);
        if (!startDate) continue;
        const key = this._dateKey(startDate);
        if (!map[key]) map[key] = [];
        map[key].push({ title: ev.title, type: "event", color: "var(--neon-cyan)" });
      }
      for (const task of this.tasks) {
        if (!task.dueUtc) continue;
        const dueDate = parseApiDate(task.dueUtc);
        if (!dueDate) continue;
        const key = this._dateKey(dueDate);
        if (!map[key]) map[key] = [];
        const color = task.status === "Done" ? "var(--neon-green)" : task.priority === "High" ? "var(--neon-red)" : "var(--neon-yellow)";
        map[key].push({ title: task.title, type: "task", color });
      }
      for (const s of this.sessions) {
        const sessionStartDate = parseApiDate(s.sessionStartUtc);
        if (!sessionStartDate) continue;
        const key = this._dateKey(sessionStartDate);
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
    dayTimelineHours() {
      return Array.from({ length: 24 }, (_, hour) => hour);
    },
    dayTimelineItems() {
      if (!this.selectedDay) return [];
      const dayKey = this.selectedDay.key;
      const items = [];

      for (const ev of this.events) {
        const start = parseApiDate(ev.startUtc);
        const end = parseApiDate(ev.endUtc);
        if (!start || !end) continue;
        if (this._dateKey(start) !== dayKey) continue;
        items.push({
          key: `event-${ev.eventId}`,
          sourceType: "event",
          source: ev,
          title: ev.title,
          type: "Event",
          color: "var(--neon-cyan)",
          chipClass: "cyan",
          location: ev.location || "",
          startMinutes: start.getHours() * 60 + start.getMinutes(),
          durationMinutes: Math.max(DAY_SLOT_MINUTES, Math.round((end - start) / 60000)),
        });
      }

      for (const task of this.tasks) {
        if (!task.dueUtc) continue;
        const due = parseApiDate(task.dueUtc);
        if (!due) continue;
        const dueMinutes = due.getHours() * 60 + due.getMinutes();
        const reminderWindowMinutes = Math.max(
          DAY_SLOT_MINUTES,
          Number.isFinite(Number(task.reminderMinutesBefore))
            ? Math.max(0, Number(task.reminderMinutesBefore))
            : 60
        );
        const startMinutes = this._clampMinutesToDay(dueMinutes - reminderWindowMinutes, reminderWindowMinutes);
        if (this._dateKey(due) !== dayKey) continue;
        const color = task.status === "Done" ? "var(--neon-green)" : task.priority === "High" ? "var(--neon-red)" : "var(--neon-yellow)";
        const chipClass = task.status === "Done" ? "green" : task.priority === "High" ? "red" : "yellow";
        items.push({
          key: `task-${task.taskItemId}`,
          sourceType: "task",
          source: task,
          title: task.title,
          type: "Task",
          color,
          chipClass,
          location: "",
          startMinutes,
          durationMinutes: reminderWindowMinutes,
        });
      }

      for (const session of this.sessions) {
        const start = parseApiDate(session.sessionStartUtc);
        const end = parseApiDate(session.sessionEndUtc);
        if (!start || !end) continue;
        if (this._dateKey(start) !== dayKey) continue;
        items.push({
          key: `session-${session.privateClassSessionId}`,
          sourceType: "session",
          source: session,
          title: session.studentName,
          type: "Session",
          color: "var(--neon-magenta)",
          chipClass: "magenta",
          location: session.topicPlanned || "",
          startMinutes: start.getHours() * 60 + start.getMinutes(),
          durationMinutes: Math.max(DAY_SLOT_MINUTES, Math.round((end - start) / 60000)),
        });
      }

      return items.sort((a, b) => a.startMinutes - b.startMinutes);
    },
    dayTimelineHeightPx() {
      return `${24 * 60 * DAY_TIMELINE_PIXELS_PER_MINUTE}px`;
    },
    dayViewHasItems() {
      return this.dayTimelineItems.length > 0;
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
  unmounted() {
    this.cancelTimelineDrag();
    this.cancelTimelineResize();
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
    _roundMinutesToSlot(minutes) {
      return Math.round(minutes / DAY_SLOT_MINUTES) * DAY_SLOT_MINUTES;
    },
    _clampMinutesToDay(minutes, durationMinutes = DAY_SLOT_MINUTES) {
      const maxStart = Math.max(0, 24 * 60 - durationMinutes);
      return Math.min(maxStart, Math.max(0, minutes));
    },
    _dayDateAtMinutes(minutes) {
      if (!this.selectedDay) return null;
      const clamped = this._clampMinutesToDay(minutes);
      const hours = Math.floor(clamped / 60);
      const mins = clamped % 60;
      return new Date(
        this.selectedDay.date.getFullYear(),
        this.selectedDay.date.getMonth(),
        this.selectedDay.date.getDate(),
        hours,
        mins,
        0,
        0
      );
    },
    _dayDefaultStartMinutes() {
      const nowValue = new Date();
      if (this.selectedDay && this._dateKey(nowValue) === this.selectedDay.key) {
        const roundedNow = this._roundMinutesToSlot(nowValue.getHours() * 60 + nowValue.getMinutes());
        return this._clampMinutesToDay(roundedNow);
      }
      return 9 * 60;
    },
    _displayStartMinutes(item) {
      if (this.dragState && this.dragState.itemKey === item.key) {
        return this.dragState.currentStartMinutes;
      }
      return item.startMinutes;
    },
    _displayDurationMinutes(item) {
      if (this.resizeState && this.resizeState.itemKey === item.key) {
        return this.resizeState.currentDurationMinutes;
      }
      return item.durationMinutes;
    },
    dayTimelineItemStyle(item) {
      const startMinutes = this._displayStartMinutes(item);
      const top = startMinutes * DAY_TIMELINE_PIXELS_PER_MINUTE;
      const height = Math.max(24, this._displayDurationMinutes(item) * DAY_TIMELINE_PIXELS_PER_MINUTE);
      return {
        top: `${top}px`,
        height: `${height}px`,
        borderLeftColor: item.color,
      };
    },
    dayTimelineItemTime(item) {
      const startDate = this._dayDateAtMinutes(this._displayStartMinutes(item));
      if (!startDate) return "";
      const endDate = new Date(startDate.getTime() + this._displayDurationMinutes(item) * 60000);
      if (item.sourceType === "task") {
        return `${this._fmtTime(startDate)} - ${this._fmtTime(endDate)} due`;
      }
      return `${this._fmtTime(startDate)} \u2013 ${this._fmtTime(endDate)}`;
    },
    quickCreateOnSelectedDay(type) {
      if (!this.selectedDay || !this.selectedCalendarId) return;
      const startDate = this._dayDateAtMinutes(this._dayDefaultStartMinutes());
      if (!startDate) return;

      if (type === "task") {
        this.resetTaskForm();
        this.taskForm.calendarId = this.selectedCalendarId;
        this.taskForm.dueLocal = toInputDateTime(startDate);
        this.activePage = "tasks";
        return;
      }

      const endDate = new Date(startDate.getTime() + 60 * 60000);

      if (type === "event") {
        this.resetEventForm();
        this.eventForm.calendarId = this.selectedCalendarId;
        this.eventForm.startLocal = toInputDateTime(startDate);
        this.eventForm.endLocal = toInputDateTime(endDate);
        this.activePage = "events";
        return;
      }

      this.resetSessionForm();
      this.sessionForm.calendarId = this.selectedCalendarId;
      this.sessionForm.sessionStartLocal = toInputDateTime(startDate);
      this.sessionForm.sessionEndLocal = toInputDateTime(endDate);
      this.activePage = "sessions";
    },
    startTimelineDrag(item, mouseEvent) {
      if (mouseEvent.button !== 0) return;
      if (this.resizeState) return;
      mouseEvent.preventDefault();
      this.lastDragWasMove = false;
      this.dragState = {
        itemKey: item.key,
        sourceType: item.sourceType,
        source: item.source,
        initialY: mouseEvent.clientY,
        initialStartMinutes: this._displayStartMinutes(item),
        currentStartMinutes: this._displayStartMinutes(item),
        durationMinutes: this._displayDurationMinutes(item),
        moved: false,
      };
      window.addEventListener("mousemove", this.onTimelineDragMove);
      window.addEventListener("mouseup", this.onTimelineDragEnd);
    },
    onTimelineDragMove(mouseEvent) {
      if (!this.dragState) return;
      const deltaY = mouseEvent.clientY - this.dragState.initialY;
      const deltaMinutesRaw = deltaY / DAY_TIMELINE_PIXELS_PER_MINUTE;
      const snappedDelta = this._roundMinutesToSlot(deltaMinutesRaw);
      const proposedStart = this.dragState.initialStartMinutes + snappedDelta;
      this.dragState.currentStartMinutes = this._clampMinutesToDay(proposedStart, this.dragState.durationMinutes);
      this.dragState.moved = this.dragState.currentStartMinutes !== this.dragState.initialStartMinutes;
    },
    cancelTimelineDrag() {
      this.dragState = null;
      window.removeEventListener("mousemove", this.onTimelineDragMove);
      window.removeEventListener("mouseup", this.onTimelineDragEnd);
    },
    startTimelineResize(item, mouseEvent) {
      if (mouseEvent.button !== 0) return;
      if (this.dragState) return;
      mouseEvent.preventDefault();
      mouseEvent.stopPropagation();
      this.lastDragWasMove = false;
      const startMinutes = this._displayStartMinutes(item);
      this.resizeState = {
        itemKey: item.key,
        sourceType: item.sourceType,
        source: item.source,
        initialY: mouseEvent.clientY,
        startMinutes,
        initialDurationMinutes: this._displayDurationMinutes(item),
        currentDurationMinutes: this._displayDurationMinutes(item),
        moved: false,
      };
      window.addEventListener("mousemove", this.onTimelineResizeMove);
      window.addEventListener("mouseup", this.onTimelineResizeEnd);
    },
    onTimelineResizeMove(mouseEvent) {
      if (!this.resizeState) return;
      const deltaY = mouseEvent.clientY - this.resizeState.initialY;
      const deltaMinutesRaw = deltaY / DAY_TIMELINE_PIXELS_PER_MINUTE;
      const snappedDelta = this._roundMinutesToSlot(deltaMinutesRaw);
      const maxDuration = Math.max(DAY_SLOT_MINUTES, 24 * 60 - this.resizeState.startMinutes);
      const proposedDuration = this.resizeState.initialDurationMinutes + snappedDelta;
      this.resizeState.currentDurationMinutes = Math.min(maxDuration, Math.max(DAY_SLOT_MINUTES, proposedDuration));
      this.resizeState.moved = this.resizeState.currentDurationMinutes !== this.resizeState.initialDurationMinutes;
    },
    cancelTimelineResize() {
      this.resizeState = null;
      window.removeEventListener("mousemove", this.onTimelineResizeMove);
      window.removeEventListener("mouseup", this.onTimelineResizeEnd);
    },
    async onTimelineResizeEnd() {
      if (!this.resizeState) return;
      const resizeSnapshot = { ...this.resizeState };
      this.cancelTimelineResize();

      this.lastDragWasMove = !!resizeSnapshot.moved;
      if (!this.lastDragWasMove) return;
      await this.persistTimelineResize(resizeSnapshot);
    },
    async onTimelineDragEnd() {
      if (!this.dragState) return;
      const dragSnapshot = { ...this.dragState };
      this.cancelTimelineDrag();

      this.lastDragWasMove = !!dragSnapshot.moved;
      if (!this.lastDragWasMove) return;
      await this.persistTimelineMove(dragSnapshot);
    },
    onTimelineItemClick(item) {
      if (this.lastDragWasMove) {
        this.lastDragWasMove = false;
        return;
      }

      if (item.sourceType === "task") {
        this.editTask(item.source);
        return;
      }

      if (item.sourceType === "event") {
        this.editEvent(item.source);
        return;
      }

      this.editSession(item.source);
    },
    async persistTimelineResize(resizeSnapshot) {
      if (!this.selectedDay || !this.user) return;
      const startDate = this._dayDateAtMinutes(resizeSnapshot.startMinutes);
      if (!startDate) return;

      try {
        if (resizeSnapshot.sourceType === "task") {
          const task = resizeSnapshot.source;
          const newDue = new Date(startDate.getTime() + resizeSnapshot.currentDurationMinutes * 60000);
          await this.apiRequest(`/api/tasks/${task.taskItemId}`, {
            method: "PUT",
            body: {
              calendarId: Number(task.calendarId),
              createdByUserId: task.createdByUserId ?? this.user.userId,
              title: task.title,
              description: task.description || null,
              dueUtc: newDue.toISOString(),
              priority: task.priority,
              status: task.status,
              completedAtUtc: task.completedAtUtc || (task.status === "Done" ? new Date().toISOString() : null),
              reminderMinutesBefore: resizeSnapshot.currentDurationMinutes,
            },
          });
          await this.refreshTasks();
          this.addToast("Task duration updated.", "success");
          return;
        }

        if (resizeSnapshot.sourceType === "event") {
          const event = resizeSnapshot.source;
          const newEnd = new Date(startDate.getTime() + resizeSnapshot.currentDurationMinutes * 60000);
          await this.apiRequest(`/api/events/${event.eventId}`, {
            method: "PUT",
            body: {
              calendarId: Number(event.calendarId),
              createdByUserId: event.createdByUserId ?? this.user.userId,
              title: event.title,
              description: event.description || null,
              location: event.location || null,
              startUtc: startDate.toISOString(),
              endUtc: newEnd.toISOString(),
              isAllDay: !!event.isAllDay,
              repeatType: event.repeatType,
              reminderMinutesBefore: event.reminderMinutesBefore ?? null,
              status: event.status,
            },
          });
          await this.refreshEvents();
          this.addToast("Event duration updated.", "success");
          return;
        }

        const session = resizeSnapshot.source;
        const newEnd = new Date(startDate.getTime() + resizeSnapshot.currentDurationMinutes * 60000);
        await this.apiRequest(`/api/private-class-sessions/${session.privateClassSessionId}`, {
          method: "PUT",
          body: {
            calendarId: Number(session.calendarId),
            createdByUserId: session.createdByUserId ?? this.user.userId,
            studentName: session.studentName,
            studentContact: session.studentContact || null,
            sessionStartUtc: startDate.toISOString(),
            sessionEndUtc: newEnd.toISOString(),
            topicPlanned: session.topicPlanned || null,
            topicDone: session.topicDone || null,
            homeworkAssigned: session.homeworkAssigned || null,
            priceAmount: Number(session.priceAmount || 0),
            currencyCode: (session.currencyCode || "RSD").toUpperCase(),
            isPaid: !!session.isPaid,
            paidAtUtc: session.paidAtUtc || null,
            paymentMethod: session.paymentMethod || null,
            paymentNote: session.paymentNote || null,
            status: session.status,
          },
        });
        await this.refreshSessions();
        await this.fetchMonthlySummary(true);
        this.addToast("Session duration updated.", "success");
      } catch (error) {
        this.addToast(error.message, "error");
      }
    },
    async persistTimelineMove(dragSnapshot) {
      if (!this.selectedDay || !this.user) return;
      const newStart = this._dayDateAtMinutes(dragSnapshot.currentStartMinutes);
      if (!newStart) return;

      try {
        if (dragSnapshot.sourceType === "task") {
          const task = dragSnapshot.source;
          const newDue = new Date(newStart.getTime() + dragSnapshot.durationMinutes * 60000);
          await this.apiRequest(`/api/tasks/${task.taskItemId}`, {
            method: "PUT",
            body: {
              calendarId: Number(task.calendarId),
              createdByUserId: task.createdByUserId ?? this.user.userId,
              title: task.title,
              description: task.description || null,
              dueUtc: newDue.toISOString(),
              priority: task.priority,
              status: task.status,
              completedAtUtc: task.completedAtUtc || (task.status === "Done" ? new Date().toISOString() : null),
              reminderMinutesBefore: dragSnapshot.durationMinutes,
            },
          });
          await this.refreshTasks();
          this.addToast("Task moved.", "success");
          return;
        }

        if (dragSnapshot.sourceType === "event") {
          const event = dragSnapshot.source;
          const newEnd = new Date(newStart.getTime() + dragSnapshot.durationMinutes * 60000);
          await this.apiRequest(`/api/events/${event.eventId}`, {
            method: "PUT",
            body: {
              calendarId: Number(event.calendarId),
              createdByUserId: event.createdByUserId ?? this.user.userId,
              title: event.title,
              description: event.description || null,
              location: event.location || null,
              startUtc: newStart.toISOString(),
              endUtc: newEnd.toISOString(),
              isAllDay: !!event.isAllDay,
              repeatType: event.repeatType,
              reminderMinutesBefore: event.reminderMinutesBefore ?? null,
              status: event.status,
            },
          });
          await this.refreshEvents();
          this.addToast("Event moved.", "success");
          return;
        }

        const session = dragSnapshot.source;
        const newEnd = new Date(newStart.getTime() + dragSnapshot.durationMinutes * 60000);
        await this.apiRequest(`/api/private-class-sessions/${session.privateClassSessionId}`, {
          method: "PUT",
          body: {
            calendarId: Number(session.calendarId),
            createdByUserId: session.createdByUserId ?? this.user.userId,
            studentName: session.studentName,
            studentContact: session.studentContact || null,
            sessionStartUtc: newStart.toISOString(),
            sessionEndUtc: newEnd.toISOString(),
            topicPlanned: session.topicPlanned || null,
            topicDone: session.topicDone || null,
            homeworkAssigned: session.homeworkAssigned || null,
            priceAmount: Number(session.priceAmount || 0),
            currencyCode: (session.currencyCode || "RSD").toUpperCase(),
            isPaid: !!session.isPaid,
            paidAtUtc: session.paidAtUtc || null,
            paymentMethod: session.paymentMethod || null,
            paymentNote: session.paymentNote || null,
            status: session.status,
          },
        });
        await this.refreshSessions();
        await this.fetchMonthlySummary(true);
        this.addToast("Session moved.", "success");
      } catch (error) {
        this.addToast(error.message, "error");
      }
    },
    selectDay(cell) {
      if (cell.outside) return;
      this.cancelTimelineDrag();
      this.cancelTimelineResize();
      this.selectedDay = this.selectedDay && this.selectedDay.key === cell.key ? null : cell;
    },
    closeDayView() {
      this.cancelTimelineDrag();
      this.cancelTimelineResize();
      this.selectedDay = null;
    },
    calPrev() {
      this.cancelTimelineDrag();
      this.cancelTimelineResize();
      this.calViewDate = new Date(this.calViewYear, this.calViewMonth - 1, 1);
      this.selectedDay = null;
    },
    calNext() {
      this.cancelTimelineDrag();
      this.cancelTimelineResize();
      this.calViewDate = new Date(this.calViewYear, this.calViewMonth + 1, 1);
      this.selectedDay = null;
    },
    calToday() {
      this.cancelTimelineDrag();
      this.cancelTimelineResize();
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
      this.cancelTimelineDrag();
      this.cancelTimelineResize();
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
          ? response.sort((a, b) => dateEpochOrMax(a.startUtc) - dateEpochOrMax(b.startUtc))
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
          ? response.sort((a, b) => dateEpochOrMax(a.sessionStartUtc) - dateEpochOrMax(b.sessionStartUtc))
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
      const date = parseApiDate(value);
      if (!date) return "";
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
