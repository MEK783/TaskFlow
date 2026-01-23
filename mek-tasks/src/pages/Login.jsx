
import React, { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import useAppStore from '../state/store'

export default function Login(){
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const login = useAppStore(s => s.login)
  const navigate = useNavigate()

  const submit = (e) => {
    e.preventDefault()
    login(username || 'user')
    navigate('/app')
  }

  return (
    <div className="min-h-screen grid place-items-center">
      <form onSubmit={submit} className="w-full max-w-md bg-white dark:bg-neutral-900 border border-neutral-200 dark:border-neutral-800 rounded-2xl p-6 space-y-4">
        <h2 className="text-2xl font-bold mb-2">Log in</h2>
        <div>
          <label className="block text-sm mb-1">Username</label>
          <input value={username} onChange={e=>setUsername(e.target.value)} className="w-full px-3 py-2 rounded-xl border border-neutral-300 dark:border-neutral-700 bg-white dark:bg-neutral-800" />
        </div>
        <div>
          <label className="block text-sm mb-1">Password</label>
          <input type="password" value={password} onChange={e=>setPassword(e.target.value)} className="w-full px-3 py-2 rounded-xl border border-neutral-300 dark:border-neutral-700 bg-white dark:bg-neutral-800" />
        </div>
        <button className="w-full rounded-full bg-green-500 hover:bg-green-600 text-white py-2 font-semibold">Log in</button>
        <div className="text-sm text-center">
          <Link to="/register" className="underline">Create a new account</Link>
        </div>
      </form>
    </div>
  )
}
