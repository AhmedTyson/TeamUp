/**
 * @date 2026-11-24
 * @purpose Static English UI copy constants preserved (no runtime language toggle; English-only platform).
 */
export type Locale = "en";

export const EN_MESSAGES = {
  "orgs.eyebrow": "Activities & Open Communities",
  "orgs.title": "Find your people.",
  "orgs.subtitle":
    "Explore university student branches, cross-campus initiatives, and open independent tech communities across Egypt.",
  "orgs.create": "Create organization",
  "orgs.searchPlaceholder": "Search communities, chapters, cities, or focus…",
  "orgs.scope.all": "All scopes",
  "orgs.scope.UniversityBranch": "University Branches",
  "orgs.scope.CrossUniversity": "Cross-University",
  "orgs.scope.Independent": "Open / Independent",
  "orgs.scope.recruiting": "Recruiting Now",
} as const;

export type MessageKey = keyof typeof EN_MESSAGES;
