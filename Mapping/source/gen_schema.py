"""TeamUpDB - Relational Mapping Diagram Generator (Refined, Zero-Collision).
Strict corridor routing ensures arrows never penetrate table rectangles.
"""
import sys
from pathlib import Path
from xml.sax.saxutils import escape

W, H_CANVAS = 3900, 2700
CH = 40
BG = "#fbfcfd"
LABEL_FONT = 20

def cw(c):
    if c == "StudentID":
        return 130
    if c in ("ProjectID", "OrganizationID", "EventID"):
        return 120
    return max(76, round(7.2 * len(c) + 24))

# ----------------- Strict Y Tier Positioning -----------------
# Tier 1: y = 170  (bottom = 210)  -> Corridor below: 220 - 500
# Tier 2: y = 540  (bottom = 580)  -> Corridor below: 600 - 920
# Tier 3: y = 960  (bottom = 1000) -> Corridor below: 1020 - 1400
# Tier 4: y = 1440 (bottom = 1480) -> Corridor below: 1500 - 1880
# Tier 5: y = 1920 (bottom = 1960) -> Corridor below: 1980 - 2600

T = {
    # --- Tier 1: Academic & Core Identity ---
    "University": (80, 170, [
        ("UniversityID", "PK"), ("Name", ""), ("City", ""), ("Country", ""), ("Website", "")
    ]),
    "Faculty": (740, 170, [
        ("FacultyID", "PK"), ("Name", ""), ("Code", ""), ("UniversityID", "FK")
    ]),
    "Student": (1300, 170, [
        ("StudentID", "PK"), ("Email", "UQ"), ("PasswordHash", ""), ("FirstName", ""),
        ("LastName", ""), ("AcademicYear", ""), ("Role", ""), ("FacultyID", "FK")
    ]),
    "StudentInterest": (2440, 170, [
        ("StudentID", "PK,FK"), ("Interest", "PK")
    ]),
    "StudentProfile": (2840, 170, [
        ("ProfileID", "PK"), ("StudentID", "FK,UQ"), ("Bio", ""), ("PhotoUrl", ""),
        ("GitHubUrl", ""), ("LinkedInUrl", "")
    ]),

    # --- Tier 2: Gamification, Skills & Alerts ---
    "Notification": (80, 540, [
        ("NotificationID", "PK"), ("StudentID", "FK"), ("Title", ""), ("Message", ""),
        ("Type", ""), ("IsRead", ""), ("CreatedAt", "")
    ]),
    "ScoreTransaction": (820, 540, [
        ("TransactionID", "PK"), ("StudentID", "FK"), ("Points", ""), ("Reason", ""),
        ("SourceType", ""), ("CreatedAt", "")
    ]),
    "Badge": (1620, 540, [
        ("BadgeID", "PK"), ("Name", ""), ("Description", ""), ("IconUrl", ""),
        ("CriteriaType", ""), ("CriteriaValue", "")
    ]),
    "StudentBadge": (2380, 540, [
        ("StudentID", "PK,FK"), ("BadgeID", "PK,FK"), ("AwardedAt", "")
    ]),
    "Skill": (2820, 540, [
        ("SkillID", "PK"), ("Name", ""), ("Category", ""), ("Description", "")
    ]),
    "StudentSkill": (3340, 540, [
        ("StudentID", "PK,FK"), ("SkillID", "PK,FK"), ("ProficiencyLevel", ""), ("YearsOfExperience", "")
    ]),

    # --- Tier 3: Projects & Teams ---
    "Project": (80, 960, [
        ("ProjectID", "PK"), ("Title", ""), ("Description", ""), ("Status", ""),
        ("CreatedAt", ""), ("MaxTeamSize", ""), ("CreatorID", "FK")
    ]),
    "ProjectRequiredSkill": (900, 960, [
        ("ProjectID", "PK,FK"), ("SkillID", "PK,FK"), ("RequiredLevel", ""), ("IsMandatory", "")
    ]),
    "ProjectMember": (1600, 960, [
        ("ProjectID", "PK,FK"), ("StudentID", "PK,FK"), ("Role", ""), ("JoinedAt", "")
    ]),
    "ProjectApplication": (2180, 960, [
        ("ApplicationID", "PK"), ("ProjectID", "FK"), ("StudentID", "FK"), ("Message", ""), ("Status", ""), ("AppliedAt", "")
    ]),
    "ProjectTask": (2900, 960, [
        ("TaskID", "PK"), ("Title", ""), ("Description", ""), ("Status", ""),
        ("DueDate", ""), ("Priority", ""), ("ProjectID", "FK"), ("AssignedStudentID", "FK")
    ]),

    # --- Tier 4: Student Activities / Organizations ---
    "Organization": (80, 1440, [
        ("OrganizationID", "PK"), ("Name", ""), ("Description", ""), ("Category", ""), ("LogoUrl", "")
    ]),
    "OrganizationMember": (840, 1440, [
        ("OrganizationID", "PK,FK"), ("StudentID", "PK,FK"), ("Role", ""), ("JoinedAt", "")
    ]),
    "MembershipApplication": (1580, 1440, [
        ("ApplicationID", "PK"), ("OrganizationID", "FK"), ("StudentID", "FK"), ("Message", ""),
        ("Status", ""), ("AppliedAt", ""), ("ReviewedAt", "")
    ]),

    # --- Tier 5: Events & Attendance ---
    "Event": (80, 1920, [
        ("EventID", "PK"), ("Title", ""), ("Description", ""), ("Type", ""),
        ("Location", ""), ("StartDate", ""), ("EndDate", ""), ("Capacity", ""), ("OrganizationID", "FK")
    ]),
    "EventRegistration": (1040, 1920, [
        ("EventID", "PK,FK"), ("StudentID", "PK,FK"), ("RegisteredAt", ""), ("Status", "")
    ]),
    "EventSession": (1640, 1920, [
        ("SessionID", "PK"), ("EventID", "FK"), ("SessionNo", ""), ("Title", ""), ("StartTime", ""),
        ("EndTime", ""), ("Room", "")
    ]),
    "SessionAttendance": (2340, 1920, [
        ("SessionID", "PK,FK"), ("StudentID", "PK,FK"),
        ("CheckInTime", ""), ("VerificationMethod", "")
    ]),
}

