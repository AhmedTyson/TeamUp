/**
 * @date 2026-11-21
 * @purpose Root authentication context providing JWT claim hints (sub, role, org_admin[]), session hydration, and live refresh/expiry sync.
 */
import { useQueryClient } from "@tanstack/react-query";
import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from "react";
import { useLocation, useNavigate } from "react-router-dom";
import {
  authApi,
  authClientEvents,
  USER_ROLES,
  type AuthResult,
  type AuthStudentSummary,
  type LoginPayload,
  type RegisterPayload,
} from "@/api";
import { decodeJwtPayload, type DecodedJwtClaims } from "@/lib/jwt";
import { queryKeys } from "@/lib/queryKeys";
import { tokenStorage } from "@/lib/tokenStorage";

export interface AuthContextValue {
  user: AuthStudentSummary | null;
  claims: DecodedJwtClaims | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  isPlatformAdmin: boolean;
  isOrgAdmin: boolean;
  orgAdminIds: readonly number[];
  isOrgAdminOf: (organizationId: number) => boolean;
  login: (payload: LoginPayload) => Promise<AuthResult>;
  register: (payload: RegisterPayload) => Promise<AuthResult>;
  logout: () => Promise<void>;
  refreshSession: () => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export interface AuthProviderProps {
  children: ReactNode;
  /** Optional initial user override useful in component unit tests. */
  initialUser?: AuthStudentSummary | null;
}

export function AuthProvider({ children, initialUser }: AuthProviderProps) {
  const queryClient = useQueryClient();
  const navigate = useNavigate();
  const location = useLocation();

  const hasInitialUserOverride = initialUser !== undefined;
  const [user, setUser] = useState<AuthStudentSummary | null>(initialUser ?? null);
  const [claims, setClaims] = useState<DecodedJwtClaims | null>(() =>
    decodeJwtPayload(tokenStorage.getAccessToken()),
  );
  const [isLoading, setIsLoading] = useState<boolean>(() => {
    if (hasInitialUserOverride) {
      return false;
    }
    return Boolean(tokenStorage.getAccessToken() || tokenStorage.getRefreshToken());
  });

  const applyAuthResult = useCallback(
    (result: AuthResult) => {
      tokenStorage.setTokens(result.accessToken, result.refreshToken);
      setClaims(decodeJwtPayload(result.accessToken));
      setUser(result.student);
      queryClient.setQueryData(queryKeys.auth.me, result.student);
    },
    [queryClient],
  );

  const clearLocalSession = useCallback(() => {
    tokenStorage.clear();
    setUser(null);
    setClaims(null);
    queryClient.clear();
  }, [queryClient]);

  const refreshSession = useCallback(async () => {
    const accessToken = tokenStorage.getAccessToken();
    const refreshToken = tokenStorage.getRefreshToken();

    if (!accessToken && !refreshToken) {
      setUser(null);
      setClaims(null);
      setIsLoading(false);
      return;
    }

    setIsLoading(true);
    try {
      const student = await authApi.getCurrentStudent();
      const latestAccessToken = tokenStorage.getAccessToken();
      setClaims(decodeJwtPayload(latestAccessToken));
      setUser(student);
      queryClient.setQueryData(queryKeys.auth.me, student);
    } catch {
      clearLocalSession();
    } finally {
      setIsLoading(false);
    }
  }, [clearLocalSession, queryClient]);

  // Hydrate authenticated session on initial mount if stored tokens exist
  useEffect(() => {
    if (hasInitialUserOverride) {
      return;
    }

    const accessToken = tokenStorage.getAccessToken();
    const refreshToken = tokenStorage.getRefreshToken();
    if (!accessToken && !refreshToken) {
      setIsLoading(false);
      return;
    }

    const controller = new AbortController();
    let active = true;

    authApi
      .getCurrentStudent(controller.signal)
      .then((student) => {
        if (!active) {
          return;
        }
        setClaims(decodeJwtPayload(tokenStorage.getAccessToken()));
        setUser(student);
        queryClient.setQueryData(queryKeys.auth.me, student);
      })
      .catch(() => {
        if (!active || controller.signal.aborted) {
          return;
        }
        clearLocalSession();
      })
      .finally(() => {
        if (active) {
          setIsLoading(false);
        }
      });

    return () => {
      active = false;
      controller.abort();
    };
  }, [clearLocalSession, hasInitialUserOverride, queryClient]);

  // Keep decoded JWT claims & redirect behavior synced with Axios silent refresh events
  useEffect(() => {
    const unsubRefresh = authClientEvents.onTokensRefreshed((newAccessToken) => {
      setClaims(decodeJwtPayload(newAccessToken));
    });

    const unsubExpired = authClientEvents.onSessionExpired(() => {
      clearLocalSession();
      if (!location.pathname.startsWith("/auth/")) {
        void navigate("/auth/login?reason=session_expired", {
          replace: true,
          state: { from: location },
        });
      }
    });

    return () => {
      unsubRefresh();
      unsubExpired();
    };
  }, [clearLocalSession, location, navigate]);

  const login = useCallback(
    async (payload: LoginPayload): Promise<AuthResult> => {
      const result = await authApi.login(payload);
      applyAuthResult(result);
      return result;
    },
    [applyAuthResult],
  );

  const register = useCallback(
    async (payload: RegisterPayload): Promise<AuthResult> => {
      const result = await authApi.register(payload);
      applyAuthResult(result);
      return result;
    },
    [applyAuthResult],
  );

  const logout = useCallback(async (): Promise<void> => {
    try {
      if (tokenStorage.getAccessToken()) {
        await authApi.logout();
      }
    } catch {
      // Best-effort server logout; always clear local session even if token already expired.
    } finally {
      clearLocalSession();
    }
  }, [clearLocalSession]);

  const effectiveRole = user?.role ?? claims?.role ?? null;
  const isPlatformAdmin = effectiveRole === USER_ROLES.PlatformAdmin;
  const isOrgAdmin = effectiveRole === USER_ROLES.OrgAdmin || isPlatformAdmin;
  const orgAdminIds = useMemo(() => claims?.orgAdminIds ?? [], [claims]);

  const isOrgAdminOf = useCallback(
    (organizationId: number): boolean => {
      if (isPlatformAdmin) {
        return true;
      }
      return orgAdminIds.includes(organizationId);
    },
    [isPlatformAdmin, orgAdminIds],
  );

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      claims,
      isAuthenticated: Boolean(user),
      isLoading,
      isPlatformAdmin,
      isOrgAdmin,
      orgAdminIds,
      isOrgAdminOf,
      login,
      register,
      logout,
      refreshSession,
    }),
    [
      user,
      claims,
      isLoading,
      isPlatformAdmin,
      isOrgAdmin,
      orgAdminIds,
      isOrgAdminOf,
      login,
      register,
      logout,
      refreshSession,
    ],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) {
    throw new Error("useAuth() must be used within an <AuthProvider />.");
  }
  return ctx;
}
