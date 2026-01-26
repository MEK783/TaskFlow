export const LS_USERS = "auth_users_v1";
const LS_INVITES = "auth_invites_v1";

function load(key, fallback) {
  try { return JSON.parse(localStorage.getItem(key)) ?? fallback; }
  catch { return fallback; }
}
function save(key, val) {
  localStorage.setItem(key, JSON.stringify(val));
}

function now() { return Date.now(); }
function genId() { return `u_${Math.random().toString(36).slice(2, 10)}`; }
function genCode() { return Math.random().toString(36).toUpperCase().slice(2, 10); } // 8 chars

function normalizeUsername(u) { return u.trim().toLowerCase(); }
const USERNAME_RGX = /^[a-zA-Z0-9_.-]{3,32}$/;

export const authApi = {
  // ---- INVITES ----
  async issueInvite(invitedBy) {
    const invites = load(LS_INVITES, []);
    const code = genCode();
    const invite = {
      code,
      invitedBy,
      createdAt: now(),
      // 15 day validity period
      expiresAt: now() + 15 * 24 * 60 * 60 * 1000,
      usedAt: null,
      usedBy: null,
    };
    invites.push(invite);
    save(LS_INVITES, invites);
    return { code, expiresAt: invite.expiresAt };
  },

  async verifyInvite(code) {
    const invites = load(LS_INVITES, []);
    const inv = invites.find(i => i.code === code);
    if (!inv) return { ok: false, reason: "not_found" };
    if (inv.usedAt) return { ok: false, reason: "used" };
    if (inv.expiresAt && inv.expiresAt < now()) return { ok: false, reason: "expired" };
    return { ok: true, invitedBy: inv.invitedBy, expiresAt: inv.expiresAt };
  },

  // ---- REGISTRATION ----
  async register( username, password, code ) {
    console.debug("[authApi.register] invites store", load(LS_INVITES, []));
    const uname = (username ?? "").trim();
    if (!USERNAME_RGX.test(uname)) {
      return Promise.reject({ code: "invalid_username", message: "Username must be 3–32 chars: letters, numbers, _.-" });
    }
    if (!password || password.length < 6) {
      return Promise.reject({ code: "weak_password", message: "Password must be at least 6 characters (demo rule)." });
    }
    const invites = load(LS_INVITES, []);
    const invite = invites.find(i => i.code === code.trim().toUpperCase());
    if (!invite) throw { code: "invite_not_found", message: "Invite code not found." };
    if (invite.usedAt) throw { code: "invite_used", message: "Invite code already used." };
    if (invite.expiresAt && invite.expiresAt < now()) throw { code: "invite_expired", message: "Invite code expired." };

    const users = load(LS_USERS, []);
    const unameNorm = normalizeUsername(uname);
    if (users.some(u => u.usernameNorm === unameNorm)) {
      throw { code: "username_taken", message: "Username is already taken." };
    }

    // DEMO ONLY: store cleartext password; replace with server-side hashing later.
    const user = {
      id: genId(),
      username: uname,
      usernameNorm: unameNorm,
      roles: ["user"],
      createdAt: now(),
    };
    users.push({ ...user, password });
    save(LS_USERS, users);

    // mark invite used
    invite.usedAt = now();
    invite.usedBy = user.id;
    save(LS_INVITES, invites);

    // return a “session-like” payload
    return { user, token: `demo.${user.id}.${Date.now()}` };
  },

  // ---- LOGIN ----
  async login({ username, password }) {
    console.debug("[authApi.login] incoming", {username, password});
    console.debug("[authApi.login] users store", load(LS_USERS, []));
    const users = load(LS_USERS, []);
    const unameNorm = normalizeUsername(username ?? "");
    const record = users.find(u => u.usernameNorm === unameNorm);
    if (!record || record.password !== password) {
      throw { code: "invalid_credentials", message: "Invalid username or password." };
    }
    const { password: _omit, ...user } = record;
    return { user, token: `demo.${user.id}.${Date.now()}` };
  },

  async logout() {
    return true;
  },

  // Utility for dev/testing: seed an initial invite if none exists.
  seedIfEmpty() {
    const invites = load(LS_INVITES, []);
    if (invites.length === 0) {
      const code = "WELCOME01";
      const invite = { code, invitedBy: "system", createdAt: now(), expiresAt: now() + 14*24*60*60*1000, usedAt: null, usedBy: null };
      invites.push(invite);
      save(LS_INVITES, invites);
      // eslint-disable-next-line no-console
      console.info(`[mockAuthApi] Seeded invite code: ${code}`);
    }
  },
};