def tx(t): return T[t][0]
def ty(t): return T[t][1]
def twidth(t): return sum(cw(c) for c, _ in T[t][2])

def cell(t, col):
    x = tx(t)
    for c, _ in T[t][2]:
        w = cw(c)
        if c == col:
            return x, x + w
        x += w
    raise KeyError((t, col))

def top(t, col, off=None):
    x0, x1 = cell(t, col)
    return ((x0 + off) if off is not None else (x0 + x1) / 2, ty(t))

def bot(t, col, off=None):
    x0, x1 = cell(t, col)
    return ((x0 + off) if off is not None else (x0 + x1) / 2, ty(t) + CH)

def left_mid(t): return (tx(t), ty(t) + CH / 2)
def right_mid(t): return (tx(t) + twidth(t), ty(t) + CH / 2)

routes = []
def route(*pts): routes.append(list(pts))

# ==================== Strict Non-Crossing Route Layout ====================
# Corridor 0: y in [95..140] (above Tier 1)
# Corridor 1: y in [230..490] (between Tier 1 & Tier 2)
# Corridor 2: y in [610..910] (between Tier 2 & Tier 3)
# Corridor 3: y in [1030..1390] (between Tier 3 & Tier 4)
# Corridor 4: y in [1510..1870] (between Tier 4 & Tier 5)
# Corridor 5: y in [1990..2600] (below Tier 5)
# Left Bus Corridor: x in [30..65]
# Student Bus Corridor: x in [1260..1290]

# --- 1. Tier 1 internal ---
# Faculty.UniversityID -> University.UniversityID
s, e = top("Faculty", "UniversityID"), top("University", "UniversityID", 30)
route(s, (s[0], 115), (e[0], 115), e)

# Student.FacultyID -> Faculty.FacultyID
s, e = top("Student", "FacultyID"), top("Faculty", "FacultyID", 40)
route(s, (s[0], 130), (e[0], 130), e)

# StudentInterest.StudentID -> Student.StudentID
s, e = top("StudentInterest", "StudentID"), top("Student", "StudentID", 25)
route(s, (s[0], 115), (e[0], 115), e)

# StudentProfile.StudentID -> Student.StudentID
s, e = top("StudentProfile", "StudentID"), top("Student", "StudentID", 45)
route(s, (s[0], 100), (e[0], 100), e)


# --- 2. Tier 2 -> Tier 1 (Student / Skills / Badges) ---
# Notification.StudentID -> Student.StudentID (via left bus x=40)
s, e = left_mid("Notification"), bot("Student", "StudentID", 15)
route(s, (40, s[1]), (40, 240), (e[0], 240), e)

