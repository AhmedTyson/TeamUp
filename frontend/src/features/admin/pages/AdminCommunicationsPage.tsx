/**
 * @date 2026-11-24
 * @purpose PlatformAdmin governance PDF report download/dispatch, sandboxed 7-template email gallery, and opt-in digest composer (/admin/communications).
 */
import {
  Download,
  FileText,
  Loader2,
  Mail,
  Monitor,
  Plus,
  Send,
  Smartphone,
  Sparkles,
  Trash2,
} from "lucide-react";
import { useState } from "react";
import { toast } from "sonner";
import {
  adminApi,
  type AdminReportEmailResultDto,
  type CampusNewsItem,
  type DigestDispatchMode,
  type DigestDispatchResultDto,
} from "@/api";
import { Badge, Button, ConfirmDialog, ErrorBanner, Input } from "@/components/ui";
import { AdminConsoleLayout } from "../components/AdminConsoleLayout";
import {
  useAdminEmailTemplatesQuery,
  useAdminTemplatePreviewQuery,
  useDispatchLeaderboardDigestMutation,
  useDispatchProjectMatchDigestMutation,
  useSendPlatformReportMutation,
} from "../hooks/useAdminHooks";

const TEMPLATE_LABELS: Record<string, string> = {
  "PasswordReset.html": "Password Reset",
  "EmailVerification.html": "Email Verification",
  "EventNotification.html": "Event Notification (M3 UI)",
  "GeneralNotification.html": "General Notification",
  "LeaderboardDigest.html": "Leaderboard + News Digest",
  "ProjectMatchDigest.html": "Portfolio Squad Match Digest",
  "AdminReportEmail.html": "Admin Governance Cover Email",
};

type DigestTab = "leaderboard" | "project-matches";
type ViewportPreview = "desktop" | "mobile";

