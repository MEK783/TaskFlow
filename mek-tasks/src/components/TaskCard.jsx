
import React, { useState } from 'react'
import { FaChevronDown, FaChevronUp, FaListUl, FaPlus } from 'react-icons/fa'
import RichTextEditor from './RichTextEditor'
import useAppStore from '../state/store'

export default function TaskCard({ column, task, color }){
  const updateTask = useAppStore(s => s.updateTask)
  const addSubtask = useAppStore(s => s.addSubtask)
  const toggleOnlySubtask = useAppStore(s => s.toggleOnlySubtask)
  const [editing, setEditing] = useState(true)

  const border = {
    todo: 'border-meklight-green dark:border-mek-green',
    progress: 'border-meklight-yellow dark:border-mek-yellow',
    finished: 'border-meklight-purple dark:border-mek-purple'
  }[column]

  return (
    <div className={`border-2 rounded-2xl p-3 bg-white/60 dark:bg-neutral-900 ${border}`}>
      <div className="flex items-center justify-between">
        {editing ? (
          <input value={task.title} onChange={e => updateTask(column, task.id, { title: e.target.value })}
            className="w-full bg-transparent outline-none font-semibold" />
        ) : (
          <h3 className="font-semibold">{task.title}</h3>
        )}
        <button className="p-2 rounded-full hover:bg-neutral-100 dark:hover:bg-neutral-800" onClick={() => setEditing(!editing)}>
          {editing ? <FaChevronUp /> : <FaChevronDown />}
        </button>
      </div>

      {editing ? (
        <div className="mt-2 space-y-2">
          <RichTextEditor value={task.description} onChange={(v)=>updateTask(column, task.id, { description: v })} />
          <div className="text-right">
            <button className="rounded-full bg-neutral-900 text-white px-4 py-1 text-sm dark:bg-neutral-100 dark:text-black" onClick={()=>setEditing(false)}>Save</button>
          </div>
        </div>
      ) : null}

      {/* Subtasks section */}
      <div className="mt-3">
        <div className="flex items-center gap-2 text-sm">
          <FaListUl className="opacity-70" />
          <span className="opacity-70">Subtasks</span>
          <button className="ml-auto p-2 rounded-full hover:bg-neutral-100 dark:hover:bg-neutral-800" onClick={()=>addSubtask(column, task.id)}><FaPlus/></button>
        </div>
        <div className="mt-2 space-y-2">
          {task.subtasks.map(st => (
            <div key={st.id} className={`border rounded-xl p-2 cursor-pointer bg-white/60 dark:bg-neutral-800 
              ${column==='todo' ? 'border-meklight-green dark:border-mek-green' : column==='progress' ? 'border-meklight-yellow dark:border-mek-yellow' : 'border-meklight-purple dark:border-mek-purple'}`}
              onClick={()=>toggleOnlySubtask(column, task.id, st.id)}
            >
              <div className="font-medium">{st.title}</div>
              {st.expanded && (
                <div className="text-sm opacity-80 mt-1">{st.description || 'No description yet.'}</div>
              )}
            </div>
          ))}
        </div>
      </div>
    </div>
  )
}
