
import React, { useState } from 'react'
import Navbar from '../components/Navbar'
import TaskColumn from '../components/TaskColumn'
import Watermark from '../components/Watermark'
import useAppStore from '../state/store'

export default function Dashboard(){
  const [activeTab, setActiveTab] = useState('todo')

  return (
    <div className="min-h-screen relative">
      <Navbar />
      <main className="max-w-7xl mx-auto px-4 py-6">
        {/* Tabs for small screens */}
        <div className="flex lg:hidden items-center justify-around border-b border-neutral-200 dark:border-neutral-800 mb-4">
          {['todo','progress','finished'].map(t => (
            <button key={t} onClick={()=>setActiveTab(t)} className={`py-2 px-3 rounded-full text-sm font-medium ${activeTab===t ? 'bg-neutral-100 dark:bg-neutral-800' : ''}`}>{
              t==='todo'?'To do': t==='progress'?'In Progress':'Finished'
            }</button>
          ))}
        </div>

        {/* Columns */}
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6 h-[calc(100vh-12rem)]">
          <div className={`border-neutral-200 dark:border-neutral-800 ${'todo'!==activeTab?'hidden lg:block':''} lg:border-r`}>
            <TaskColumn column="todo" title="To do" />
          </div>
          <div className={`border-neutral-200 dark:border-neutral-800 ${'progress'!==activeTab?'hidden lg:block':''} lg:border-r`}>
            <TaskColumn column="progress" title="In Progress" />
          </div>
          <div className={`${'finished'!==activeTab?'hidden lg:block':''}`}>
            <TaskColumn column="finished" title="Finished" />
          </div>
        </div>
      </main>
      <Watermark />
    </div>
  )
}
