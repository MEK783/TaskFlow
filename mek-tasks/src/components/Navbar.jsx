
import React from 'react'
import ThemeToggle from './ThemeToggle'
import { FaGithub, FaLinkedin } from 'react-icons/fa'
import useAppStore from '../state/store'
import logo from '../assets/app-logo.svg'
import { useNavigate } from 'react-router-dom'

export default function Navbar(){
  const { auth, logout } = useAppStore()
  const navigate = useNavigate()

  const handleLoginLogout = () => {
    if(auth.loggedIn){ logout(); navigate('/login') }
    else navigate('/login')
  }

  return (
    <header className="w-full bg-lightbg dark:bg-mek-dark border-b border-neutral-200 dark:border-neutral-800">
      <div className="max-w-7xl mx-auto px-4 py-3 flex items-center justify-between">
        {/* Left: site logo + name + socials under */}
        <div className="flex items-start gap-3">
          <img src={logo} alt="MEK Tasks" className="h-10 w-10 rounded-xl" />
          <div>
            <h1 className="text-xl font-bold dark:text-white">MEK Tasks</h1>
            <div className="flex items-center gap-3 mt-1 text-neutral-700 dark:text-neutral-300">
              <a href="https://github.com/your-repo" className="p-2 rounded-full hover:bg-neutral-100 dark:hover:bg-neutral-800" aria-label="GitHub">
                <FaGithub size={18} />
              </a>
              <a href="https://www.linkedin.com/in/your-handle" className="p-2 rounded-full hover:bg-neutral-100 dark:hover:bg-neutral-800" aria-label="LinkedIn">
                <FaLinkedin size={18} />
              </a>
            </div>
          </div>
        </div>

        {/* Right controls in order: Theme, Invite (if logged), Help, Login/Logout */}
        <div className="flex items-center gap-3">
          <ThemeToggle />

          {auth.loggedIn && (
            <button className="rounded-full px-4 py-2 text-sm font-medium bg-mek.magenta/10 dark:bg-mek.magenta/20 text-pink-600 dark:text-pink-300 border border-pink-300/40">
              Invites
            </button>
          )}

          <button className="rounded-full px-4 py-2 text-sm font-medium bg-neutral-100 dark:bg-neutral-800 text-neutral-800 dark:text-neutral-200 border border-neutral-300 dark:border-neutral-700">
            Help
          </button>

          <button onClick={handleLoginLogout}
            className={`rounded-full px-4 py-2 text-sm font-semibold text-white 
              ${auth.loggedIn ? 'bg-red-500 hover:bg-red-600' : 'bg-green-500 hover:bg-green-600'}`}
          >
            {auth.loggedIn ? 'Log out' : 'Log in'}
          </button>
        </div>
      </div>
    </header>
  )
}
