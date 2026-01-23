
import { create } from 'zustand'
import { nanoid } from './util'

const initialTasks = {
  todo: [
    { id: nanoid(), title: 'Sample task', description: 'Edit me', subtasks: [], expanded: true },
  ],
  progress: [],
  finished: []
}

const useAppStore = create((set, get) => ({
  theme: 'dark',
  setTheme: (t) => set({ theme: t }),

  auth: { loggedIn: false, username: null },
  login: (username) => set({ auth: { loggedIn: true, username } }),
  logout: () => set({ auth: { loggedIn: false, username: null } }),

  invites: [], // {id, code, createdAt, claimed:false}
  generateInvite: () => set(state => ({
    invites: [...state.invites, { id: nanoid(), code: Math.random().toString(36).slice(2,8).toUpperCase(), createdAt: new Date().toISOString(), claimed: false }]
  })),

  tasks: initialTasks,
  addTask: (column) => set(state => {
    const t = { id: nanoid(), title: 'New task', description: '', subtasks: [], expanded: true }
    return { tasks: { ...state.tasks, [column]: [t, ...state.tasks[column]] } }
  }),
  updateTask: (column, id, patch) => set(state => {
    const list = state.tasks[column].map(t => t.id === id ? { ...t, ...patch } : t)
    return { tasks: { ...state.tasks, [column]: list } }
  }),
  moveTask: (fromCol, toCol, id, toIndex=0) => set(state => {
    let from = [...state.tasks[fromCol]]
    const idx = from.findIndex(t => t.id === id)
    if(idx === -1) return {}
    const [item] = from.splice(idx,1)
    const to = [...state.tasks[toCol]]
    to.splice(toIndex,0,item)
    return { tasks: { ...state.tasks, [fromCol]: from, [toCol]: to } }
  }),
  reorderTask: (column, fromIndex, toIndex) => set(state => {
    const list = [...state.tasks[column]]
    const [moved] = list.splice(fromIndex,1)
    list.splice(toIndex,0,moved)
    return { tasks: { ...state.tasks, [column]: list } }
  }),
  // Subtasks
  addSubtask: (column, taskId) => set(state => {
    const list = state.tasks[column].map(t => {
      if(t.id !== taskId) return t
      const st = { id: nanoid(), title: 'Subtask', description: '', expanded: false }
      return { ...t, subtasks: [...t.subtasks, st] }
    })
    return { tasks: { ...state.tasks, [column]: list } }
  }),
  toggleOnlySubtask: (column, taskId, subId) => set(state => {
    const list = state.tasks[column].map(t => {
      if(t.id !== taskId) return t
      const subs = t.subtasks.map(s => ({ ...s, expanded: s.id === subId ? !s.expanded : false }))
      return { ...t, subtasks: subs }
    })
    return { tasks: { ...state.tasks, [column]: list } }
  }),
}))

export default useAppStore
