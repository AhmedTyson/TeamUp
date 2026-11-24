/**
 * @date 2026-11-24
 * @purpose Unified Ctrl+K / Cmd+K global command palette searching student activities, events, projects, and quick routes.
 */
import {
  Building2,
  CalendarDays,
  Compass,
  FolderKanban,
  Languages,
  QrCode,
  Search,
  Trophy,
  UserCheck,
  X,
} from "lucide-react";
import { useEffect, useMemo, useRef, useState } from "react";
import { createPortal } from "react-dom";
import { useNavigate } from "react-router-dom";
import { eventsApi, organizationsApi, projectsApi } from "@/api";
import { useAuth } from "@/features/auth/AuthContext";
import { cn } from "@/lib/cn";
import { useI18n } from "@/lib/i18n";

export const OPEN_COMMAND_PALETTE_EVENT = "teamup:open-command-palette";

export function dispatchOpenCommandPalette(): void {
  window.dispatchEvent(new CustomEvent(OPEN_COMMAND_PALETTE_EVENT));
}

interface PaletteItem {
  id: string;
  section: string;
  label: string;
  subtitle?: string;
  to?: string;
  onSelect?: () => void;
  icon: "org" | "event" | "project" | "route" | "lang";
}

export function CommandPalette() {
  const navigate = useNavigate();
  const { isAuthenticated } = useAuth();
  const { t, toggleLocale, locale } = useI18n();

  const [open, setOpen] = useState(false);
  const [query, setQuery] = useState("");
  const [activeIndex, setActiveIndex] = useState(0);
  const [remoteItems, setRemoteItems] = useState<PaletteItem[]>([]);
  const [loadingRemote, setLoadingRemote] = useState(false);
  const inputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    const handleKeyDown = (event: KeyboardEvent) => {
      if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === "k") {
        event.preventDefault();
        setOpen((prev) => !prev);
      }
    };
    const handleCustomOpen = () => setOpen(true);

    window.addEventListener("keydown", handleKeyDown);
    window.addEventListener(OPEN_COMMAND_PALETTE_EVENT, handleCustomOpen);
    return () => {
      window.removeEventListener("keydown", handleKeyDown);
      window.removeEventListener(OPEN_COMMAND_PALETTE_EVENT, handleCustomOpen);
    };
  }, []);

  useEffect(() => {
    if (!open) {
      setQuery("");
      setActiveIndex(0);
      return;
    }
    const timer = window.setTimeout(() => {
      inputRef.current?.focus();
    }, 30);
    return () => window.clearTimeout(timer);
  }, [open]);

  useEffect(() => {
    if (!open) {
      return;
    }
    const controller = new AbortController();
    const timer = window.setTimeout(async () => {
      setLoadingRemote(true);
      try {
        const q = query.trim() || undefined;
        const [orgs, events, projects] = await Promise.allSettled([
          isAuthenticated
            ? organizationsApi.list({ search: q, page: 1, pageSize: 5 }, controller.signal)
            : organizationsApi.publicList({ search: q, page: 1, pageSize: 5 }, controller.signal),
          isAuthenticated
            ? eventsApi.list({ search: q, page: 1, pageSize: 5 }, controller.signal)
            : eventsApi.publicList({ search: q, page: 1, pageSize: 5 }, controller.signal),
          isAuthenticated
            ? projectsApi.list({ keyword: q, page: 1, pageSize: 4 }, controller.signal)
            : Promise.resolve({ items: [] }),
        ]);

        if (controller.signal.aborted) {
          return;
        }

        const next: PaletteItem[] = [];
        if (orgs.status === "fulfilled") {
          for (const org of orgs.value.items) {
            next.push({
              id: `org-${org.organizationId}`,
              section: t("cmd.organizations"),
              label: org.name,
              subtitle: org.universityName ?? org.city ?? org.category,
              to: `/organizations/${org.organizationId}`,
              icon: "org",
            });
          }
        }
        if (events.status === "fulfilled") {
          for (const ev of events.value.items) {
            next.push({
              id: `event-${ev.eventId}`,
              section: t("cmd.events"),
              label: ev.title,
              subtitle: `${ev.organizationName} · ${ev.location}`,
              to: `/events/${ev.eventId}`,
              icon: "event",
            });
          }
        }
        if (projects.status === "fulfilled") {
          for (const proj of projects.value.items) {
            next.push({
              id: `project-${proj.projectId}`,
              section: t("cmd.projects"),
              label: proj.title,
              subtitle: proj.creatorName,
              to: `/projects/${proj.projectId}`,
              icon: "project",
            });
          }
        }
        setRemoteItems(next);
      } finally {
        if (!controller.signal.aborted) {
          setLoadingRemote(false);
        }
      }
    }, 180);

    return () => {
      controller.abort();
      window.clearTimeout(timer);
    };
  }, [open, query, isAuthenticated, t]);

  const quickActions = useMemo<PaletteItem[]>(() => {
    const base: PaletteItem[] = [
      {
        id: "route-orgs",
        section: t("cmd.quickActions"),
        label: t("nav.studentActivities"),
        subtitle: "/organizations",
        to: "/organizations",
        icon: "org",
      },
      {
        id: "route-events",
        section: t("cmd.quickActions"),
        label: t("nav.eventsWorkshops"),
        subtitle: "/events",
        to: "/events",
        icon: "event",
      },
      {
        id: "route-checkin",
        section: t("cmd.quickActions"),
        label: t("events.checkIn"),
        subtitle: "/check-in",
        to: "/check-in",
        icon: "route",
      },
      {
        id: "route-projects",
        section: t("cmd.quickActions"),
        label: t("nav.teamFormation"),
        subtitle: "/projects",
        to: "/projects",
        icon: "project",
      },
      {
        id: "route-leaderboard",
        section: t("cmd.quickActions"),
        label: "Leaderboard",
        subtitle: "/leaderboard",
        to: "/leaderboard",
        icon: "route",
      },
      {
        id: "route-profile",
        section: t("cmd.quickActions"),
        label: "Profile Studio",
        subtitle: "/profile/me",
        to: "/profile/me",
        icon: "route",
      },
      {
        id: "action-lang",
        section: t("cmd.quickActions"),
        label: locale === "en" ? "Switch Language to Arabic (العربية RTL)" : "Switch Language to English (LTR)",
        subtitle: locale === "en" ? "AR · RTL" : "EN · LTR",
        onSelect: () => toggleLocale(),
        icon: "lang",
      },
    ];
    const trimmed = query.trim().toLowerCase();
    if (!trimmed) {
      return base;
    }
    return base.filter(
      (item) =>
        item.label.toLowerCase().includes(trimmed) ||
        (item.subtitle?.toLowerCase().includes(trimmed) ?? false),
    );
  }, [query, t, locale, toggleLocale]);

  const items = useMemo(
    () => [...remoteItems, ...quickActions],
    [remoteItems, quickActions],
  );

  const selectItem = (item: PaletteItem | undefined) => {
    if (!item) {
      return;
    }
    setOpen(false);
    if (item.onSelect) {
      item.onSelect();
      return;
    }
    if (item.to) {
      void navigate(item.to);
    }
  };

  if (!open || typeof document === "undefined") {
    return null;
  }

  return createPortal(
    <div
      role="dialog"
      aria-modal="true"
      aria-label="Global command palette"
      className="fixed inset-0 z-[90] flex items-start justify-center bg-black/70 px-4 pt-20 backdrop-blur-sm sm:pt-28"
      onClick={() => setOpen(false)}
    >
      <div
        className="w-full max-w-2xl overflow-hidden rounded-2xl border border-white/15 bg-[#1b1d26] shadow-2xl shadow-black/70"
        onClick={(e) => e.stopPropagation()}
        onKeyDown={(e) => {
          if (e.key === "Escape") {
            e.preventDefault();
            setOpen(false);
          } else if (e.key === "ArrowDown") {
            e.preventDefault();
            setActiveIndex((prev) => (items.length ? (prev + 1) % items.length : 0));
          } else if (e.key === "ArrowUp") {
            e.preventDefault();
            setActiveIndex((prev) =>
              items.length ? (prev - 1 + items.length) % items.length : 0,
            );
          } else if (e.key === "Enter") {
            e.preventDefault();
            selectItem(items[activeIndex]);
          }
        }}
      >
        <div className="flex items-center gap-3 border-b border-white/10 px-4 py-3.5">
          <Search className="size-4 shrink-0 text-[#d3fb52]" aria-hidden="true" />
          <input
            ref={inputRef}
            type="text"
            value={query}
            onChange={(e) => {
              setQuery(e.target.value);
              setActiveIndex(0);
            }}
            placeholder={t("cmd.placeholder")}
            aria-label={t("cmd.placeholder")}
            className="w-full bg-transparent text-sm text-white placeholder:text-white/45 focus:outline-none"
          />
          {loadingRemote && (
            <span className="shrink-0 font-mono text-[10px] text-[#d3fb52]">SYNC</span>
          )}
          <button
            type="button"
            onClick={() => setOpen(false)}
            aria-label="Close command palette"
            className="inline-flex size-7 cursor-pointer items-center justify-center rounded-lg text-white/60 hover:bg-white/10 hover:text-white"
          >
            <X className="size-4" aria-hidden="true" />
          </button>
        </div>

        <div className="max-h-[60vh] overflow-y-auto p-2">
          {items.length === 0 ? (
            <p className="px-4 py-8 text-center text-sm text-white/55">{t("cmd.noResults")}</p>
          ) : (
            items.map((item, idx) => {
              const Icon =
                item.icon === "org"
                  ? Building2
                  : item.icon === "event"
                    ? CalendarDays
                    : item.icon === "project"
                      ? FolderKanban
                      : item.icon === "lang"
                        ? Languages
                        : item.to === "/check-in"
                          ? QrCode
                          : item.to === "/leaderboard"
                            ? Trophy
                            : item.to === "/profile/me"
                              ? UserCheck
                              : Compass;

              return (
                <button
                  key={item.id}
                  type="button"
                  onMouseEnter={() => setActiveIndex(idx)}
                  onClick={() => selectItem(item)}
                  className={cn(
                    "flex w-full cursor-pointer items-center justify-between gap-3 rounded-xl px-3.5 py-2.5 text-start transition-colors",
                    idx === activeIndex
                      ? "bg-[#d3fb52] text-[#14151c]"
                      : "text-white hover:bg-white/[0.06]",
                  )}
                >
                  <div className="flex min-w-0 items-center gap-3">
                    <Icon
                      className={cn(
                        "size-4 shrink-0",
                        idx === activeIndex ? "text-[#14151c]" : "text-[#d3fb52]",
                      )}
                      aria-hidden="true"
                    />
                    <div className="min-w-0">
                      <p className="truncate text-sm font-semibold">{item.label}</p>
                      {item.subtitle && (
                        <p
                          className={cn(
                            "truncate text-xs",
                            idx === activeIndex ? "text-[#14151c]/75" : "text-white/55",
                          )}
                        >
                          {item.subtitle}
                        </p>
                      )}
                    </div>
                  </div>
                  <span
                    className={cn(
                      "shrink-0 rounded-full px-2 py-0.5 text-[10px] font-semibold",
                      idx === activeIndex
                        ? "bg-[#14151c]/15 text-[#14151c]"
                        : "bg-white/[0.06] text-white/60",
                    )}
                  >
                    {item.section}
                  </span>
                </button>
              );
            })
          )}
        </div>

        <div className="flex items-center justify-between border-t border-white/10 bg-[#14151c]/70 px-4 py-2 text-[11px] text-white/50">
          <span>↑↓ Navigate · ↵ Open · Esc Close</span>
          <span className="font-mono">Ctrl + K</span>
        </div>
      </div>
    </div>,
    document.body,
  );
}
