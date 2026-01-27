import { useState } from "react";
import { useNavigate } from 'react-router-dom';
import { useAuth } from "../state/AuthContext.jsx";
import { useToast } from "../state/ToastContext.jsx";
import { validateRegistration, getAuthApiErrors } from "../utils/validators.js";
import AppTitle from "../components/AppTitle.jsx";

export default function LoginScreen() {
    const { register, loading } = useAuth();
    const { error: toastError, success } = useToast();
    const navigate = useNavigate();
    
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");
    const [invitation, setInvitation] = useState("");
    const [remember, setRemember] = useState(false);
    const [errors, setErrors] = useState({});

    function setFieldError(fieldName, message) {
        setErrors((err) => ({ ...err, [fieldName]: message}));
    }
    
    function clearFieldError(fieldName) {
        setErrors((err) => ({ ...err, [fieldName]: undefined}));
    }

    function handleBlur(ev) {
        const name = ev.target.id;
        const draft = {username, password, invitation};
        const result = validateRegistration(draft);

        if (result[name]) {
            setFieldError(name, result[name]);
        }
        else {
            clearFieldError(name);
        }
    }

    async function handleSubmit(ev) {
        ev.preventDefault();
        const user = {username, password, invitation, remember};
        
        // Client-side validation
        const clientErrors = validateRegistration(user);
        if (Object.keys(clientErrors).length) {
            setErrors(clientErrors);

            // Focus on the first error
            const first = Object.keys(clientErrors)[0];
            document.querySelector(`[name="${first}"]`)?.focus();
            return;
        }

        // Try user registration
        try {
            await register(user);
            success("Your account has been created.", "Registration successful");
            navigate("/app");
        } catch (error) {
            const {errors, message} = getAuthApiErrors(error);
            if (Object.keys(errors).length) {
                setErrors(errors);
                toastError(message);

                // Focus on the first error
                const first = Object.keys(errors)[0];
                document.querySelector(`[name="${first}"]`)?.focus();
            }
        }
    }

    function className(key) {
        return `authInput ${errors[key] ? "authInput--error" : ""}`;
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
                        <input
                            id="invitation"
                            value={invitation}
                            type="text"
                            placeholder="Enter your invitation code"
                            autoComplete="off"
                            className={className("invitation")}
                            onBlur={handleBlur}
                            onChange={(ev) => setInvitation(ev.target.value)}/>
                        {errors.invitation && <div className="validateHelp">{errors.invitation}</div>}
                    </div>
                    <div className="mb-2">
                        <label className="block mb-2 font-medium" htmlFor="username">Username</label>
                        <input
                            id="username"
                            value={username}
                            type="text"
                            placeholder="Enter your username"
                            autoComplete="off"
                            className={className("username")}
                            onBlur={handleBlur}
                            onChange={(ev) => setUsername(ev.target.value)}/>
                        {errors.username && <div className="validateHelp">{errors.username}</div>}
                    </div>
                    <div className="mb-3">
                        <label className="block mb-2 font-medium" htmlFor="password">Password</label>
                        <input
                            id="password"
                            value={password}
                            type="password"
                            placeholder="Enter your password"
                            autoComplete="off"
                            className={className("password")}
                            onBlur={handleBlur}
                            onChange={(ev) => setPassword(ev.target.value)}/>
                        {errors.password && <div className="validateHelp">{errors.password}</div>}
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
                    <button className="greenButton" type="submit" disabled={loading} >{loading ? "Registering..." : "Register"}</button>
                </form>
            </div>
            </section>
        </>
    );
}