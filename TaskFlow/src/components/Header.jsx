import { useAuth } from "../state/AuthContext.jsx";
import { FaGithub, FaLinkedin } from "react-icons/fa";
import { MdLogout, MdHelpOutline } from "react-icons/md";
import { LiaUserPlusSolid } from "react-icons/lia";
import { SiSwagger } from "react-icons/si";
import AppTitle from "./AppTitle.jsx";
import ThemeToggle from "./ThemeToggle.jsx";

export default function Header() {
    const {user, logout } = useAuth();

    return (
        <header className="w-full flex flex-row items-center justify-between">
           <div className="flex items-start gap-3">
                <div className="flex flex-col items-start gap-0.5 mt-2 mb-4">
                    <AppTitle />
                    <div className="flex items-center gap-3 mt-1 text-neutral-700 dark:text-neutral-300 ml-16">
                        <a href="https://github.com/MEK783/TaskFlow" target="_blank" rel="noopener noreferrer" className="p-2 rounded-full hover:bg-neutral-100 dark:hover:bg-neutral-700" aria-label="GitHub">
                        <FaGithub size={18} />
                        </a>
                        <a href="https://www.linkedin.com/in/mark-farrugia-37760b25/" target="_blank" rel="noopener noreferrer" className="p-2 rounded-full hover:bg-neutral-100 dark:hover:bg-neutral-700" aria-label="LinkedIn">
                        <FaLinkedin size={18} />
                        </a>
                        <a href="https://taskflowapi.azurewebsites.net/swagger/index.html" target="_blank" rel="noopener noreferrer" className="p-2 rounded-full hover:bg-neutral-100 dark:hover:bg-neutral-700" aria-label="API Documentation">
                        <SiSwagger size={18} />
                        </a>
                    </div>
                </div>
            </div>
            <div className="flex items-center gap-3 mr-4 -mt-11">
                <ThemeToggle />
                {user && (
                <button className="
                    rounded-full p-0.5 items-center justify-center text-sm font-medium
                    bg-magenta-light/35 dark:bg-magenta-night/35
                    text-magenta-night dark:text-magenta-night
                    border-2 border-magenta-ui
                    hover:bg-magenta-ui/45 hover:border-magenta-hover
                    dark:hover:bg-magenta-ui/60" aria-label="Invites">
                <LiaUserPlusSolid size={30} />
                </button>
                )}
                <button className="rounded-full p-0.5 items-center justify-center text-sm font-medium bg-blue-500 hover:bg-blue-700 text-neutral-200 border-border-light border-2 dark:border-border-night" aria-label="Help">
                <MdHelpOutline size={32}/>
                </button>
                {user && (<button
                className="inline-flex items-center gap-2 rounded-lg px-4 py-1.5 text-base font-semibold text-white bg-red-ui border-border-light dark:border-border-night border-2 hover:bg-red-hover" title="Log out" onClick={logout}
                ><MdLogout />Log out</button>)}
            </div>
        </header>
    )
}