
import React, { useEffect } from 'react'
import { Routes, Route, Navigate, useLocation } from 'react-router-dom'
import Dashboard from './pages/Dashboard'
import Login from './pages/Login'
import Register from './pages/Register'
import useAppStore from './state/store'

export default function App(){
  const theme = useAppStore(s => s.theme)
  const setTheme = useAppStore(s => s.setTheme)

  // Load persisted theme
  useEffect(() => {
    const saved = localStorage.getItem('mek-theme')
    if(saved && (saved === 'light' || saved === 'dark')) setTheme(saved)
  }, [])

  useEffect(() => {
    document.documentElement.classList.toggle('dark', theme === 'dark')
    localStorage.setItem('mek-theme', theme)
  }, [theme])

  return (
    <Routes>
      <Route path="/" element={<Navigate to="/app" />} />
      <Route path="/app" element={<Dashboard />} />
      <Route path="/login" element={<Login />} />
      <Route path="/register" element={<Register />} />
      <Route path="*" element={<Navigate to="/app" />} />
    </Routes>
  )
}
