/**
 * @date 2026-11-22
 * @purpose Public auth module exports consumed across M1–M4 (guards, hooks, auth pages, and validation schemas).
 */
export { AuthProvider, useAuth } from "./AuthContext";
export type { AuthContextValue, AuthProviderProps } from "./AuthContext";
export { ProtectedRoute } from "./ProtectedRoute";
export type { ProtectedRouteProps } from "./ProtectedRoute";
export { RoleGuard } from "./RoleGuard";
export type { RoleGuardProps } from "./RoleGuard";
export { GuestOnlyRoute } from "./GuestOnlyRoute";
export type { GuestOnlyRouteProps } from "./GuestOnlyRoute";

export { AuthSplitLayout } from "./components/AuthSplitLayout";
export type { AuthSplitLayoutProps } from "./components/AuthSplitLayout";
export { PasswordStrengthHints } from "./components/PasswordStrengthHints";
export type { PasswordStrengthHintsProps } from "./components/PasswordStrengthHints";
export {
  useAcademicCatalogQuery,
  useForgotPasswordMutation,
  useResetPasswordMutation,
} from "./hooks/useAcademicCatalog";
export {
  loginSchema,
  registerSchema,
  forgotPasswordSchema,
  resetPasswordSchema,
  evaluatePasswordStrength,
} from "./schemas/authSchemas";
export type {
  LoginFormValues,
  RegisterFormValues,
  ForgotPasswordFormValues,
  ResetPasswordFormValues,
  PasswordStrengthResult,
} from "./schemas/authSchemas";
// Pages are intentionally NOT re-exported here: this barrel is imported by main-chunk modules
// (providers, Navbar, guards), and re-exporting the pages pulls them into the main bundle,
// defeating the React.lazy() route splitting in app/routes/authRoutes.tsx.
// Import pages directly from "./pages/<Page>" (tests) or via lazy routes (app).
