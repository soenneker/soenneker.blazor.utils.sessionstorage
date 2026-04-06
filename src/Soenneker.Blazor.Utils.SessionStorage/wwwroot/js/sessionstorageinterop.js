export function get(key) {
    if (typeof window === "undefined" || window.sessionStorage == null)
        return null;

    return window.sessionStorage.getItem(key);
}

export function set(key, value) {
    if (typeof window === "undefined" || window.sessionStorage == null)
        return;

    window.sessionStorage.setItem(key, value ?? "");
}

export function remove(key) {
    if (typeof window === "undefined" || window.sessionStorage == null)
        return;

    window.sessionStorage.removeItem(key);
}

export function clear() {
    if (typeof window === "undefined" || window.sessionStorage == null)
        return;

    window.sessionStorage.clear();
}

export function containsKey(key) {
    if (typeof window === "undefined" || window.sessionStorage == null)
        return false;

    return window.sessionStorage.getItem(key) !== null;
}

export function getKeys() {
    if (typeof window === "undefined" || window.sessionStorage == null)
        return [];

    const keys = [];

    for (let i = 0; i < window.sessionStorage.length; i++) {
        const key = window.sessionStorage.key(i);

        if (key != null)
            keys.push(key);
    }

    return keys;
}

export function getLength() {
    if (typeof window === "undefined" || window.sessionStorage == null)
        return 0;

    return window.sessionStorage.length;
}
