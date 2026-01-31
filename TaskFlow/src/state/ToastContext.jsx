import { createContext, useContext, useEffect, useRef, useState } from "react";
import { TOAST_TYPE, DEFAULT_TTL } from "../utils/sharedVariables";

const ToastContext = createContext(null);

function useAutoDismiss({ id, ttl, onClose }) {
  const timerRef = useRef(null);
  const remainingRef = useRef(ttl ?? DEFAULT_TTL);
  const startedAtRef = useRef(null);

  function start() {
    clear();
    startedAtRef.current = Date.now();
    timerRef.current = setTimeout(() => onClose(id), remainingRef.current);
    console.debug("[ToastItem] start()", ["start triggered"]);
  }

  function pause() {
    if (!timerRef.current) return;
    clearTimeout(timerRef.current);
    console.debug("[ToastItem] pause()", ["pause triggered"]);
    timerRef.current = null;
    remainingRef.current -= Date.now() - startedAtRef.current;
  }

  function clear() {
    if (timerRef.current) {
      clearTimeout(timerRef.current);
      console.debug("[ToastItem] clear()", ["clear triggered"]);
      timerRef.current = null;
    }
  }

  useEffect(() => {
    remainingRef.current = ttl ?? DEFAULT_TTL;
    start();
    return clear;
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id, ttl]); // restart if content/ttl changes

  return { pause, start, clear };
}

function ToastItem({toast, onClose, offset}) {
    const {id, title, message, type, ttl} = toast;
    const {pause, start} = useAutoDismiss({id, ttl, onClose});

    let palette;

    switch(type) {
        case TOAST_TYPE.ERROR:
            palette = "bg-red-hover dark:bg-red-night text-white border-red-ui";
            break;
        case TOAST_TYPE.OK:
            palette = "bg-green-light dark:bg-green-night text-white border-green-ui";
            break;
        case TOAST_TYPE.INFO:
            palette = "bg-blue-500 text-white border-blue-700";
            break;
        default:
            palette = "bg-lightbg dark:bg-nightbg greyText border-border-light dark:border-border-night";
            break;
    };

    const move = `translateY(${offset * 6}px)`;

    return (
        <div
            role="status"
            aria-live="assertive"
            className={`min-w-44 max-w-44 rounded-md border px-4 py-3 shadow-lg ${palette}`}
            style={{transform: move}}
            onClick={() => onClose(id)}
            onMouseEnter={pause}
            onMouseLeave={start}>
            {title && <div className="font-semibold">{title}</div>}
            {message && <div className="text-sm opacity-95">{message}</div>}
            <div className="mt-2 text-[10px] opacity-75">Click or press Esc to dismiss</div>
        </div>
    );
}

export function ToastProvider({children }) {
    const [toasts, setToasts] = useState([]);

    function push({ title, message, type = TOAST_TYPE.ERROR, ttl = DEFAULT_TTL}) {
        const id= Math.random().toString(36).slice(2);
        setToasts((prevToasts) => [...prevToasts, {id, title, message, type, ttl}]);

        return id;
    }

    function pull(toastId) {
        setToasts((prevToasts) => prevToasts.filter((toast) => toast.id !== toastId));
    }

    // Detect  when the escape key is pressed to dismiss the toast
    useEffect(() => {
        function onKey(k) {
            if (k.key === "Escape" && toasts.length) {
                pull(toasts[toasts.length - 1].id);
            }
        }

        window.addEventListener("keydown", onKey);
        return () => window.removeEventListener("keydown", onKey);
    }, [toasts]);

    const providerValue = {
        push,
        pull,
        success: (message, title = "Success", ttl) => push({type: TOAST_TYPE.OK, message, title, ttl}),
        error: (message, title = "Error", ttl) => push({type: TOAST_TYPE.ERROR, message, title, ttl}),
        info: (message, title = "Info", ttl) => push({type: TOAST_TYPE.INFO, title, message, ttl})
    };

    const blocking = toasts.some(toast => toast.type === TOAST_TYPE.ERROR);

    return (
        <ToastContext.Provider value={providerValue}>
            {children}
            {toasts.length > 0 && (
            <div
                aria-live="assertive"
                aria-atomic="true"
                className={`fixed inset-0 z-[9999] grid
                    ${blocking ? "pointer-events-auto backdrop-blur-md bg-nightbg/45 dark:bg-lightbg/45" : "pointer-events-none"} transition-all`}
                style={{ gridTemplateColumns: "1fr", gridTemplateRows: "1fr" }}
                onClick={() => {if (blocking) { const last = toasts[toasts.length - 1]; if (last) pull(last.id); }} } >
                <div className="pointer-events-auto absolute bottom-4 right-4" onClick={(ev) => ev.stopPropagation()}>
                    <div className="rounded-xl p-4 sm:p-5 shadow-lg border border-border-light dark:border-border-night bg-nightbg/80 dark:bg-lightbg/80 backdrop-blur-xs">
                        <div className="flex flex-col gap-2">
                            {toasts.map((toast, index) => (<ToastItem key={toast.id} toast={toast} onClose={pull} offset={index} />))}
                        </div>
                    </div>
                </div>
            </div>)}
        </ToastContext.Provider>
    );
}

export function useToast() {
    const context = useContext(ToastContext);
    if (!context) {
        throw new Error("useToast must be used within a ToastProvider");
    }

    const success = (message, title = "Success") => context.push({type: TOAST_TYPE.OK, title, message});
    const error = (message, title = "Error") => context.push({type: TOAST_TYPE.ERROR, title, message});

    return {success, error };
}