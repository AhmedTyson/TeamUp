/**
 * @date 2026-11-21
 * @purpose Safe browser storage abstraction for JWT access and refresh tokens.
 */
const ACCESS_TOKEN_KEY = "teamup.accessToken";
const REFRESH_TOKEN_KEY = "teamup.refreshToken";

function getStorage(): Storage | null {
  if (typeof window === "undefined") {
    return null;
  }
  try {
    return window.localStorage;
  } catch {
    return null;
  }
}

export const tokenStorage = {
  getAccessToken(): string | null {
    return getStorage()?.getItem(ACCESS_TOKEN_KEY) ?? null;
  },

  getRefreshToken(): string | null {
    return getStorage()?.getItem(REFRESH_TOKEN_KEY) ?? null;
  },

  setTokens(accessToken: string, refreshToken: string): void {
    const storage = getStorage();
    if (!storage) {
      return;
    }
    storage.setItem(ACCESS_TOKEN_KEY, accessToken);
    storage.setItem(REFRESH_TOKEN_KEY, refreshToken);
  },

  clear(): void {
    const storage = getStorage();
    if (!storage) {
      return;
    }
    storage.removeItem(ACCESS_TOKEN_KEY);
    storage.removeItem(REFRESH_TOKEN_KEY);
  },
};
