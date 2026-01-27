import * as vars from './sharedVariables.js';

export function validateRegistration({username, password, invitation}) {
    const errors = {};
    if (!invitation?.trim()) {
        errors.invitation = "Invitation code is required.";
    }

    if (!username?.trim()) {
        errors.username = "Username is required.";
    }

    if (!vars.USERNAME_RGX.test(username.trim())) {
        errors.username = "Username has to be between 3-32 characters long";
    }

    if (!password) {
        errors.password = "Password is required.";
    }

    if (password.length < 6) {
        errors.password = "Password has to be at least 6 characters long."
    }

    return errors;
}

export function validateLogin({username, password}) {
    const errors = {};

    if(!username?.trim()) {
        errors.username = "Username is required.";
    }

    if (!password) {
        errors.password = "Password is required.";
    }

    return errors;
}

export function getAuthApiErrors(error) {
    const { code, message } = error || {};
    const errors = {};

    switch (code) {
        case vars.AUTH_ERRORS.TAKEN:
            errors.username = "This username is already in use.";
            break;
        case vars.AUTH_ERRORS.INVALID_USER:
            errors.username = "Invalid username format.";
            break;
        case vars.AUTH_ERRORS.WEAK_PASS:
            errors.password = "Password is too weak.";
            break;
        case vars.AUTH_ERRORS.INVALID_INVITE:
            errors.invitation = "Invitation code not found.";
            break;
        case vars.AUTH_ERRORS.USED_INVITE:
            errors.invitation = "This invitation code has already been used.";
            break;
        case vars.AUTH_ERRORS.EXPIRED:
            errors.invitation = "Invitation code is expired.";
            break;
        case vars.AUTH_ERRORS.BAD_LOGIN:
            errors.password = "Invalid username or password.";
            break;
        default:
            errors.general = "The authentication process has encountered an error";
            break;
    }

    return {errors, message: message || "Something went wrong." };
}