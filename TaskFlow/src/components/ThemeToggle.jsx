import { useState, useEffect } from 'react'
import { MdOutlineDarkMode, MdOutlineLightMode } from 'react-icons/md';
//import useAppStore from '../state/store'

export default function ThemeToggle(){
/*  const theme = useAppStore(s => s.theme)
  const setTheme = useAppStore(s => s.setTheme)
*/
  const [theme, setTheme] = useState("light")
  const isDark = theme === "dark"

  useEffect(() => {
    const root = document.documentElement;
    if (isDark) {root.classList.add("dark");}
    else {root.classList.remove("dark");}
  }, [isDark])

  return (
    <>
    <span className="inset-0 items-center justify-center text-mek-green">{isDark ? <MdOutlineDarkMode size={24} /> : <MdOutlineLightMode size={24} />}</span>
    <button
      onClick={() => setTheme(previous => (previous === "light" ? "dark" : "light"))}
      className="relative w-14 h-8 rounded-full transition-colors duration-200 dark:bg-mek-green bg-meklight-mgreen border border-meklight-border dark:border-mek-border"
      aria-label="Toggle theme"
      role="switch"
      aria-checked={isDark}
    >
      <span className={`absolute top-1 left-1 w-6 h-6 rounded-full bg-white shadow transform transition-transform duration-200 text-center 
        ${isDark ? 'translate-x-6' : 'translate-x-0'}`}/>
    </button>
    </>
  )
}
