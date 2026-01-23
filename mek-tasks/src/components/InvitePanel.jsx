
import React from 'react'
import useAppStore from '../state/store'

export default function InvitePanel({ open, onClose }){
  const invites = useAppStore(s => s.invites)
  const generate = useAppStore(s => s.generateInvite)
  if(!open) return null
  return (
    <div className="fixed inset-0 z-40">
      <div className="absolute inset-0 bg-black/20" onClick={onClose}></div>
      <div className="absolute top-24 right-6 w-md bg-white dark:bg-neutral-900 border border-neutral-200 dark:border-neutral-700 rounded-2xl shadow-xl p-4">
        <div className="flex items-center justify-between mb-2">
          <h3 className="text-lg font-semibold">Invites</h3>
          <button onClick={generate} className="rounded-full px-3 py-1 bg-mek.magenta/10 dark:bg-mek.magenta/20 text-pink-600 dark:text-pink-300 border border-pink-300/40 text-sm">Generate</button>
        </div>
        <ul className="space-y-2 max-h-60 overflow-auto">
          {invites.length === 0 && <li className="text-sm text-neutral-500">No active invites yet.</li>}
          {invites.map(inv => (
            <li key={inv.id} className="flex items-center justify-between px-3 py-2 rounded-xl bg-neutral-50 dark:bg-neutral-800 border border-neutral-200 dark:border-neutral-700 text-sm">
              <span className="font-mono">{inv.code}</span>
              <span className="text-neutral-500">created {new Date(inv.createdAt).toLocaleString()}</span>
            </li>
          ))}
        </ul>
      </div>
    </div>
  )
}
