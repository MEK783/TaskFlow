import logo from "/Logo.png"

export default function Footer() {
    return (
        <footer className="bg-nightbg fixed left-0 right-0 bottom-0 shadow">
            <div className="ml-0 mx-auto max-w-7xl px-4 py-3 sm:px-6 lg:px-8 flex items-center">
                <a href="https://github.com/MEK783" target="_blank" rel="noopener noreferrer"><img src={logo} alt="@MEK783™" className="h-11 object-contain" /></a>
                <p className="text-gray-400 text-sm">© {new Date().getFullYear()} <a href="https://github.com/MEK783" target="_blank" rel="noopener noreferrer" className="hover:underline">@MEK783™</a> All rights reserved.</p>
            </div>
        </footer>
    );
}