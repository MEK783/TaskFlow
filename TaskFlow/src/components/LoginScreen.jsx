import { useState } from "react";
import { useAuth } from "../state/AuthContext.jsx";
import AppTitle from "./AppTitle.jsx";

export default function LoginScreen() {
    const { login } = useAuth();
    const [username, setUsername] = useState("Dev");
    const [remember, setRemember] = useState(false);

    function handleSubmit(ev) {
        ev.preventDefault();
        login(username, remember);
    }
    
    return (
        <>
            <section className="flex items-center justify-center py-12 px-4">
            <div className="max-w-md">
                <div className="text-center mb-8">
                <a className="inline-block mb-6" href="#">
                    <AppTitle />
                </a>
                <h1 className="text-2xl md:text-3xl font-bold text-gray-900 tracking-tight mb-2">Welcome back</h1>
                <p className="text-gray-500 font-medium">Sign in to manage your tasks</p>
                </div>
                <form action="#" method="post" onSubmit={handleSubmit}>
                    <div className="mb-6">
                    <label className="block mb-2 text-gray-800 font-medium" htmlFor="username">Username</label>
                    <input className="
                        w-full py-3 px-4 leading-tight rounded-lg shadow-xs
                        text-gray-500 placeholder-gray-400
                        focus:outline-none focus:ring-2 focus:ring-mek-green focus:ring-opacity-50
                        border border-meklight-border dark:border-mek-border"
                        type="text" id="username" placeholder="Enter your username" value={username} onChange={(ev) => setUsername(ev.target.value)}/>
                    </div>
                    <div className="mb-6">
                    <label className="block mb-2 text-gray-800 font-medium" htmlFor="password">Password</label>
                    <input className="
                        w-full py-3 px-4 rounded-lg shadow-xs
                        text-gray-500 leading-tight placeholder-gray-400
                        focus:outline-none focus:ring-2 focus:ring-mek-green focus:ring-opacity-50
                        border border-meklight-border dark:border-mek-border"
                        type="password" id="password" placeholder="Enter your password (Not in use yet)"/>
                    </div>
                    <button className="
                        inline-block py-3 px-7 w-full text-lg leading-7 font-medium text-center
                        text-green-50 bg-meklight-mgreen hover:bg-mek-green
                        focus:ring-2 focus:ring-offset-mek-green focus:ring-opacity-50
                        border border-transparent rounded-md shadow-sm" type="submit" >Log In</button>
                </form>
                <div className="mt-6 text-center">
                    <p className="text-gray-500">
                    <span>Do you have an invite?  </span>
                    <a className="text-mek-green font-medium" href="#">Sign up!</a>
                    </p>
                </div>
            </div>
            </section>
        </>
    );
}