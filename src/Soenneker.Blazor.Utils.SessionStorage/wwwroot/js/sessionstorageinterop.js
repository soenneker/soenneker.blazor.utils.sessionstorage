function validateKey(key) {
    if (typeof key !== "string" || key.trim().length === 0)
        throw new Error("Session storage key must be a non-empty string.");
}

export function get(key) {
    validateKey(key);

    const storage = typeof window === "undefined" ? null : window.sessionStorage;
    if (storage == null)
        return null;

    return storage.getItem(key);
}

export function set(key, value) {
    validateKey(key);

    if (typeof value !== "string")
        throw new Error("Session storage value must be a string.");

    const storage = typeof window === "undefined" ? null : window.sessionStorage;
    if (storage == null)
        return;

    storage.setItem(key, value);
}

export function remove(key) {
    validateKey(key);

    const storage = typeof window === "undefined" ? null : window.sessionStorage;
    if (storage == null)
        return;

    storage.removeItem(key);
}

export function clear() {
    const storage = typeof window === "undefined" ? null : window.sessionStorage;
    if (storage == null)
        return;

    storage.clear();
}

export function containsKey(key) {
    validateKey(key);

    const storage = typeof window === "undefined" ? null : window.sessionStorage;
    if (storage == null)
        return false;

    return storage.getItem(key) !== null;
}

export function getKeys() {
    const storage = typeof window === "undefined" ? null : window.sessionStorage;
    if (storage == null)
        return [];

    const keys = [];

    for (let i = 0, length = storage.length; i < length; i++) {
        const key = storage.key(i);

        if (key != null)
            keys.push(key);
    }

    return keys;
}

export function getLength() {
    const storage = typeof window === "undefined" ? null : window.sessionStorage;
    if (storage == null)
        return 0;

    return storage.length;
}
