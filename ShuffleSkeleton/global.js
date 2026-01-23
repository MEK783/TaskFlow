(function () {
  const init = () => {
    let tasks = [];
    let taskIdCounter = 1;
    let draggedTask = null;
    let currentMobileTab = "waiting";

    const loginSection = document
    .querySelector(".login-form")
    ?.closest("section");
    const signupSection = document.querySelector(".signup-section");
    const dashboardSection = document.querySelector(".dashboard-section");
    const dashboardFooter = document.querySelector(".dashboard-footer");

    const showLogin = () => {
      loginSection?.classList.remove("hidden");
      signupSection?.classList.add("hidden");
      dashboardSection?.classList.add("hidden");
      dashboardFooter?.classList.add("hidden");
    };

    const showSignup = () => {
      loginSection?.classList.add("hidden");
      signupSection?.classList.remove("hidden");
      dashboardSection?.classList.add("hidden");
      dashboardFooter?.classList.add("hidden");
    };

    const showDashboard = () => {
      loginSection?.classList.add("hidden");
      signupSection?.classList.add("hidden");
      dashboardSection?.classList.remove("hidden");
      dashboardFooter?.classList.remove("hidden");
    };

    document.querySelectorAll(".signup-link").forEach((link) => {
      link.addEventListener("click", (e) => {
        e.preventDefault();
        showSignup();
      });
    });

    document.querySelectorAll(".login-link").forEach((link) => {
      link.addEventListener("click", (e) => {
        e.preventDefault();
        showLogin();
      });
    });

    document.querySelectorAll(".login-btn").forEach((btn) => {
      btn.addEventListener("click", () => showDashboard());
    });

    document.querySelectorAll(".register-btn").forEach((btn) => {
      btn.addEventListener("click", () => showDashboard());
    });

    document.querySelectorAll(".logout-btn").forEach((btn) => {
      btn.addEventListener("click", () => showLogin());
    });

    const getTaskColors = (status) => {
      const colors = {
        waiting: {
          bg: "bg-amber-50",
          border: "border-amber-300",
          header: "bg-amber-100",
        },
        inprogress: {
          bg: "bg-blue-50",
          border: "border-blue-300",
          header: "bg-blue-100",
        },
        finished: {
          bg: "bg-green-50",
          border: "border-green-300",
          header: "bg-green-100",
        },
      };
      return colors[status] || colors.waiting;
    };

    const createTaskElement = (task) => {
      const colors = getTaskColors(task.status);
      const div = document.createElement("div");
      div.className = `task-item mb-3 rounded-lg border-2 ${colors.border} ${colors.bg} overflow-hidden cursor-move`;
      div.draggable = true;
      div.dataset.taskId = task.id;

      const isEditing = task.isNew || task.isEditing;
      const subtasksHtml = task.subtasks
      .map((st, idx) => {
        const stColors = getTaskColors(st.status);
        return `<div class="subtask-item flex items-center gap-2 p-2 rounded border ${stColors.border} ${stColors.bg} mb-2" data-subtask-idx="${idx}">
<select class="subtask-status-select text-xs py-1 px-2 rounded border border-coolGray-300">
<option value="waiting" ${st.status === "waiting" ? "selected" : ""}>Waiting</option>
<option value="inprogress" ${st.status === "inprogress" ? "selected" : ""}>In Progress</option>
<option value="finished" ${st.status === "finished" ? "selected" : ""}>Finished</option>
</select>
<span class="flex-1 text-sm text-coolGray-700">${st.title}</span>
<button class="delete-subtask-btn text-red-500 hover:text-red-700">
<svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path></svg>
</button>
</div>`;
      })
      .join("");

      div.innerHTML = `
<div class="task-header flex items-center justify-between p-3 ${colors.header} cursor-pointer">
<div class="flex items-center gap-2 flex-1 min-w-0">
<svg class="collapse-icon w-4 h-4 text-coolGray-500 transition-transform ${task.collapsed ? "" : "rotate-90"}" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"></path></svg>
${isEditing ? `<input type="text" class="task-title-input flex-1 py-1 px-2 text-sm font-medium border border-coolGray-300 rounded" value="${task.title}" placeholder="Task title">` : `<span class="task-title text-sm font-medium text-coolGray-900 truncate">${task.title || "Untitled Task"}</span>`}
</div>
<div class="flex items-center gap-1">
${
        !isEditing
        ? `
<button class="edit-task-btn p-1 text-coolGray-500 hover:text-coolGray-700">
<svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"></path></svg>
</button>
`
      : ""
    }
<button class="move-left-btn p-1 text-coolGray-500 hover:text-coolGray-700 ${task.status === "waiting" ? "opacity-30 cursor-not-allowed" : ""}">
<svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7"></path></svg>
</button>
<button class="move-right-btn p-1 text-coolGray-500 hover:text-coolGray-700 ${task.status === "finished" ? "opacity-30 cursor-not-allowed" : ""}">
<svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"></path></svg>
</button>
<button class="delete-task-btn p-1 text-red-500 hover:text-red-700">
<svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"></path></svg>
</button>
</div>
</div>
<div class="task-body p-3 ${task.collapsed ? "hidden" : ""}">
${isEditing ? `<textarea class="task-desc-input w-full py-2 px-3 text-sm border border-coolGray-300 rounded mb-3" rows="3" placeholder="Task description">${task.description}</textarea>` : `<p class="task-description text-sm text-coolGray-600 mb-3">${task.description || "No description"}</p>`}
${isEditing ? `<button class="save-task-btn inline-block py-2 px-4 text-sm text-white bg-green-500 hover:bg-green-600 rounded font-medium mb-3">Save</button>` : ""}
<div class="subtasks-section border-t border-coolGray-200 pt-3 mt-3">
<div class="flex items-center justify-between mb-2">
<span class="text-xs font-medium text-coolGray-500 uppercase">Sub-tasks</span>
<button class="add-subtask-btn text-xs text-green-500 hover:text-green-600 font-medium">+ Add</button>
</div>
<div class="subtasks-list">${subtasksHtml}</div>
<div class="add-subtask-form hidden">
<input type="text" class="subtask-title-input w-full py-2 px-3 text-sm border border-coolGray-300 rounded mb-2" placeholder="Sub-task title">
<div class="flex gap-2">
<button class="confirm-subtask-btn py-1 px-3 text-xs text-white bg-green-500 hover:bg-green-600 rounded">Add</button>
<button class="cancel-subtask-btn py-1 px-3 text-xs text-coolGray-500 bg-coolGray-200 hover:bg-coolGray-300 rounded">Cancel</button>
</div>
</div>
</div>
</div>
`;

      return div;
    };

    const renderTasks = () => {
      document.querySelectorAll(".task-list").forEach((list) => {
        const status = list.dataset.status;
        list.innerHTML = "";
        tasks
          .filter((t) => t.status === status)
          .forEach((task) => {
          list.appendChild(createTaskElement(task));
        });
      });
      attachTaskEventListeners();
    };

    const attachTaskEventListeners = () => {
      document.querySelectorAll(".task-item").forEach((item) => {
        const taskId = parseInt(item.dataset.taskId);
        const task = tasks.find((t) => t.id === taskId);
        if (!task) return;

        item.addEventListener("dragstart", (e) => {
          draggedTask = task;
          e.dataTransfer.effectAllowed = "move";
          item.classList.add("opacity-50");
        });

        item.addEventListener("dragend", () => {
          item.classList.remove("opacity-50");
          draggedTask = null;
        });

        const header = item.querySelector(".task-header");
        header?.addEventListener("click", (e) => {
          if (e.target.closest("button") || e.target.closest("input"))
            return;
          task.collapsed = !task.collapsed;
          renderTasks();
        });

        item.querySelector(".edit-task-btn")?.addEventListener(
          "click",
          (e) => {
            e.stopPropagation();
            task.isEditing = true;
            task.collapsed = false;
            renderTasks();
          },
        );

        item.querySelector(".save-task-btn")?.addEventListener(
          "click",
          (e) => {
            e.stopPropagation();
            const titleInput =
                  item.querySelector(".task-title-input");
            const descInput =
                  item.querySelector(".task-desc-input");
            task.title = titleInput?.value || "Untitled Task";
            task.description = descInput?.value || "";
            task.isNew = false;
            task.isEditing = false;
            renderTasks();
          },
        );

        item.querySelector(".delete-task-btn")?.addEventListener(
          "click",
          (e) => {
            e.stopPropagation();
            tasks = tasks.filter((t) => t.id !== taskId);
            renderTasks();
          },
        );

        item.querySelector(".move-left-btn")?.addEventListener(
          "click",
          (e) => {
            e.stopPropagation();
            if (task.status === "inprogress")
              task.status = "waiting";
            else if (task.status === "finished")
              task.status = "inprogress";
            renderTasks();
          },
        );

        item.querySelector(".move-right-btn")?.addEventListener(
          "click",
          (e) => {
            e.stopPropagation();
            if (task.status === "waiting")
              task.status = "inprogress";
            else if (task.status === "inprogress")
              task.status = "finished";
            renderTasks();
          },
        );

        item.querySelector(".add-subtask-btn")?.addEventListener(
          "click",
          (e) => {
            e.stopPropagation();
            item.querySelector(
              ".add-subtask-form",
            )?.classList.remove("hidden");
          },
        );

        item.querySelector(".confirm-subtask-btn")?.addEventListener(
          "click",
          (e) => {
            e.stopPropagation();
            const input = item.querySelector(
              ".subtask-title-input",
            );
            if (input?.value.trim()) {
              task.subtasks.push({
                title: input.value.trim(),
                status: "waiting",
              });
              renderTasks();
            }
          },
        );

        item.querySelector(".cancel-subtask-btn")?.addEventListener(
          "click",
          (e) => {
            e.stopPropagation();
            item.querySelector(".add-subtask-form")?.classList.add(
              "hidden",
            );
          },
        );

        item.querySelectorAll(".subtask-status-select").forEach(
          (select) => {
            select.addEventListener("change", (e) => {
              e.stopPropagation();
              const idx = parseInt(
                select.closest(".subtask-item").dataset
                .subtaskIdx,
              );
              task.subtasks[idx].status = select.value;
              renderTasks();
            });
          },
        );

        item.querySelectorAll(".delete-subtask-btn").forEach((btn) => {
          btn.addEventListener("click", (e) => {
            e.stopPropagation();
            const idx = parseInt(
              btn.closest(".subtask-item").dataset.subtaskIdx,
            );
            task.subtasks.splice(idx, 1);
            renderTasks();
          });
        });
      });
    };

    document.querySelectorAll(".task-list").forEach((list) => {
      list.addEventListener("dragover", (e) => {
        e.preventDefault();
        e.dataTransfer.dropEffect = "move";
      });

      list.addEventListener("drop", (e) => {
        e.preventDefault();
        if (draggedTask) {
          draggedTask.status = list.dataset.status;
          renderTasks();
        }
      });
    });

    document.querySelectorAll(".add-task-btn").forEach((btn) => {
      btn.addEventListener("click", () => {
        const newTask = {
          id: taskIdCounter++,
          title: "",
          description: "",
          status: "waiting",
          collapsed: false,
          isNew: true,
          isEditing: false,
          subtasks: [],
        };
        tasks.unshift(newTask);
        renderTasks();
      });
    });

    document.querySelectorAll(".tab-mobile-btn").forEach((btn) => {
      btn.addEventListener("click", () => {
        const tab = btn.dataset.tab;
        currentMobileTab = tab;

        document.querySelectorAll(".tab-mobile-btn").forEach((b) => {
          b.classList.remove("active", "bg-green-500", "text-white");
          b.classList.add("text-coolGray-500");
        });
        btn.classList.add("active", "bg-green-500", "text-white");
        btn.classList.remove("text-coolGray-500");

        document.querySelectorAll(".task-column").forEach((col) => {
          col.classList.add("hidden");
          col.classList.remove("md:block");
        });

        const activeCol = document.querySelector(
          `.${tab === "inprogress" ? "inprogress" : tab}-column`,
        );
        activeCol?.classList.remove("hidden");

        document.querySelectorAll(".task-column").forEach((col) => {
          col.classList.add("md:block");
        });
      });
    });

    renderTasks();
  };

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", init);
  } else {
    init();
  }
})();
