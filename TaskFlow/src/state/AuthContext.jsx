import { createContext, useContext, useState, useEffect, useRef, useMemo } from "react";
import { authApi, LS_USERS } from "./AuthApi";

const AuthContext = createContext(null);

const IDLE_TIMEOUT_MINS = 15;
const IDLE_MS = IDLE_TIMEOUT_MINS * 60 * 1000;

const STORAGE_USER = "authUser";
const STORAGE_REMEMBER = "authRemember";
const STORAGE_TOKEN = "authToken";

export function AuthProvider({ children }) {
  // Load user from localStorage on startup
  const [user, setUser] = useState(null);
  const [remember, setRemember] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const idleTimer = useRef(null);
  const lastActivity = useRef(Date.now());

  // Helper functions Denoted with a 'H:-' prefix
  // H:- Switch between local and session storages depending on the "Remember Me" status
  function currentStorage(rem = remember) {
    return rem? localStorage : sessionStorage;
  }

  // H:- Verify invite code validity
  async function verifyInvite(invitation) {
    return authApi.verifyInvite(invitation);
  }

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
    }, IDLE_MS)
  }

  // Login
  async function login(username = "DemoUser", password = "demo", rememberMe = false) {
    console.debug("[AuthContext] login()", {username, password});
    setLoading(true);
    setError(null);
    try {
      const {user, token } = await authApi.login({username, password});
      setRemember(rememberMe);
      setUser(user);
      const storage = currentStorage(rememberMe);
      storage.setItem(STORAGE_TOKEN, token);

      return user;
    }
    catch (err) {
      setError(err?.message || "Login failed.");
      throw err;
    }
    finally {
      setLoading(false);
    }
  };

  // Logout clears everything
  async function logout() {
    try {
      await authApi.logout();
    }
    catch {}

    setUser(null);

    localStorage.removeItem(STORAGE_USER);
    sessionStorage.removeItem(STORAGE_USER);
    localStorage.removeItem(STORAGE_TOKEN);
    sessionStorage.removeItem(STORAGE_TOKEN);
  };

  // New user registration
  async function register(username, password, inviteCode, rememberMe = false){
    console.debug("[AuthContext] register()", {username, password, inviteCode});
    setLoading(true);
    setError(null);
    try {
      const {user, token} = await authApi.register(username, password, inviteCode);
      setRemember(rememberMe);
      setUser(user);
      const storage = currentStorage(rememberMe);
      storage.setItem(STORAGE_TOKEN, token);

      return user;
    }
    catch (err) {
      setError(err?.message || "Registration failed.");
      throw err;
    }
    finally {
      setLoading(false);
    }
  }

  // Create a new invite code and register it to the user that requested it
  async function generateInvite() {
    if (!user)
    {
      throw Error("Must be logged in to issue invites.");
    }

    return authApi.issueInvite(user.username);
  }

  // Load stored user on app start
  useEffect(() => {
    // Generate a fake invite code for disconnected testing purposes
    authApi.seedIfEmpty();

    const storedRemember = localStorage.getItem(STORAGE_REMEMBER) === "true";
    setRemember(storedRemember);

    const storage = currentStorage(storedRemember);

    const rawUser = storage.getItem(STORAGE_USER);
    if (rawUser) {
      try {
        setUser(JSON.parse(rawUser));
      } catch {
        storage.removeItem(STORAGE_USER);
      }
    }
  }, []);

  // Save whenever user or remember changes
  useEffect(() => {
    const storage = currentStorage(remember);

    if (user) {
      storage.setItem(STORAGE_USER, JSON.stringify(user));
    } else {
      storage.removeItem(STORAGE_USER);
    }

    localStorage.setItem(STORAGE_REMEMBER, remember.toString());
  }, [user, remember]);

  // Detect whenever there is an activity while a user is logged in
  useEffect(() => {
    if (!user) {
      if (idleTimer.current) {
        clearTimeout(idleTimer.current);
      }

      return;
    }

    const events = ["mousemove", "keydown", "click", "scroll", "touchstart"];

    events.forEach((ev) => window.addEventListener(ev, resetIdleTimer));
    // Initialize the timer
    resetIdleTimer();

    return (() => {
      events.forEach((ev) => window.removeEventListener(ev, resetIdleTimer));
      if (idleTimer.current) {
        clearTimeout(idleTimer.current);
      }
    });
  }, [user]);

  const providerValues = useMemo(() => ({
    // User states
    user,
    remember,
    setRemember,

    // Flags
    loading,
    error,

    // Authentication functions
    login,
    logout,
    register,

    // Invitation handling
    verifyInvite,
    generateInvite
  }), [user, remember, loading, error])

  return (
    <AuthContext.Provider value={providerValues}>
      {children}
    </AuthContext.Provider>
  );
}

// Easy hook for accessing auth
export function useAuth() {
  return useContext(AuthContext);
}
