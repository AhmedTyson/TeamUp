/**
 * @date 2026-11-27
 * @purpose Student campus email verification page handling tokenized links (/auth/verify-email?email=...&token=...) and manual verification for signed-in or signed-out students.
 */
import { useQueryClient } from "@tanstack/react-query";
import { BadgeCheck, KeyRound, Loader2, Mail, Send } from "lucide-react";
import { useEffect, useRef, useState, type FormEvent } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { toast } from "sonner";
import { authApi } from "@/api";
import { Button, ErrorBanner, Input } from "@/components/ui";
import { queryKeys } from "@/lib/queryKeys";
import { useAuth } from "../AuthContext";
import { AuthSplitLayout } from "../components/AuthSplitLayout";

export function VerifyEmailPage() {
  const [searchParams] = useSearchParams();
  const queryClient = useQueryClient();
  const { user, isAuthenticated, refreshSession } = useAuth();

  const paramEmail = searchParams.get("email") ?? "";
  const paramToken = searchParams.get("token") ?? "";
  const hasAutoParams = Boolean(paramEmail.trim() && paramToken.trim());

  const [email, setEmail] = useState(paramEmail || user?.email || "");
  const [token, setToken] = useState(paramToken);
  const [isVerifying, setIsVerifying] = useState(hasAutoParams);
  const [isResending, setIsResending] = useState(false);
  const [verifiedMessage, setVerifiedMessage] = useState<string | null>(null);
  const [error, setError] = useState<unknown>(null);

  const autoAttemptedRef = useRef(false);

  const runVerify = async (targetEmail: string, targetToken: string) => {
    setError(null);
    setIsVerifying(true);
    try {
      const msg = await authApi.verifyEmail({
        email: targetEmail.trim(),
        token: targetToken.trim(),
      });
      setVerifiedMessage(msg);
      toast.success("Email verified! Your Verified Student badge is now active.");
      await queryClient.invalidateQueries({ queryKey: queryKeys.students.all });
      await queryClient.invalidateQueries({ queryKey: queryKeys.auth.me });
      if (isAuthenticated) {
        await refreshSession();
      }
    } catch (err: unknown) {
      setError(err);
    } finally {
      setIsVerifying(false);
    }
  };

  useEffect(() => {
    if (!hasAutoParams || autoAttemptedRef.current) {
      return;
    }
    autoAttemptedRef.current = true;
    void runVerify(paramEmail, paramToken);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [hasAutoParams, paramEmail, paramToken]);

  useEffect(() => {
    if (!email && user?.email) {
      setEmail(user.email);
    }
  }, [email, user?.email]);

  const handleSubmit = (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!email.trim() || !token.trim()) {
      setError(new Error("Enter both your campus email and verification token."));
      return;
    }
    void runVerify(email, token);
  };

  const handleResend = async () => {
    if (!isAuthenticated) {
      return;
    }
    setError(null);
    setIsResending(true);
    try {
      const res = await authApi.sendVerificationEmail();
      toast.success(res.message);
      if (res.alreadyVerified) {
        setVerifiedMessage(res.message);
        await refreshSession();
      }
    } catch (err: unknown) {
      setError(err);
    } finally {
      setIsResending(false);
    }
  };

  return (
    <AuthSplitLayout
      eyebrow="Identity Verification"
      headline="Earn your Verified Student badge."
      showcaseSubtitle="Confirming your campus email validates your academic affiliation across university squads, clubs, and leaderboards."
      heroImageSrc="/images/hero/hero-demo-day.jpg"
      heroImageAlt="University students collaborating during Campus Demo Day"
      heroImagePosition="object-center"
      title="Verify your email"
      description="Open your verification link or paste the token from your inbox below."
      footer={
        <div className="flex flex-wrap items-center justify-between gap-2 text-xs">
          {isAuthenticated ? (
            <Link
              to="/profile/me"
              className="font-semibold text-[#d3fb52] transition-colors hover:text-[#e6ff8a]"
            >
              Back to Profile Studio
            </Link>
          ) : (
            <Link
              to="/auth/login"
              className="font-semibold text-[#d3fb52] transition-colors hover:text-[#e6ff8a]"
            >
              Back to Sign In
            </Link>
          )}
          <Link
            to="/students"
            className="font-medium text-white/75 transition-colors hover:text-[#d3fb52]"
          >
            Browse Student Directory
          </Link>
        </div>
      }
    >
      {verifiedMessage || user?.isEmailVerified ? (
        <div
          role="status"
          className="space-y-4 rounded-2xl border border-[#d3fb52]/35 bg-[#d3fb52]/10 p-5"
        >
          <div className="flex items-start gap-3">
            <BadgeCheck className="mt-0.5 size-6 shrink-0 text-[#d3fb52]" aria-hidden="true" />
            <div className="space-y-1">
              <p className="font-bold text-white">Verified Student Status Active</p>
              <p className="text-xs leading-relaxed text-white/80">
                {verifiedMessage ??
                  "Your campus email is verified and your Verified Student badge is visible across TeamUp."}
              </p>
            </div>
          </div>

          <div className="flex flex-wrap gap-2.5">
            {isAuthenticated ? (
              <Link
                to="/profile/me"
                className="inline-flex h-10 items-center rounded-xl bg-[#d3fb52] px-4 text-sm font-bold text-[#14151c] transition-colors hover:bg-[#deff6e]"
              >
                Open Profile Studio
              </Link>
            ) : (
              <Link
                to="/auth/login"
                className="inline-flex h-10 items-center rounded-xl bg-[#d3fb52] px-4 text-sm font-bold text-[#14151c] transition-colors hover:bg-[#deff6e]"
              >
                Sign in to continue
              </Link>
            )}
          </div>
        </div>
      ) : (
        <div className="space-y-4">
          {isVerifying && (
            <div
              role="status"
              className="flex items-center gap-3 rounded-2xl border border-white/10 bg-[#14151c] p-4 text-sm text-white/80"
            >
              <Loader2 className="size-4 animate-spin text-[#d3fb52]" aria-hidden="true" />
              <span>Verifying your campus email token…</span>
            </div>
          )}

          {error !== null && <ErrorBanner error={error} />}

          <form onSubmit={handleSubmit} noValidate className="space-y-4">
            <Input
              label="Registered Email"
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="student@university.edu"
              leftElement={<Mail className="size-4" aria-hidden="true" />}
              required
            />
            <Input
              label="Verification Code / Token"
              type="text"
              value={token}
              onChange={(e) => setToken(e.target.value)}
              placeholder="Paste verification token"
              leftElement={<KeyRound className="size-4" aria-hidden="true" />}
              required
            />
            <div className="flex flex-wrap gap-2.5 pt-1">
              <Button type="submit" size="lg" isLoading={isVerifying} className="flex-1">
                Verify Email
              </Button>
              {isAuthenticated && (
                <Button
                  type="button"
                  size="lg"
                  variant="outline"
                  isLoading={isResending}
                  leftIcon={<Send className="size-4" aria-hidden="true" />}
                  onClick={() => void handleResend()}
                >
                  Send New Link
                </Button>
              )}
            </div>
          </form>
        </div>
      )}
    </AuthSplitLayout>
  );
}
