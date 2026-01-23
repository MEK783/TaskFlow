import { createContext, useContext, useState, useEffect, useRef } from "react";

const AuthContext = createContext(null);

const IDLE_TIMEOUT_MINS = 15;
const IDLE_MS = IDLE_TIMEOUT_MINS * 60 * 1000;

const STORAGE_USER = "authUser";
const STORAGE_REMEMBER = "authRemember";

export function AuthProvider({ children }) {
  // Load user from localStorage on startup
  const [user, setUser] = useState(null);
  const [remember, setRemember] = useState(false);
  const idleTimer = useRef(null);
  const lastActivity = useRef(Date.now());
  // Auto logout after idle period expires
  function resetIdleTimer() {
    lastActivity.current = Date.now();
    if (idleTimer.current) {
      clearTimeout(idleTimer.current);
    }

    idleTimer.current = setTimeout(() => {
      const elapsed = Date.now() - lastActivity.current;

      if (elapsed >= IDLE_MS) {
        logout();
      }
    })
  }

  // Fake login
  function login(username = "DemoUser", rememberMe = false) {
    setRemember(rememberMe);
    const dummyUser = { username };
    setUser(dummyUser);
  };

  // Logout clears everything
  function logout() {
    setUser(null);

    localStorage.removeItem(STORAGE_USER);
    sessionStorage.removeItem(STORAGE_USER);

    if (!remember) {
      sessionStorage.removeItem(STORAGE_USER);
    }
  };

  // Load stored user on app start
  useEffect(() => {
    const storedRemember = localStorage.getItem(STORAGE_REMEMBER) === "true";

    setRemember(storedRemember);

    const storage = storedRemember ? localStorage : sessionStorage;

    const rawUser = storage.getItem(STORAGE_USER);
    if (rawUser) {
      try {
        setUser(JSON.parse(rawUser));
      } catch {
        storage.removeItem(STORAGE_USER);
      }
    }
  }, [user]);

  // Save whenever user or remember changes
  useEffect(() => {
    const storage = remember ? localStorage : sessionStorage;

    if (user) {
      storage.setItem(STORAGE_USER, JSON.stringify(user));
    } else {
      storage.removeItem(STORAGE_USER);
    }

    localStorage.setItem(STORAGE_REMEMBER, remember.toString());
  }, [user, remember]);

  // Detect whenever there is an activity
  useEffect(() => {
    const events = ["mousemove", "keydown", "click", "scroll", "touchstart"];

    events.forEach((ev) => window.addEventListener(ev, resetIdleTimer));
    // Initialize the timer
    resetIdleTimer();

    return (() => {
      events.forEach((ev) => window.removeEventListener(ev, resetIdleTimer));
      clearTimeout(idleTimer.current);
    });
  }, [user]);

  return (
    <AuthContext.Provider value={{ user, login, logout, remember, setRemember }}>
      {children}
    </AuthContext.Provider>
  );
}

// Easy hook for accessing auth
export function useAuth() {
  return useContext(AuthContext);
}
