/**
 * @date 2026-11-24
 * @purpose Constants, filtered marquee routes, partner campus fallback re-export, and spotlight helpers for TeamUp landing page.
 */
import type { MouseEvent } from "react";
import { FALLBACK_UNIVERSITIES } from "@/features/catalog/useUniversityCatalogFallback";

export const PLATFORM_SERVICES = [
  {
    id: "team-formation",
    to: "/projects",
    title: "Team Formation & Projects",
    subtitleMetric: "Up to +250 pts / project",
    tagsLine: "Create Projects · Publish Skills · Find Teammates · Task Board",
  },
  {
    id: "student-profiles",
    to: "/students",
    title: "Student Profiles & Scoring",
    subtitleMetric: "Verified Skill Portfolio",
    tagsLine: "University · Independent Learners · Graduates · Tech Peers",
  },
  {
    id: "student-activities",
    to: "/organizations",
    title: "Student Activities Hub",
    subtitleMetric: "Branches & Open Collectives",
    tagsLine: "Chapters · 4-Tier Hierarchy · Recruitment Kanban · Active Roles",
  },
  {
    id: "events-workshops",
    to: "/events",
    title: "Events & Workshops",
    subtitleMetric: "Up to +100 pts / workshop",
    tagsLine: "On-Campus · Hybrid · Online · Digital Pass & .ics Export",
  },
  {
    id: "attendance-verification",
    to: "/check-in",
    title: "Attendance Verification",
    subtitleMetric: "Instant Session Check-In",
    tagsLine: "QR Camera Scan · Stage Projector · RFID Verification · CSV",
  },
  {
    id: "gamification-engine",
    to: "/gamification",
    title: "Gamification & Leaderboards",
    subtitleMetric: "Points, Levels & Badges",
    tagsLine: "Score Ledger · Level Formula · Unlock Badges · Smart Inbox",
  },
] as const;

export const MARQUEE_TILES = [
  { id: "campus-guilds", to: "/organizations" },
  { id: "ieee-cs", to: "/organizations?search=IEEE" },
  { id: "robotics-ai", to: "/organizations?search=AI" },
  { id: "eng-cs", to: "/organizations?search=Engineering" },
  { id: "enactus", to: "/organizations?search=Enactus" },
  { id: "acm", to: "/organizations?search=STP" },
  { id: "gdg-cloud", to: "/organizations?search=GDG" },
] as const;

export const ACM_ROTATIONS = Array.from({ length: 16 }, (_, idx) => idx * 22.5);

export const PARTNER_CAMPUSES_FALLBACK = FALLBACK_UNIVERSITIES;

export function updateSpotlightCoords(event: MouseEvent<HTMLElement>) {
  const rect = event.currentTarget.getBoundingClientRect();
  event.currentTarget.style.setProperty("--mouse-x", `${event.clientX - rect.left}px`);
  event.currentTarget.style.setProperty("--mouse-y", `${event.clientY - rect.top}px`);
}
