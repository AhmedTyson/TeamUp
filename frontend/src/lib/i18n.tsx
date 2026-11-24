/**
 * @date 2026-11-24
 * @purpose Bilingual Arabic (RTL) / English (LTR) React i18n context syncing <html dir & lang> and persisting choice across sessions.
 */
import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from "react";

export type Locale = "en" | "ar";

const STORAGE_KEY = "teamup.locale";

const EN_MESSAGES = {
  "nav.teamFormation": "Team Formation",
  "nav.profilesScoring": "Profiles & Scoring",
  "nav.studentActivities": "Student Activities",
  "nav.eventsWorkshops": "Events & Workshops",
  "nav.gamification": "Gamification",
  "nav.signIn": "Sign In",
  "nav.getStarted": "Get Started",
  "nav.signOut": "Sign out",
  "nav.searchHint": "Quick jump…",
  "cmd.placeholder": "Search activities, events, projects, or jump to a page…",
  "cmd.noResults": "No matching communities, events, or shortcuts found.",
  "cmd.quickActions": "Quick Navigation",
  "cmd.organizations": "Student Activities & Communities",
  "cmd.events": "Events & Workshops",
  "cmd.projects": "Projects & Squads",
  "health.online": "API Live",
  "health.checking": "Checking API…",
  "health.degraded": "API Degraded",
  "health.offline": "API Offline",
  "orgs.eyebrow": "Activities & Open Communities",
  "orgs.title": "Find your people.",
  "orgs.subtitle": "Explore university student branches, cross-campus initiatives, and open independent tech communities across Egypt.",
  "orgs.create": "Create organization",
  "orgs.searchPlaceholder": "Search communities, chapters, cities, or focus…",
  "orgs.scope.all": "All scopes",
  "orgs.scope.UniversityBranch": "University Branches",
  "orgs.scope.FacultyChapter": "Faculty Chapters",
  "orgs.scope.CrossUniversity": "Cross-University",
  "orgs.scope.Independent": "Open / Independent",
  "orgs.scope.NationalNetwork": "National Network",
  "orgs.scope.recruiting": "Recruiting Now",
  "orgs.tab.overview": "Overview",
  "orgs.tab.units": "Chapters & Committees",
  "orgs.tab.hierarchy": "Org Hierarchy",
  "orgs.tab.members": "Members & Crew",
  "orgs.tab.recruitment": "Recruitment Pipeline",
  "orgs.tab.participants": "Participants",
  "orgs.tab.events": "Hosted Events",
  "events.eyebrow": "Live Events & Bootcamps",
  "events.title": "Show up live.",
  "events.subtitle": "Reserve workshop seats, download calendar invites & digital passes, and verify your attendance via QR.",
  "events.create": "Create event",
  "events.checkIn": "Student check-in",
  "events.downloadIcs": "Add to Calendar (.ics)",
  "events.digitalPass": "TeamUp Digital Pass",
  "attendance.stageMode": "Launch Stage Mode",
  "attendance.exportCsv": "Export CSV",
  "auth.signInPrompt": "Sign in to apply or register across any device.",
} as const;

export type MessageKey = keyof typeof EN_MESSAGES;