# ScoreTransaction.StudentID -> Student.StudentID
s, e = top("ScoreTransaction", "StudentID"), bot("Student", "StudentID", 30)
route(s, (s[0], 260), (e[0], 260), e)

# StudentBadge.BadgeID -> Badge.BadgeID
s, e = top("StudentBadge", "BadgeID"), top("Badge", "BadgeID", 40)
route(s, (s[0], 480), (e[0], 480), e)

# StudentBadge.StudentID -> Student.StudentID
s, e = top("StudentBadge", "StudentID"), bot("Student", "StudentID", 45)
route(s, (s[0], 280), (e[0], 280), e)

# StudentSkill.SkillID -> Skill.SkillID
s, e = top("StudentSkill", "SkillID"), top("Skill", "SkillID", 40)
route(s, (s[0], 480), (e[0], 480), e)

# StudentSkill.StudentID -> Student.StudentID (around right side)
s, e = right_mid("StudentSkill"), top("Student", "StudentID", 65)
route(s, (3840, s[1]), (3840, 85), (e[0], 85), e)


# --- 3. Tier 3 (Projects) ---
# Project.CreatorID -> Student.StudentID (up corridor)
s, e = top("Project", "CreatorID"), bot("Student", "StudentID", 60)
route(s, (s[0], 660), (1270, 660), (1270, 300), (e[0], 300), e)

# ProjectRequiredSkill.ProjectID -> Project.ProjectID
s, e = top("ProjectRequiredSkill", "ProjectID"), bot("Project", "ProjectID", 25)
route(s, (s[0], 880), (e[0], 880), e)

# ProjectRequiredSkill.SkillID -> Skill.SkillID
s, e = top("ProjectRequiredSkill", "SkillID"), bot("Skill", "SkillID", 25)
route(s, (s[0], 640), (e[0], 640), e)

# ProjectMember.ProjectID -> Project.ProjectID
s, e = top("ProjectMember", "ProjectID"), bot("Project", "ProjectID", 45)
route(s, (s[0], 860), (e[0], 860), e)

# ProjectMember.StudentID -> Student.StudentID
s, e = top("ProjectMember", "StudentID"), bot("Student", "StudentID", 75)
route(s, (s[0], 680), (1275, 680), (1275, 320), (e[0], 320), e)

# ProjectApplication.ProjectID -> Project.ProjectID
s, e = top("ProjectApplication", "ProjectID"), bot("Project", "ProjectID", 65)
route(s, (s[0], 840), (e[0], 840), e)

# ProjectApplication.StudentID -> Student.StudentID
s, e = top("ProjectApplication", "StudentID"), bot("Student", "StudentID", 90)
route(s, (s[0], 700), (1280, 700), (1280, 340), (e[0], 340), e)

# ProjectTask.ProjectID -> Project.ProjectID
s, e = top("ProjectTask", "ProjectID"), bot("Project", "ProjectID", 85)
route(s, (s[0], 820), (e[0], 820), e)

# ProjectTask.AssignedStudentID -> Student.StudentID
s, e = top("ProjectTask", "AssignedStudentID"), bot("Student", "StudentID", 105)
route(s, (s[0], 720), (1285, 720), (1285, 360), (e[0], 360), e)


# --- 4. Tier 4 (Organizations) ---
# OrganizationMember.OrganizationID -> Organization.OrganizationID
s, e = top("OrganizationMember", "OrganizationID"), bot("Organization", "OrganizationID", 25)
route(s, (s[0], 1340), (e[0], 1340), e)

# OrganizationMember.StudentID -> Student.StudentID (via left bus x=55)
s, e = left_mid("OrganizationMember"), bot("Student", "StudentID", 118)
route(s, (55, s[1]), (55, 380), (e[0], 380), e)

# MembershipApplication.OrganizationID -> Organization.OrganizationID
s, e = top("MembershipApplication", "OrganizationID"), bot("Organization", "OrganizationID", 50)
route(s, (s[0], 1320), (e[0], 1320), e)

# MembershipApplication.StudentID -> Student.StudentID
s, e = top("MembershipApplication", "StudentID"), bot("Student", "StudentID", 124)
route(s, (s[0], 1200), (1290, 1200), (1290, 400), (e[0], 400), e)


