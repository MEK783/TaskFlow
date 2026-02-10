import logo from "/Logo.png"
import { PiHeartbeatFill } from "react-icons/pi";

export default function Footer() {
    return (
        <footer className="bg-nightbg fixed left-0 right-0 bottom-0 shadow flex flex-row items-center justify-center">
            <div className="ml-0 mx-auto max-w-7xl px-4 py-3 sm:px-6 lg:px-8 flex items-center">
                <a href="https://github.com/MEK783" target="_blank" rel="noopener noreferrer">
                    <img src={logo} alt="@MEK783™" className="h-11 object-contain" />
                </a>
                <p className="text-gray-400 text-sm">© {new Date().getFullYear()} <a href="https://github.com/MEK783" target="_blank" rel="noopener noreferrer" className="hover:underline">@MEK783™</a> All rights reserved.</p>
            </div>
            <div className="flex items-center gap-3 mr-4 -mt-11">
                <button className={`
                    inline-flex items-center gap-2 rounded-lg
                    px-4 py-1.5 text-base font-semibold text-red-active
                    border-border-light border-2 hover:bg-red-hover`}  onClick={logout}><PiHeartbeatFill /></button>
            </div>
        </footer>
    );
}