export function AdminCommunicationsPage() {
  const [reportPeriod, setReportPeriod] = useState("");
  const [reportRecipientEmail, setReportRecipientEmail] = useState("");
  const [isDownloadingPdf, setIsDownloadingPdf] = useState(false);
  const [lastReportReceipt, setLastReportReceipt] = useState<AdminReportEmailResultDto | null>(null);

  const [selectedTemplate, setSelectedTemplate] = useState<string | null>(null);
  const [previewViewport, setPreviewViewport] = useState<ViewportPreview>("desktop");

  const [activeDigestTab, setActiveDigestTab] = useState<DigestTab>("leaderboard");
  const [digestPeriodLabel, setDigestPeriodLabel] = useState("WEEKLY CAMPUS DISPATCH");
  const [newsItems, setNewsItems] = useState<CampusNewsItem[]>([
    {
      category: "COMPETITION · FCAI",
      headline: "Spring Inter-University Hackathon Registration Open",
      summary: "Form cross-faculty engineering squads of 3 to 5 members before Friday.",
    },
  ]);
  const [draftCategory, setDraftCategory] = useState("");
  const [draftHeadline, setDraftHeadline] = useState("");
  const [draftSummary, setDraftSummary] = useState("");

  const [maxMatchesPerStudent, setMaxMatchesPerStudent] = useState(4);
  const [minMatchPercentage, setMinMatchPercentage] = useState(20);
  const [pendingBroadcastKind, setPendingBroadcastKind] = useState<DigestTab | null>(null);
  const [lastDigestReceipt, setLastDigestReceipt] = useState<DigestDispatchResultDto | null>(null);

  const templatesQuery = useAdminEmailTemplatesQuery();
  const activeTemplate = selectedTemplate ?? templatesQuery.data?.[0] ?? null;
  const previewQuery = useAdminTemplatePreviewQuery(activeTemplate);
  const sendReportMutation = useSendPlatformReportMutation();
  const dispatchLeaderboardMutation = useDispatchLeaderboardDigestMutation();
  const dispatchMatchMutation = useDispatchProjectMatchDigestMutation();

  const handleDownloadPdf = async () => {
    setIsDownloadingPdf(true);
    try {
      const { blob, fileName } = await adminApi.downloadPlatformSummaryPdf(
        reportPeriod.trim() || undefined,
      );
      if (typeof URL.createObjectURL === "function") {
        const objectUrl = URL.createObjectURL(blob);
        const anchor = document.createElement("a");
        anchor.href = objectUrl;
        anchor.download = fileName;
        document.body.appendChild(anchor);
        anchor.click();
        anchor.remove();
        if (typeof URL.revokeObjectURL === "function") {
          URL.revokeObjectURL(objectUrl);
        }
      }
      toast.success(`Downloaded ${fileName}`);
    } catch (error: unknown) {
      toast.error(error instanceof Error ? error.message : "Failed to download PDF report.");
    } finally {
      setIsDownloadingPdf(false);
    }
  };

  const handleEmailReport = async () => {
    try {
      const result = await sendReportMutation.mutateAsync({
        reportPeriod: reportPeriod.trim() || undefined,
        recipientEmail: reportRecipientEmail.trim() || undefined,
      });
      setLastReportReceipt(result);
      toast.success(`Report emailed to ${result.recipientEmail}`);
    } catch (error: unknown) {
      toast.error(error instanceof Error ? error.message : "Failed to email governance report.");
    }
  };

  const handleAddNewsItem = () => {
    if (!draftHeadline.trim() || !draftSummary.trim()) {
      toast.error("Provide both a headline and summary for the campus news item.");
      return;
    }
    setNewsItems((prev) => [
      ...prev,
      {
        category: draftCategory.trim() || "PLATFORM UPDATE",
        headline: draftHeadline.trim(),
        summary: draftSummary.trim(),
      },
    ]);
    setDraftCategory("");
    setDraftHeadline("");
    setDraftSummary("");
  };

  const runLeaderboardDispatch = async (mode: DigestDispatchMode) => {
    try {
      const result = await dispatchLeaderboardMutation.mutateAsync({
        mode,
        periodLabel: digestPeriodLabel.trim() || undefined,
        newsItems,
      });
      setLastDigestReceipt(result);
      toast.success(
        mode === "TestToMe"
          ? "Sent test Leaderboard Digest to your admin email."
          : `Dispatched Leaderboard Digest to ${result.sentCount} opted-in student(s).`,
      );
    } catch (error: unknown) {
      toast.error(error instanceof Error ? error.message : "Failed to send Leaderboard Digest.");
    } finally {
      setPendingBroadcastKind(null);
    }
  };

  const runMatchDispatch = async (mode: DigestDispatchMode) => {
    try {
      const result = await dispatchMatchMutation.mutateAsync({
        mode,
        maxMatchesPerStudent,
        minMatchPercentage,
      });
      setLastDigestReceipt(result);
      toast.success(
        mode === "TestToMe"
          ? "Sent test Project Match Digest to your admin email."
          : `Dispatched Project Match Digest to ${result.sentCount} opted-in student(s).`,
      );
    } catch (error: unknown) {
      toast.error(error instanceof Error ? error.message : "Failed to send Project Match Digest.");
    } finally {
      setPendingBroadcastKind(null);
    }
  };

  return (
    <AdminConsoleLayout
      title="Reports & Comms"
      description="Download or email QuestPDF platform governance ledgers, inspect all 7 editorial email templates, and send opt-in leaderboard or portfolio-match digests."
    >
      {/* 1. Executive Governance PDF Card */}
      <section className="rounded-2xl border border-white/[0.085] bg-[#1b1d26] p-6 space-y-5">
        <div className="flex flex-col gap-3 border-b border-white/[0.08] pb-4 sm:flex-row sm:items-center sm:justify-between">
          <div className="space-y-1">
            <span className="inline-flex items-center gap-1.5 font-mono text-[11px] uppercase tracking-wider text-[#d3fb52]">
              <FileText className="size-3.5" aria-hidden="true" />
              QuestPDF Executive Governance Ledger
            </span>
            <h2 className="text-lg font-bold text-white" style={{ fontFamily: "var(--font-heading)" }}>
              Platform Summary PDF &amp; Cover Email
            </h2>
          </div>

          <div className="flex flex-wrap items-center gap-2.5">
            <Button
              type="button"
              variant="outline"
              onClick={() => void handleDownloadPdf()}
              isLoading={isDownloadingPdf}
              leftIcon={<Download className="size-4" aria-hidden="true" />}
            >
              Download PDF Report
            </Button>
            <Button
              type="button"
              variant="primary"
              onClick={() => void handleEmailReport()}
              isLoading={sendReportMutation.isPending}
              leftIcon={<Mail className="size-4" aria-hidden="true" />}
            >
              Email Report to Me
            </Button>
          </div>
        </div>

        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <Input
            label="Report Period Label (Optional)"
            placeholder="EXECUTIVE LEDGER · NOVEMBER 2026"
            value={reportPeriod}
            onChange={(e) => setReportPeriod(e.target.value)}
          />
          <Input
            label="Recipient Email Override (Optional)"
            type="email"
            placeholder="Defaults to your admin email"
            value={reportRecipientEmail}
            onChange={(e) => setReportRecipientEmail(e.target.value)}
          />
        </div>

        {lastReportReceipt && (
          <div className="flex flex-wrap items-center justify-between gap-2 rounded-xl border border-emerald-500/30 bg-emerald-500/10 px-4 py-3 text-xs text-emerald-200">
            <span>
              Dispatched <strong>{lastReportReceipt.fileName}</strong> to{" "}
              <span className="font-mono">{lastReportReceipt.recipientEmail}</span>
            </span>
            <span className="font-mono tabular-nums">
              {Math.max(1, Math.round(lastReportReceipt.pdfSizeBytes / 1024))} KB PDF
            </span>
          </div>
        )}
      </section>

      {/* 2. Email Template Gallery */}
      <section className="rounded-2xl border border-white/[0.085] bg-[#1b1d26] p-6 space-y-5">
        <div className="flex flex-col gap-3 border-b border-white/[0.08] pb-4 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <span className="font-mono text-[11px] uppercase tracking-wider text-[#d3fb52]">
              Editorial Campus Print System
            </span>
            <h2 className="text-lg font-bold text-white" style={{ fontFamily: "var(--font-heading)" }}>
              Email Template Gallery ({templatesQuery.data?.length ?? 7})
            </h2>
          </div>

          <div className="inline-flex flex-wrap rounded-xl border border-white/10 bg-[#14151c] p-1">
            <button
              type="button"
              onClick={() => setPreviewViewport("desktop")}
              className={`inline-flex items-center gap-1.5 rounded-lg px-3 py-1.5 text-xs font-semibold transition-colors ${
                previewViewport === "desktop"
                  ? "bg-[#d3fb52] text-[#14151c]"
                  : "text-white/75 hover:text-white"
              }`}
            >
              <Monitor className="size-3.5" aria-hidden="true" />
              <span>Desktop (600px)</span>
            </button>
            <button
              type="button"
              onClick={() => setPreviewViewport("mobile")}
              className={`inline-flex items-center gap-1.5 rounded-lg px-3 py-1.5 text-xs font-semibold transition-colors ${
                previewViewport === "mobile"
                  ? "bg-[#d3fb52] text-[#14151c]"
                  : "text-white/75 hover:text-white"
              }`}
            >
              <Smartphone className="size-3.5" aria-hidden="true" />
              <span>Mobile (375px)</span>
            </button>
          </div>
        </div>

        {templatesQuery.isError && <ErrorBanner error={templatesQuery.error} />}

        <div className="grid grid-cols-1 gap-6 lg:grid-cols-12">
          <div className="space-y-2 lg:col-span-4" role="tablist" aria-label="Email templates">
            {(templatesQuery.data ?? []).map((templateFile) => {
              const isSelected = activeTemplate === templateFile;
              return (
                <button
                  key={templateFile}
                  type="button"
                  role="tab"
                  aria-selected={isSelected}
                  onClick={() => setSelectedTemplate(templateFile)}
                  className={`flex w-full items-center justify-between rounded-xl border px-3.5 py-2.5 text-left text-xs transition-colors ${
                    isSelected
                      ? "border-[#d3fb52] bg-[#d3fb52]/10 text-white"
                      : "border-white/[0.08] bg-[#14151c] text-white/75 hover:border-white/25 hover:text-white"
                  }`}
                >
                  <span className="font-semibold">
                    {TEMPLATE_LABELS[templateFile] ?? templateFile}
                  </span>
                  <span className="font-mono text-[10px] text-[#9ca3af]">{templateFile}</span>
                </button>
              );
            })}
          </div>

          <div className="rounded-xl border border-white/10 bg-[#f3f5f2] p-4 lg:col-span-8 flex flex-col items-center overflow-x-auto">
            {templatesQuery.isLoading || previewQuery.isLoading || !activeTemplate ? (
              <div className="flex min-h-[420px] items-center justify-center gap-2 text-xs text-[#14151c]">
                <Loader2 className="size-4 animate-spin" aria-hidden="true" />
                <span>Rendering editorial template…</span>
              </div>
            ) : previewQuery.isError ? (
              <ErrorBanner error={previewQuery.error} />
            ) : (
              <iframe
                title="Email template preview"
                srcDoc={previewQuery.data ?? ""}
                sandbox="allow-same-origin"
                className={`min-h-[620px] border border-[#E2E6E0] bg-white transition-all ${
                  previewViewport === "mobile" ? "w-full max-w-[375px]" : "w-full max-w-[660px]"
                }`}
              />
            )}
          </div>
        </div>
      </section>

      {/* 3. Opt-In Digest Composer */}
      <section className="rounded-2xl border border-white/[0.085] bg-[#1b1d26] p-6 space-y-5">
        <div className="flex flex-col gap-3 border-b border-white/[0.08] pb-4 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <span className="inline-flex items-center gap-1.5 font-mono text-[11px] uppercase tracking-wider text-[#d3fb52]">
              <Sparkles className="size-3.5" aria-hidden="true" />
              Opt-In Student Communications
            </span>
            <h2 className="text-lg font-bold text-white" style={{ fontFamily: "var(--font-heading)" }}>
              Leaderboard News &amp; Portfolio Match Digest Composer
            </h2>
          </div>

          <div className="inline-flex flex-wrap rounded-xl border border-white/10 bg-[#14151c] p-1">
            <button
              type="button"
              onClick={() => setActiveDigestTab("leaderboard")}
              className={`rounded-lg px-3 py-1.5 text-xs font-semibold ${
                activeDigestTab === "leaderboard"
                  ? "bg-[#d3fb52] text-[#14151c]"
                  : "text-white/75 hover:text-white"
              }`}
            >
              Leaderboard &amp; News
            </button>
            <button
              type="button"
              onClick={() => setActiveDigestTab("project-matches")}
              className={`rounded-lg px-3 py-1.5 text-xs font-semibold ${
                activeDigestTab === "project-matches"
                  ? "bg-[#d3fb52] text-[#14151c]"
                  : "text-white/75 hover:text-white"
              }`}
            >
              Portfolio Project Matches
            </button>
          </div>
        </div>

        {activeDigestTab === "leaderboard" ? (
          <div className="space-y-4">
            <Input
              label="Issue Period Label"
              value={digestPeriodLabel}
              onChange={(e) => setDigestPeriodLabel(e.target.value)}
            />

            <div className="space-y-2.5 rounded-xl border border-white/[0.08] bg-[#14151c] p-4">
              <div className="flex items-center justify-between">
                <span className="text-xs font-bold uppercase tracking-wider text-white">
                  Campus News Items ({newsItems.length})
                </span>
                <Badge variant="outline" size="sm">
                  HTML Escaped
                </Badge>
              </div>

              {newsItems.map((item, idx) => (
                <div
                  key={`${item.headline}-${idx}`}
                  className="flex items-start justify-between gap-3 rounded-lg border border-white/[0.07] bg-[#1b1d26] p-3"
                >
                  <div className="space-y-0.5 text-xs">
                    <p className="font-mono text-[10px] uppercase tracking-wider text-[#d3fb52]">
                      {item.category}
                    </p>
                    <p className="font-semibold text-white">{item.headline}</p>
                    <p className="text-[#9ca3af]">{item.summary}</p>
                  </div>
                  <button
                    type="button"
                    aria-label={`Remove ${item.headline}`}
                    onClick={() =>
                      setNewsItems((prev) => prev.filter((_, itemIdx) => itemIdx !== idx))
                    }
                    className="rounded-lg p-1.5 text-white/60 hover:bg-white/10 hover:text-rose-300"
                  >
                    <Trash2 className="size-4" aria-hidden="true" />
                  </button>
                </div>
              ))}

              <div className="grid grid-cols-1 gap-3 border-t border-white/[0.06] pt-3 sm:grid-cols-3">
                <Input
                  label="Category"
                  placeholder="EVENTS · FCAI"
                  value={draftCategory}
                  onChange={(e) => setDraftCategory(e.target.value)}
                />
                <Input
                  label="News Headline"
                  placeholder="Demo Day Submissions Open"
                  value={draftHeadline}
                  onChange={(e) => setDraftHeadline(e.target.value)}
                />
                <Input
                  label="News Summary"
                  placeholder="Short editorial blurb"
                  value={draftSummary}
                  onChange={(e) => setDraftSummary(e.target.value)}
                />
              </div>

              <div className="flex justify-end">
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  onClick={handleAddNewsItem}
                  leftIcon={<Plus className="size-3.5" aria-hidden="true" />}
                >
                  Add News Item
                </Button>
              </div>
            </div>

            <div className="flex flex-wrap items-center justify-end gap-3">
              <Button
                type="button"
                variant="outline"
                isLoading={dispatchLeaderboardMutation.isPending}
                onClick={() => void runLeaderboardDispatch("TestToMe")}
                leftIcon={<Mail className="size-4" aria-hidden="true" />}
              >
                Send Test to My Email
              </Button>
              <Button
                type="button"
                variant="primary"
                isLoading={dispatchLeaderboardMutation.isPending}
                onClick={() => setPendingBroadcastKind("leaderboard")}
                leftIcon={<Send className="size-4" aria-hidden="true" />}
              >
                Broadcast Leaderboard Digest
              </Button>
            </div>
          </div>
        ) : (
          <div className="space-y-4">
            <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
              <Input
                label="Max Matches Per Student"
                type="number"
                min={1}
                max={10}
                value={String(maxMatchesPerStudent)}
                onChange={(e) =>
                  setMaxMatchesPerStudent(Math.min(10, Math.max(1, Number(e.target.value) || 1)))
                }
              />
              <Input
                label="Minimum Match Percentage (%)"
                type="number"
                min={0}
                max={100}
                value={String(minMatchPercentage)}
                onChange={(e) =>
                  setMinMatchPercentage(Math.min(100, Math.max(0, Number(e.target.value) || 0)))
                }
              />
            </div>

            <div className="flex flex-wrap items-center justify-end gap-3">
              <Button
                type="button"
                variant="outline"
                isLoading={dispatchMatchMutation.isPending}
                onClick={() => void runMatchDispatch("TestToMe")}
                leftIcon={<Mail className="size-4" aria-hidden="true" />}
              >
                Send Test to My Email
              </Button>
              <Button
                type="button"
                variant="primary"
                isLoading={dispatchMatchMutation.isPending}
                onClick={() => setPendingBroadcastKind("project-matches")}
                leftIcon={<Send className="size-4" aria-hidden="true" />}
              >
                Broadcast Project Matches
              </Button>
            </div>
          </div>
        )}

        {lastDigestReceipt && (
          <div className="flex flex-wrap items-center justify-between gap-2 rounded-xl border border-white/15 bg-[#14151c] px-4 py-3 text-xs text-white/85">
            <span>
              <strong>{lastDigestReceipt.digestType}</strong> (
              {lastDigestReceipt.isTestMode ? "Test Mode" : "Opted-In Broadcast"})
            </span>
            <span className="font-mono tabular-nums text-[#d3fb52]">
              Eligible: {lastDigestReceipt.eligibleRecipientsCount} · Sent:{" "}
              {lastDigestReceipt.sentCount} · Skipped: {lastDigestReceipt.skippedCount}
            </span>
          </div>
        )}
      </section>

      <ConfirmDialog
        open={pendingBroadcastKind !== null}
        onClose={() => setPendingBroadcastKind(null)}
        onConfirm={() =>
          pendingBroadcastKind === "leaderboard"
            ? runLeaderboardDispatch("Broadcast")
            : runMatchDispatch("Broadcast")
        }
        title="Confirm Opt-In Broadcast Dispatch"
        description={
          pendingBroadcastKind === "leaderboard"
            ? "This sends the Leaderboard & Campus News issue to all active students who opted in to email notifications and leaderboard digests."
            : "This sends personalized Portfolio Project Match recommendations to all active students who opted in to project match alerts."
        }
        confirmLabel="Send Opt-In Broadcast"
        variant="primary"
        isLoading={
          dispatchLeaderboardMutation.isPending || dispatchMatchMutation.isPending
        }
      />
    </AdminConsoleLayout>
  );
}
