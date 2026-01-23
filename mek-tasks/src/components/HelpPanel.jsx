
import React from 'react'

export default function HelpPanel({ open, onClose }){
  if(!open) return null
  return (
    <div className="fixed inset-0 z-40">
      <div className="absolute inset-0 bg-black/20" onClick={onClose}></div>
      <div className="absolute top-20 right-6 w-96 bg-white dark:bg-neutral-900 border border-neutral-200 dark:border-neutral-700 rounded-2xl shadow-xl p-4">
        <h3 className="text-lg font-semibold mb-2">How this site works</h3>
        <ul className="list-disc pl-5 space-y-1 text-sm text-neutral-700 dark:text-neutral-300">
          <li>Add tasks in <b>To do</b> via the + button.</li>
          <li>Drag tasks between columns or use arrows to move up/down.</li>
          <li>Click the subtask icon to reveal subtasks. Only one expands at a time.</li>
          <li>Use the theme switch to toggle light/dark.</li>
        </ul>
      </div>
    </div>
  )
}
