
import React from 'react'
import TaskCard from './TaskCard'
import useAppStore from '../state/store'
import { FaPlus } from 'react-icons/fa'

export default function TaskColumn({ column, title }){
  const tasks = useAppStore(s => s.tasks[column])
  const addTask = useAppStore(s => s.addTask)

  return (
    <section className="h-full flex flex-col">
      <div className="flex items-center justify-between mb-3">
        <h2 className="text-2xl font-bold">{title}</h2>
        <button className="p-2 rounded-full bg-neutral-100 dark:bg-neutral-800" onClick={()=>addTask(column)} aria-label="Add task">
          <FaPlus />
        </button>
      </div>

      <div className="flex-1 overflow-auto pr-2 space-y-3">
        {tasks.map(t => (
          <TaskCard key={t.id} column={column} task={t} />
        ))}
      </div>
    </section>
  )
}
