// Regex string for username validity
export const USERNAME_RGX = /^[a-zA-Z0-9_.-]{3,32}$/;

// Enum for the various authentication api errors
export const AUTH_ERRORS = Object.freeze({
    TAKEN:          "username_taken",
    INVALID_USER:   "invalid_username",
    WEAK_PASS:      "weak_passwored",
    INVALID_INVITE: "invite_not_found",
    USED_INVITE:    "invite_used",
    EXPIRED:        "invite_expired",
    BAD_LOGIN:      "invalid_credentials"
})

// Enum for the TOAST type and it's styling
export const TOAST_TYPE = Object.freeze({
    INFO:   "info",
    WARN:   "warning",
    ERROR:  "error",
    OK:     "success",
    MSG:    "message"
});

// Generic Time-to-live value for elements that need to perform an action after a small defined period
export const DEFAULT_TTL = 5000;