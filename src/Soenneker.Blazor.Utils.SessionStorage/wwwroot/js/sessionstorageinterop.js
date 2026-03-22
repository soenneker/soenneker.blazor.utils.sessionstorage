export class SessionStorageInterop {
    initialize() {
    }

    get(key) {
        if (typeof window === "undefined" || window.sessionStorage == null)
            return null;

        return window.sessionStorage.getItem(key);
    }

    set(key, value) {
        if (typeof window === "undefined" || window.sessionStorage == null)
            return;

        window.sessionStorage.setItem(key, value ?? "");
    }

    remove(key) {
        if (typeof window === "undefined" || window.sessionStorage == null)
            return;

        window.sessionStorage.removeItem(key);
    }

    clear() {
        if (typeof window === "undefined" || window.sessionStorage == null)
            return;

        window.sessionStorage.clear();
    }

    containsKey(key) {
        if (typeof window === "undefined" || window.sessionStorage == null)
            return false;

        return window.sessionStorage.getItem(key) !== null;
    }

    getKeys() {
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

    getLength() {
        if (typeof window === "undefined" || window.sessionStorage == null)
            return 0;

        return window.sessionStorage.length;
    }
}

window.SessionStorageInterop = new SessionStorageInterop();