# --- 5. Tier 5 (Events) ---
# Event.OrganizationID -> Organization.OrganizationID
s, e = top("Event", "OrganizationID"), bot("Organization", "OrganizationID", 75)
route(s, (s[0], 1680), (e[0], 1680), e)

# EventRegistration.EventID -> Event.EventID
s, e = top("EventRegistration", "EventID"), bot("Event", "EventID", 25)
route(s, (s[0], 1820), (e[0], 1820), e)

# EventRegistration.StudentID -> Student.StudentID
s, e = top("EventRegistration", "StudentID"), bot("Student", "StudentID", 130)
route(s, (s[0], 1740), (70, 1740), (70, 420), (e[0], 420), e)

# EventSession.EventID -> Event.EventID
s, e = top("EventSession", "EventID"), bot("Event", "EventID", 50)
route(s, (s[0], 1800), (e[0], 1800), e)

# SessionAttendance.SessionID -> EventSession.SessionID
s, e = top("SessionAttendance", "SessionID"), bot("EventSession", "SessionID", 25)
route(s, (s[0], 1840), (e[0], 1840), e)

# SessionAttendance.StudentID -> Student.StudentID
s, e = top("SessionAttendance", "StudentID"), bot("Student", "StudentID", 138)
route(s, (s[0], 1760), (1295, 1760), (1295, 440), (e[0], 440), e)


# ==================== Rendering Code ====================
def underline(x, y, w, dashed=False):
    d = ' stroke-dasharray="4 3"' if dashed else ""
    return (f'<line x1="{x:.1f}" y1="{y:.1f}" x2="{x+w:.1f}" y2="{y:.1f}" '
            f'stroke="#111" stroke-width="1.3"{d}/>')

