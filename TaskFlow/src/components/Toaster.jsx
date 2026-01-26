import { createContext, useContext, useEffect, useRef, useState } from "react";

const ToastContext = createContext(null);
export const TOAST_TYPE = Object.freeze({
    INFO: "info",
    WARN: "warning",
    ERROR: "error",
    OK: "success",
    MSG: "message"
});

export default function Toast({children }) {
    const [toasts, setToasts] = useState([]);

    function push(toast) {
        const id= Math.random().toString(36).slice(2);
        setToasts((prevToasts) => [...prevToasts, {id, ...toast}]);

        return id;
    }

    function pull(toastId) {
        setToasts((prevToasts) => prevToasts.filter((toast) => toast.id !== toastId));
    }

    return (
        <ToastContext.Provider value={{push, pull}}>
            {children}
            <div className="fixed right-4 top-4 z-[9999] space-y-2">
                {toasts.map(toast) => (
                    <div key={toast.id} className=`rounded-md px-3 py-2 text-sm shadow-md`
                )}
            </div>
        </ToastContext.Provider>
    )
}