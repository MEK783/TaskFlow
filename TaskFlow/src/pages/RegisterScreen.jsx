import { useState } from "react";
import { useAuth } from "../state/AuthContext.jsx";
import { useNavigate } from 'react-router-dom';
import AppTitle from "../components/AppTitle.jsx";

export default function LoginScreen() {
    const { register } = useAuth();
    const navigate = useNavigate();
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");
    const [invitation, setInvitation] = useState("");
    const [remember, setRemember] = useState(false);

    function handleSubmit(ev) {
        ev.preventDefault();
        const user = (username ?? "").trim();
        register(user, password, invitation, remember);
        navigate('/app');
    }

    return (
        <>
            <section className="flex items-center justify-center py-12 px-4">
            <div className="max-w-md greyText">
                <div className="text-center mb-8">
                <a className="inline-block mb-6" href="#">
                    <AppTitle />
                </a>
                <h1 className="text-2xl md:text-3xl font-bold greyText tracking-tight mb-2">Welcome</h1>
                <p className="font-medium">Sign up to start managing your tasks</p>
                </div>
                <form action="#" method="post" onSubmit={handleSubmit}>
                    <div className="mb-2">
                        <label className="block mb-2 font-medium" htmlFor="invitation">Invitation Code</label>
                    <input className="authInput"
                        type="text" id="invitation" placeholder="Enter your invitation" value={invitation} onChange={(ev) => setInvitation(ev.target.value)}/>
                    </div>
                    <div className="mb-2">
                    <label className="block mb-2 font-medium" htmlFor="username">Username</label>
                    <input className="authInput"
                        type="text" id="username" placeholder="Enter your username" value={username} onChange={(ev) => setUsername(ev.target.value)}/>
                    </div>
                    <div className="mb-3">
                    <label className="block mb-2 font-medium" htmlFor="password">Password</label>
                    <input className="authInput" autoComplete="off"
                        type="password" id="password" placeholder="Enter your password" value={password} onChange={(ev) => setPassword(ev.target.value)}/>
                    </div>
                    <div className="mb-8 flex items-center gap-2">
                        <input className="
                        py-3 px-4 leading-tight rounded-lg shadow-xs
                        checked:accent-green-ui h-4 w-4
                        focus:outline-none focus:ring-2 focus:ring-green-active focus:ring-opacity-50 focus:border-none
                        border border-border-light dark:border-border-night"
                        type="checkbox" id="remember" checked={remember} onChange={(ev) => setRemember(ev.target.checked)} />
                        <label className="block font-medium text-grey-night" htmlFor="remember">Remember me?</label>
                    </div>
                    <button className="
                        inline-block py-3 px-7 w-full text-lg leading-7 font-medium text-center
                        text-green-50 bg-green-ui hover:bg-green-hover
                        focus:ring-2 focus:ring-offset-green-active focus:ring-opacity-50
                        border border-transparent rounded-md shadow-sm" type="submit" >Register</button>
                </form>
            </div>
            </section>
        </>
    );
}