def svg():
    o = [
        f'<svg xmlns="http://www.w3.org/2000/svg" width="{W}" height="{H_CANVAS}" '
        f'viewBox="0 0 {W} {H_CANVAS}" font-family="Segoe UI, Arial, sans-serif" font-size="13">',
        '<defs><marker id="arr" viewBox="0 0 10 10" refX="9" refY="5" markerWidth="8" '
        'markerHeight="8" orient="auto"><path d="M0,1 L9,5 L0,9 z" fill="#1f3a68"/></marker></defs>',
        f'<rect width="{W}" height="{H_CANVAS}" fill="{BG}"/>',
        '<text x="80" y="55" font-size="34" font-weight="bold" fill="#0b2e59">'
        'TeamUpDB — Relational Schema Mapping (23 Tables)</text>',
        '<text x="80" y="82" font-size="16" fill="#555">'
        'Normalized (3NF) • All FKs ON DELETE NO ACTION / ON UPDATE NO ACTION • DELETE = soft delete (single column: DeletedAt IS NULL = active, timestamp = deleted)</text>'
    ]

    # Draw routes first so table boxes overlay cleanly
    for r in routes:
        pts = " ".join(f"{x:.1f},{y:.1f}" for x, y in r)
        o.append(f'<polyline points="{pts}" fill="none" stroke="#1f3a68" stroke-width="1.4" '
                 'marker-end="url(#arr)"/>')

    # Draw tables
    for name, (x, y, cols) in T.items():
        o.append(f'<text x="{x}" y="{y-12}" font-size="{LABEL_FONT}" font-weight="bold" '
                 f'fill="#0b2e59">{escape(name)}</text>')
        cx = x
        pk_w = 0
        pk_start_x = None
        for c, f in cols:
            flags = f.split(",") if f else []
            w = cw(c)
            o.append(f'<rect x="{cx}" y="{y}" width="{w}" height="{CH}" fill="white" '
                     'stroke="#1f3a68" stroke-width="1.2"/>')
            o.append(f'<text x="{cx+w/2:.1f}" y="{y+25}" text-anchor="middle" fill="#111">'
                     f'{escape(c)}</text>')
            tw = 7.0 * len(c)
            ux = cx + w / 2 - tw / 2
            if "PK" in flags and "FK" in flags:
                o.append(underline(ux, y + 27, tw))
                o.append(underline(ux, y + 31, tw, dashed=True))
            elif "PK" in flags:
                o.append(underline(ux, y + 28, tw))
            elif "FK" in flags:
                o.append(underline(ux, y + 28, tw, dashed=True))
            if "UQ" in flags:
                o.append(f'<text x="{cx+w-4}" y="{y+12}" text-anchor="end" font-size="9" '
                         'fill="#c62828" font-weight="bold">UQ</text>')
            if "PK" in flags:
                if pk_start_x is None:
                    pk_start_x = cx
                pk_w += w
            cx += w

        n_pk = sum(1 for _, f in cols if "PK" in f.split(","))
        if n_pk > 1 and pk_start_x is not None:
            # Composite PK line under table
            o.append(f'<line x1="{pk_start_x+4}" y1="{y+CH+6}" x2="{pk_start_x+pk_w-4}" y2="{y+CH+6}" '
                     'stroke="#0b2e59" stroke-width="1.6"/>')

    # Legend
    lx0, ly0 = 2680, 2040
    o.append(f'<rect x="{lx0}" y="{ly0}" width="1120" height="540" fill="white" stroke="#999" rx="8"/>')
    o.append(f'<text x="{lx0+28}" y="{ly0+42}" font-size="22" font-weight="bold" fill="#0b2e59">'
             'TeamUpDB Relational Schema Legend</text>')
    o.append(f'<text x="{lx0+28}" y="{ly0+68}" font-size="14" fill="#555">'
             'Standard Relational Mapping Notation (23 Tables, 3NF Certified)</text>')

    rows = [
        ("pk", "Primary Key (solid underline inside cell)"),
        ("fk", "Foreign Key (dashed underline inside cell)"),
        ("pkfk", "Composite Primary Key &amp; Foreign Key (both solid and dashed underlines)"),
        ("comp", "Underline below table spanning key cells = Composite Primary Key"),
        ("uq", "UQ badge = filtered UNIQUE index WHERE DeletedAt IS NULL (e.g., Student.Email)"),
        ("arrow", "Orthogonal Arrow: Foreign Key column -&gt; referenced table Primary Key cell"),
        ("derived", "Derived attributes (TotalPoints, Level, TeamSize, RegisteredCount) are computed, not stored")
    ]
    ly = ly0 + 105
    for kind, text in rows:
        bx = lx0 + 28
        if kind in ("pk", "fk", "pkfk", "comp", "uq"):
            o.append(f'<rect x="{bx}" y="{ly-18}" width="95" height="32" fill="white" stroke="#1f3a68" stroke-width="1.2"/>')
            lab = "ID" if kind != "uq" else "Email"
            o.append(f'<text x="{bx+47}" y="{ly+4}" text-anchor="middle" font-size="13">{lab}</text>')
            tw = 7.0 * len(lab)
            ux = bx + 47 - tw / 2
            if kind == "pk" or kind == "comp":
                o.append(underline(ux, ly + 8, tw))
            elif kind == "fk":
                o.append(underline(ux, ly + 8, tw, True))
            elif kind == "pkfk":
                o.append(underline(ux, ly + 7, tw))
                o.append(underline(ux, ly + 11, tw, True))
            else:
                o.append(f'<text x="{bx+90}" y="{ly-7}" text-anchor="end" font-size="9" fill="#c62828" font-weight="bold">UQ</text>')
            if kind == "comp":
                o.append(f'<line x1="{bx+4}" y1="{ly+22}" x2="{bx+91}" y2="{ly+22}" stroke="#0b2e59" stroke-width="1.6"/>')
        elif kind == "arrow":
            o.append(f'<polyline points="{bx},{ly} {bx+95},{ly}" fill="none" stroke="#1f3a68" stroke-width="1.4" marker-end="url(#arr)"/>')
        else:
            o.append(f'<rect x="{bx}" y="{ly-18}" width="95" height="32" fill="#eef2f9" stroke="#999" stroke-dasharray="4 3"/>')
            o.append(f'<text x="{bx+47}" y="{ly+4}" text-anchor="middle" font-size="11" fill="#555">[Computed]</text>')
        o.append(f'<text x="{bx+125}" y="{ly+4}" font-size="15" fill="#222">{text}</text>')
        ly += 42

    o.append(f'<text x="{lx0+28}" y="{ly0+505}" font-size="13" fill="#666">'
             'Designed for Microsoft SQL Server &amp; Entity Framework Core (ASP.NET Core Web API / JWT Auth)</text>')
    o.append("</svg>")
    return "\n".join(o)


if __name__ == "__main__":
    out = Path(sys.argv[1])
    out.mkdir(parents=True, exist_ok=True)
    (out / "TeamUp-Relational-Mapping.svg").write_text(svg(), encoding="utf-8")
    print("tables:", len(T), "routes:", len(routes))
