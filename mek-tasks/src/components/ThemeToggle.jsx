
import React from 'react'
import useAppStore from '../state/store'

export default function ThemeToggle(){
  const theme = useAppStore(s => s.theme)
  const setTheme = useAppStore(s => s.setTheme)
  const checked = theme === 'dark'

  return (
    <button
      onClick={() => setTheme(checked ? 'light' : 'dark')}
      className={`relative w-14 h-8 rounded-full transition-colors duration-200 
        ${checked ? 'bg-neutral-700' : 'bg-green-400'}`}
      aria-label="Toggle theme"
    >
      <span className={`absolute top-1 left-1 w-6 h-6 rounded-full bg-white shadow transform transition-transform duration-200 
        ${checked ? 'translate-x-6' : 'translate-x-0'}`}/>
    </button>
  )
}