const AR_MESSAGES: Record<MessageKey, string> = {
  "nav.teamFormation": "تكوين الفرق",
  "nav.profilesScoring": "الملفات والنقاط",
  "nav.studentActivities": "الأنشطة الطلابية",
  "nav.eventsWorkshops": "الفعاليات والورش",
  "nav.gamification": "التحفيز والشارات",
  "nav.signIn": "تسجيل الدخول",
  "nav.getStarted": "ابدأ الآن",
  "nav.signOut": "تسجيل الخروج",
  "nav.searchHint": "بحث سريع…",
  "cmd.placeholder": "ابحث عن نشاط طلابي، فعالية، مشروع أو انتقل لصفحة…",
  "cmd.noResults": "لم يتم العثور على نتائج مطابقة.",
  "cmd.quickActions": "انتقال سريع",
  "cmd.organizations": "الأنشطة والمجتمعات الطلابية",
  "cmd.events": "الفعاليات والورش التدريبية",
  "cmd.projects": "المشاريع والفرق",
  "health.online": "الخادم متصل",
  "health.checking": "جاري الفحص…",
  "health.degraded": "اتصال محدود",
  "health.offline": "الخادم غير متصل",
  "orgs.eyebrow": "الأنشطة الطلابية والمجتمعات التقنية",
  "orgs.title": "اكتشف مجتمعك.",
  "orgs.subtitle": "استكشف الفروع الطلابية بالجامعات المصرية مثل IEEE وEnactus والمجتمعات المستقلة المفتوحة مثل GDG.",
  "orgs.create": "إنشاء نشاط / مجتمع",
  "orgs.searchPlaceholder": "ابحث بالاسم، المدينة، الجامعة أو التخصص…",
  "orgs.scope.all": "الكل",
  "orgs.scope.UniversityBranch": "فروع الجامعات",
  "orgs.scope.FacultyChapter": "جمعيات الكليات",
  "orgs.scope.CrossUniversity": "بين الجامعات",
  "orgs.scope.Independent": "مجتمعات مستقلة",
  "orgs.scope.NationalNetwork": "شبكة قومية",
  "orgs.scope.recruiting": "التقديم مفتوح الآن",
  "orgs.tab.overview": "نظرة عامة",
  "orgs.tab.units": "التشابترز واللجان",
  "orgs.tab.hierarchy": "الهيكل التنظيمي",
  "orgs.tab.members": "الأعضاء والفريق",
  "orgs.tab.recruitment": "مسار القبول (HR)",
  "orgs.tab.participants": "المشاركون",
  "orgs.tab.events": "الفعاليات المستضافة",
  "events.eyebrow": "الفعاليات المباشرة والمعسكرات",
  "events.title": "احجز مقعدك.",
  "events.subtitle": "سجّل في الورش والهاكاثون، احصل على بطاقة الدخول الرقمية، وسجّل حضورك بالكاميرا.",
  "events.create": "إنشاء فعالية",
  "events.checkIn": "تسجيل حضور الطالب",
  "events.downloadIcs": "إضافة للتقويم (.ics)",
  "events.digitalPass": "تذكرة حضور TeamUp الرقمية",
  "attendance.stageMode": "عرض وضع القاعة (Full Stage)",
  "attendance.exportCsv": "تصدير CSV",
  "auth.signInPrompt": "سجّل الدخول للتقديم أو الحجز ومزامنة حالتك من أي جهاز.",
};

interface I18nContextValue {
  locale: Locale;
  dir: "ltr" | "rtl";
  setLocale: (next: Locale) => void;
  toggleLocale: () => void;
  t: (key: MessageKey) => string;
}

const I18nContext = createContext<I18nContextValue | undefined>(undefined);

function readInitialLocale(): Locale {
  if (typeof window === "undefined") {
    return "en";
  }
  try {
    const stored = window.localStorage.getItem(STORAGE_KEY);
    return stored === "ar" ? "ar" : "en";
  } catch {
    return "en";
  }
}

export function I18nProvider({ children }: { children: ReactNode }) {
  const [locale, setLocaleState] = useState<Locale>(readInitialLocale);
  const dir: "ltr" | "rtl" = locale === "ar" ? "rtl" : "ltr";

  useEffect(() => {
    document.documentElement.lang = locale;
    document.documentElement.dir = dir;
    try {
      window.localStorage.setItem(STORAGE_KEY, locale);
    } catch {
      // Ignore storage failures
    }
  }, [locale, dir]);

  const setLocale = useCallback((next: Locale) => {
    setLocaleState(next);
  }, []);

  const toggleLocale = useCallback(() => {
    setLocaleState((prev) => (prev === "en" ? "ar" : "en"));
  }, []);

  const t = useCallback(
    (key: MessageKey): string => (locale === "ar" ? AR_MESSAGES[key] : EN_MESSAGES[key]) ?? EN_MESSAGES[key],
    [locale],
  );

  const value = useMemo(
    () => ({ locale, dir, setLocale, toggleLocale, t }),
    [locale, dir, setLocale, toggleLocale, t],
  );

  return <I18nContext.Provider value={value}>{children}</I18nContext.Provider>;
}

export function useI18n(): I18nContextValue {
  const ctx = useContext(I18nContext);
  if (!ctx) {
    return {
      locale: "en",
      dir: "ltr",
      setLocale: () => {},
      toggleLocale: () => {},
      t: (key) => EN_MESSAGES[key],
    };
  }
  return ctx;
}
