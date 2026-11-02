"""TeamUpDB - Chen ERD generator. Usage: python gen_erd.py <out_dir>
Outputs SVG (render to PNG) and .drawio (editable)."""
import math
import sys
from pathlib import Path
from xml.sax.saxutils import escape

W, H = 6600, 5700
nodes, edges = {}, []


def E(i, label, x, y, weak=False, w=210):
    nodes[i] = dict(id=i, t="entity", label=label, x=x, y=y, w=w, h=95, weak=weak)


def R(i, label, x, y, ident=False):
    nodes[i] = dict(id=i, t="rel", label=label, x=x, y=y, w=230, h=120, ident=ident)


_n = [0]


def attr(owner, label, kind, x, y):
    _n[0] += 1
    i = f"a{_n[0]}"
    w = max(120, 9.6 * len(label) + 40)
    nodes[i] = dict(id=i, t="attr", kind=kind, label=label, x=x, y=y, w=w, h=62)
    edges.append(dict(a=owner, b=i, double=False, label=None, ent=None, off=0))
    return i


def pos(owner, ang, r):
    n = nodes[owner]
    a = math.radians(ang)
    return n["x"] + r * math.cos(a), n["y"] + r * math.sin(a)


def fan(owner, items, a0, a1, rs):
    """items: (label, kind[, children]). Angles in screen degrees (0=E, 90=S, 270=N)."""
    k = len(items)
    for j, it in enumerate(items):
        ang = (a0 + a1) / 2 if k == 1 else a0 + (a1 - a0) * j / (k - 1)
        r = rs[j % len(rs)]
        x, y = pos(owner, ang, r)
        pid = attr(owner, it[0], it[1] if it[1] != "comp" else "composite", x, y)
        if len(it) > 2:
            for c, ch in enumerate(it[2]):
                d = -1 if c == 0 else 1
                cx = x + 150 * math.cos(math.radians(ang + d * 38))
                cy = y + 150 * math.sin(math.radians(ang + d * 38))
                attr(pid, ch, "simple", cx, cy)


def L(a, b, card=None, ent="a", total=False, off=20):
    edges.append(dict(a=a, b=b, double=total, label=card, ent=ent, off=off))


# ---------------- layout helpers ----------------
def RP(i, label, o, ang, Rr, ident=False):
    x, y = pos(o, ang, Rr)
    R(i, label, x, y, ident)


def A(o, label, kind, ang, r, children=()):
    x, y = pos(o, ang, r)
    pid = attr(o, label, kind, x, y)
    for cl, ca, cr in children:
        cx, cy = pos(o, ca, cr)
        attr(pid, cl, "simple", cx, cy)


def col(o, items, dx, step, side=-1):
    n = nodes[o]
    y0 = n["y"] - step * (len(items) - 1) / 2
    for j, (lab, kind) in enumerate(items):
        attr(o, lab, kind, n["x"] + side * dx, y0 + j * step)


# ---------------- entities ----------------
E("St", "Student", 3200, 2400, w=1000)
E("Un", "University", 700, 900)
E("Fa", "Faculty", 2000, 900)
E("Pf", "StudentProfile", 1250, 1950)
E("No", "Notification", 1250, 2550)
E("Sc", "ScoreTransaction", 1250, 3100)
E("Bd", "Badge", 1250, 3650)
E("Og", "Organization", 1900, 4500)
E("Ev", "Event", 3300, 4750)
E("Se", "EventSession", 4800, 4650, weak=True)
E("Sk", "Skill", 5500, 900)
E("Pr", "Project", 5700, 2400, w=320)
E("Tk", "Task", 5700, 3750)

# ---------------- relationships ----------------
R("Contains", "Contains", 1350, 900)
R("Requires", "Requires", 5600, 1650)
R("HasTask", "HasTask", 5700, 3050)
R("Organizes", "Organizes", 2600, 4650)
R("HasSession", "HasSession", 4050, 4700, ident=True)
RP("HasSkill", "HasSkill", "St", 332, 1200)
RP("Creates", "Creates", "St", 350, 1150)
RP("MemberOf", "MemberOf", "St", 8, 1150)
RP("AppliesTo", "AppliesTo", "St", 26, 1200)
RP("AssignedTo", "AssignedTo", "St", 44, 1300)
RP("Attends", "Attends", "St", 62, 1700)
RP("Registers", "Registers", "St", 80, 1500)
RP("OrgMember", "OrgMember", "St", 98, 1500)
RP("JoinRequest", "JoinRequest", "St", 116, 1300)
RP("EarnsBadge", "EarnsBadge", "St", 134, 1250)
RP("Earns", "Earns", "St", 152, 1200)
RP("Receives", "Receives", "St", 170, 1200)
RP("HasProfile", "HasProfile", "St", 188, 1200)
RP("StudiesAt", "StudiesAt", "St", 206, 1200)

# ---------------- links with cardinality ----------------
L("Un", "Contains", "1", "a"); L("Contains", "Fa", "N", "b", total=True)
L("Fa", "StudiesAt", "1", "a"); L("StudiesAt", "St", "N", "b", total=True)
L("St", "HasProfile", "1", "a"); L("HasProfile", "Pf", "1", "b", total=True)
L("St", "HasSkill", "M", "a"); L("HasSkill", "Sk", "N", "b")
L("St", "Creates", "1", "a"); L("Creates", "Pr", "N", "b", total=True)
L("St", "MemberOf", "M", "a"); L("MemberOf", "Pr", "N", "b")
L("St", "AppliesTo", "M", "a"); L("AppliesTo", "Pr", "N", "b")
L("Pr", "Requires", "M", "a"); L("Requires", "Sk", "N", "b")
L("Pr", "HasTask", "1", "a"); L("HasTask", "Tk", "N", "b", total=True)
L("St", "AssignedTo", "1", "a"); L("AssignedTo", "Tk", "N", "b")
L("St", "OrgMember", "M", "a"); L("OrgMember", "Og", "N", "b")
L("St", "JoinRequest", "M", "a"); L("JoinRequest", "Og", "N", "b")
L("Og", "Organizes", "1", "a"); L("Organizes", "Ev", "N", "b")
L("Ev", "HasSession", "1", "a", total=True); L("HasSession", "Se", "N", "b", total=True)
L("St", "Registers", "M", "a"); L("Registers", "Ev", "N", "b")
L("St", "Attends", "M", "a"); L("Attends", "Se", "N", "b")
L("St", "Earns", "1", "a"); L("Earns", "Sc", "N", "b", total=True)
L("St", "EarnsBadge", "M", "a"); L("EarnsBadge", "Bd", "N", "b")
L("St", "Receives", "1", "a"); L("Receives", "No", "N", "b", total=True)

# ---------------- attributes ----------------
# Student: inner arc r=600, outer arc r=900 (top of the entity)
A("St", "StudentID", "key", 232, 600)
A("St", "Email", "simple", 251, 600)
A("St", "Name", "composite", 270, 600, children=[("FirstName", 258, 900), ("LastName", 282, 900)])
A("St", "PasswordHash", "simple", 289, 600)
A("St", "AcademicYear", "simple", 308, 600)
A("St", "Role", "simple", 222, 900)
A("St", "Interest", "multi", 241.5, 900)
A("St", "TotalPoints", "derived", 298.5, 900)
A("St", "Level", "derived", 318, 900)

fan("Un", [("UniversityID", "key"), ("Name", "simple"), ("City", "simple"),
           ("Country", "simple"), ("Website", "simple")], 150, 330, [300, 470])
fan("Fa", [("FacultyID", "key"), ("Name", "simple"), ("Code", "simple")], 235, 305, [330])
col("Pf", [("ProfileID", "key"), ("Bio", "simple"), ("PhotoUrl", "simple"),
           ("GitHubUrl", "simple"), ("LinkedInUrl", "simple")], 380, 70)
col("No", [("NotificationID", "key"), ("Title", "simple"), ("Message", "simple"),
           ("Type", "simple"), ("IsRead", "simple"), ("CreatedAt", "simple")], 380, 70)
col("Sc", [("TransactionID", "key"), ("Points", "simple"), ("Reason", "simple"),
           ("SourceType", "simple"), ("CreatedAt", "simple")], 380, 70)
col("Bd", [("BadgeID", "key"), ("Name", "simple"), ("Description", "simple"),
           ("IconUrl", "simple"), ("CriteriaType", "simple"), ("CriteriaValue", "simple")], 380, 70)
fan("Og", [("OrganizationID", "key"), ("Name", "simple"), ("Description", "simple"),
           ("Category", "simple"), ("LogoUrl", "simple")], 100, 230, [320, 500])
fan("Ev", [("EventID", "key"), ("Title", "simple"), ("Description", "simple"),
           ("Type", "simple"), ("Location", "simple"), ("StartDate", "simple"),
           ("EndDate", "simple"), ("Capacity", "simple"), ("RegisteredCount", "derived")],
    40, 140, [330, 520, 710])
fan("Se", [("SessionID", "partial"), ("Title", "simple"), ("StartTime", "simple"),
           ("EndTime", "simple"), ("Room", "simple")], 330, 470, [320, 500])
fan("Sk", [("SkillID", "key"), ("Name", "simple"), ("Category", "simple"),
           ("Description", "simple")], 190, 350, [300, 470])
fan("Pr", [("ProjectID", "key"), ("Title", "simple"), ("Description", "simple"),
           ("Status", "simple"), ("CreatedAt", "simple"), ("MaxTeamSize", "simple"),
           ("TeamSize", "derived")], 300, 420, [340, 540])
fan("Tk", [("TaskID", "key"), ("Title", "simple"), ("Description", "simple"),
           ("Status", "simple"), ("DueDate", "simple"), ("Priority", "simple")], 330, 480, [330, 520])

# relationship attributes
A("HasSkill", "ProficiencyLevel", "simple", 270, 270)
A("HasSkill", "YearsOfExperience", "simple", 215, 270)
A("Requires", "RequiredLevel", "simple", 330, 270)
A("Requires", "IsMandatory", "simple", 30, 270)
A("MemberOf", "Role", "simple", 290, 250)
A("MemberOf", "JoinedAt", "simple", 70, 250)
A("AppliesTo", "Message", "simple", 25, 270)
A("AppliesTo", "Status", "simple", 55, 270)
A("AppliesTo", "AppliedAt", "simple", 85, 270)
A("Registers", "RegisteredAt", "simple", 320, 260)
A("Registers", "Status", "simple", 20, 270)
A("Attends", "CheckInTime", "simple", 280, 270)
A("Attends", "VerificationMethod", "simple", 340, 270)
A("OrgMember", "Role", "simple", 60, 260)
A("OrgMember", "JoinedAt", "simple", 110, 260)
A("JoinRequest", "Message", "simple", 165, 280)
A("JoinRequest", "Status", "simple", 195, 280)
A("JoinRequest", "AppliedAt", "simple", 165, 480)
A("JoinRequest", "ReviewedAt", "simple", 195, 480)
A("EarnsBadge", "AwardedAt", "simple", 190, 230)


# ---------------- geometry ----------------
def rect_intersect(cx, cy, w, h, tx, ty):
    dx, dy = tx - cx, ty - cy
    if dx == 0 and dy == 0:
        return cx, cy
    sx = (w / 2) / abs(dx) if dx else 1e9
    sy = (h / 2) / abs(dy) if dy else 1e9
    scale = min(sx, sy)
    return cx + dx * scale, cy + dy * scale


# Explicit anchor coordinates on Student and Project boundaries
STUDENT_ANCHORS = {
    # Right edge (x = 3700)
    "HasSkill":    (3700, 2375),
    "Creates":     (3700, 2395),
    "MemberOf":    (3700, 2418),
    "AppliesTo":   (3700, 2438),
    # Bottom edge (y = 2447.5)
    "AssignedTo":  (3570, 2448),
    "Attends":     (3430, 2448),
    "Registers":   (3280, 2448),
    "OrgMember":   (3120, 2448),
    "JoinRequest": (2960, 2448),
    "EarnsBadge":  (2800, 2448),
    # Left edge (x = 2700)
    "Earns":       (2700, 2435),
    "Receives":    (2700, 2410),
    "HasProfile":  (2700, 2385),
    "StudiesAt":   (2700, 2365),
}

PROJECT_ANCHORS = {
    # Left edge (x = 5700 - 160 = 5540)
    "Requires":  (5540, 2368),
    "Creates":   (5540, 2390),
    "MemberOf":  (5540, 2415),
    "AppliesTo": (5540, 2438),
    # Bottom edge
    "HasTask":   (5700, 2448),
}


def border_pt(node_id, toward_id):
    if node_id == "St" and toward_id in STUDENT_ANCHORS:
        return STUDENT_ANCHORS[toward_id]
    if node_id == "Pr" and toward_id in PROJECT_ANCHORS:
        return PROJECT_ANCHORS[toward_id]
    n, t = nodes[node_id], nodes[toward_id]
    if n["t"] == "entity":
        return rect_intersect(n["x"], n["y"], n["w"], n["h"], t["x"], t["y"])
    elif n["t"] == "rel":
        dx, dy = t["x"] - n["x"], t["y"] - n["y"]
        if dx == 0 and dy == 0:
            return n["x"], n["y"]
        w2, h2 = n["w"] / 2, n["h"] / 2
        scale = 1.0 / (abs(dx) / w2 + abs(dy) / h2)
        return n["x"] + dx * scale, n["y"] + dy * scale
    return n["x"], n["y"]


def pts(e):
    a_id, b_id = e["a"], e["b"]
    a, b = nodes[a_id], nodes[b_id]
    if a["t"] == "attr" or b["t"] == "attr":
        return a["x"], a["y"], b["x"], b["y"]
    ax, ay = border_pt(a_id, b_id)
    bx, by = border_pt(b_id, a_id)
    return ax, ay, bx, by


def label_pos(e):
    eid = e["a"] if e["ent"] == "a" else e["b"]
    other_id = e["b"] if e["ent"] == "a" else e["a"]
    bx, by = border_pt(eid, other_id)
    other = nodes[other_id]
    dx, dy = other["x"] - bx, other["y"] - by
    ln = math.hypot(dx, dy) or 1
    ux, uy = dx / ln, dy / ln
    nx, ny = -uy, ux
    return bx + ux * 42 + nx * (e["off"] * 0.6), by + uy * 42 + ny * (e["off"] * 0.6)


def svg():
    o = [f'<svg xmlns="http://www.w3.org/2000/svg" width="{W}" height="{H}" viewBox="0 0 {W} {H}" '
         'font-family="Arial, Helvetica, sans-serif" font-size="18">',
         f'<rect width="{W}" height="{H}" fill="white"/>',
         '<text x="60" y="80" font-size="40" font-weight="bold" fill="#0b2e59">'
         'TeamUpDB - Chen ERD</text>']
    for e in edges:
        ax, ay, bx, by = pts(e)
        if e["double"]:
            dx, dy = bx - ax, by - ay
            ln = math.hypot(dx, dy) or 1
            nx, ny = -dy / ln * 3.2, dx / ln * 3.2
            for s in (1, -1):
                o.append(f'<line x1="{ax+nx*s:.1f}" y1="{ay+ny*s:.1f}" x2="{bx+nx*s:.1f}" '
                         f'y2="{by+ny*s:.1f}" stroke="#222" stroke-width="1.6"/>')
        else:
            o.append(f'<line x1="{ax:.1f}" y1="{ay:.1f}" x2="{bx:.1f}" y2="{by:.1f}" '
                     'stroke="#222" stroke-width="1.6"/>')
    for n in nodes.values():
        x, y, w, h = n["x"], n["y"], n["w"], n["h"]
        if n["t"] == "entity":
            o.append(f'<rect x="{x-w/2}" y="{y-h/2}" width="{w}" height="{h}" fill="#dbe9ff" '
                     'stroke="#1f3a68" stroke-width="2.6"/>')
            if n["weak"]:
                o.append(f'<rect x="{x-w/2+7}" y="{y-h/2+7}" width="{w-14}" height="{h-14}" '
                         'fill="none" stroke="#1f3a68" stroke-width="2.2"/>')
            o.append(f'<text x="{x}" y="{y+7}" text-anchor="middle" font-size="26" '
                     f'font-weight="bold">{escape(n["label"])}</text>')
        elif n["t"] == "rel":
            pts_ = f'{x},{y-h/2} {x+w/2},{y} {x},{y+h/2} {x-w/2},{y}'
            o.append(f'<polygon points="{pts_}" fill="#ffe9c7" stroke="#8a5a00" stroke-width="2.2"/>')
            if n["ident"]:
                i2 = f'{x},{y-h/2+9} {x+w/2-17},{y} {x},{y+h/2-9} {x-w/2+17},{y}'
                o.append(f'<polygon points="{i2}" fill="none" stroke="#8a5a00" stroke-width="2"/>')
            o.append(f'<text x="{x}" y="{y+5}" text-anchor="middle" font-weight="bold">'
                     f'{escape(n["label"])}</text>')
        else:
            k = n["kind"]
            dash = ' stroke-dasharray="7 5"' if k == "derived" else ""
            o.append(f'<ellipse cx="{x:.1f}" cy="{y:.1f}" rx="{w/2:.1f}" ry="{h/2}" fill="#e6f7e6" '
                     f'stroke="#2f6b2f" stroke-width="1.8"{dash}/>')
            if k == "multi":
                o.append(f'<ellipse cx="{x:.1f}" cy="{y:.1f}" rx="{w/2-7:.1f}" ry="{h/2-7}" '
                         'fill="none" stroke="#2f6b2f" stroke-width="1.8"/>')
            tw = 9.0 * len(n["label"])
            if k in ("key", "partial"):
                d2 = ' stroke-dasharray="5 3"' if k == "partial" else ""
                o.append(f'<text x="{x:.1f}" y="{y+5:.1f}" text-anchor="middle">{escape(n["label"])}</text>')
                o.append(f'<line x1="{x-tw/2:.1f}" y1="{y+9:.1f}" x2="{x+tw/2:.1f}" y2="{y+9:.1f}" '
                         f'stroke="#111" stroke-width="1.3"{d2}/>')
            else:
                o.append(f'<text x="{x:.1f}" y="{y+5:.1f}" text-anchor="middle">{escape(n["label"])}</text>')
    for e in edges:
        if e["label"]:
            lx, ly = label_pos(e)
            o.append(f'<text x="{lx:.1f}" y="{ly+6:.1f}" text-anchor="middle" font-size="24" '
                     f'font-weight="bold" fill="#b00020">{e["label"]}</text>')
    o += legend()
    o.append("</svg>")
    return "\n".join(o)


def legend():
    x0, y0 = 3000, 120
    o = [f'<rect x="{x0}" y="{y0}" width="900" height="480" fill="#fafafa" stroke="#999"/>',
         f'<text x="{x0+20}" y="{y0+38}" font-size="24" font-weight="bold">Legend</text>']
    rows = [("rect", "Entity"), ("weak", "Weak entity"), ("diamond", "Relationship"),
            ("idiam", "Identifying relationship"), ("simple", "Simple attribute"),
            ("key", "Key attribute (solid underline)"), ("partial", "Partial key (dashed underline)"),
            ("multi", "Multivalued attribute (double oval)"),
            ("derived", "Derived attribute (dashed oval)"), ("dbl", "Total participation (double line)")]
    y = y0 + 80
    for k, text in rows:
        cx = x0 + 80
        if k in ("rect", "weak"):
            o.append(f'<rect x="{cx-38}" y="{y-14}" width="76" height="28" fill="#dbe9ff" stroke="#1f3a68" stroke-width="2"/>')
            if k == "weak":
                o.append(f'<rect x="{cx-32}" y="{y-8}" width="64" height="16" fill="none" stroke="#1f3a68" stroke-width="1.6"/>')
        elif k in ("diamond", "idiam"):
            o.append(f'<polygon points="{cx},{y-18} {cx+40},{y} {cx},{y+18} {cx-40},{y}" fill="#ffe9c7" stroke="#8a5a00" stroke-width="2"/>')
            if k == "idiam":
                o.append(f'<polygon points="{cx},{y-12} {cx+30},{y} {cx},{y+12} {cx-30},{y}" fill="none" stroke="#8a5a00" stroke-width="1.6"/>')
        elif k == "dbl":
            for dy in (-3, 3):
                o.append(f'<line x1="{cx-38}" y1="{y+dy}" x2="{cx+38}" y2="{y+dy}" stroke="#222" stroke-width="1.6"/>')
        else:
            dash = ' stroke-dasharray="6 4"' if k == "derived" else ""
            o.append(f'<ellipse cx="{cx}" cy="{y}" rx="40" ry="16" fill="#e6f7e6" stroke="#2f6b2f" stroke-width="1.8"{dash}/>')
            if k == "multi":
                o.append(f'<ellipse cx="{cx}" cy="{y}" rx="33" ry="10" fill="none" stroke="#2f6b2f" stroke-width="1.6"/>')
            if k in ("key", "partial"):
                d2 = ' stroke-dasharray="4 3"' if k == "partial" else ""
                o.append(f'<text x="{cx}" y="{y+4}" text-anchor="middle" font-size="13">ID</text>')
                o.append(f'<line x1="{cx-9}" y1="{y+8}" x2="{cx+9}" y2="{y+8}" stroke="#111"{d2}/>')
        o.append(f'<text x="{x0+170}" y="{y+5}" font-size="17">{escape(text)}</text>')
        y += 38
    return o


def drawio():
    cells = ['<mxCell id="0"/>', '<mxCell id="1" parent="0"/>']
    for n in nodes.values():
        x, y = n["x"] - n["w"] / 2, n["y"] - n["h"] / 2
        if n["t"] == "entity":
            st = "whiteSpace=wrap;html=1;fillColor=#dbe9ff;strokeColor=#1f3a68;strokeWidth=2;fontStyle=1;"
            if n["weak"]:
                st += "shape=ext;double=1;"
        elif n["t"] == "rel":
            st = "rhombus;whiteSpace=wrap;html=1;fillColor=#ffe9c7;strokeColor=#8a5a00;fontStyle=1;"
            if n["ident"]:
                st += "double=1;"
        else:
            st = ("shape=doubleEllipse;" if n["kind"] == "multi" else "ellipse;") + \
                 "whiteSpace=wrap;html=1;fillColor=#e6f7e6;strokeColor=#2f6b2f;"
            if n["kind"] == "derived":
                st += "dashed=1;"
            if n["kind"] == "key":
                st += "fontStyle=4;"
        cells.append(f'<mxCell id="{n["id"]}" value="{escape(n["label"])}" style="{st}" vertex="1" parent="1">'
                     f'<mxGeometry x="{x:.0f}" y="{y:.0f}" width="{n["w"]:.0f}" height="{n["h"]:.0f}" as="geometry"/></mxCell>')
    for i, e in enumerate(edges):
        st = "endArrow=none;html=1;" + ("shape=link;" if e["double"] else "")
        cells.append(f'<mxCell id="e{i}" style="{st}" edge="1" parent="1" source="{e["a"]}" target="{e["b"]}">'
                     '<mxGeometry relative="1" as="geometry"/></mxCell>')
        if e["label"]:
            lx, ly = label_pos(e)
            cells.append(f'<mxCell id="l{i}" value="{e["label"]}" style="text;html=1;fontStyle=1;fontColor=#b00020;" '
                         f'vertex="1" parent="1"><mxGeometry x="{lx-15:.0f}" y="{ly-12:.0f}" width="30" height="24" as="geometry"/></mxCell>')
    return ('<mxfile><diagram name="TeamUp Chen ERD"><mxGraphModel grid="1" page="0"><root>'
            + "".join(cells) + '</root></mxGraphModel></diagram></mxfile>')


if __name__ == "__main__":
    out = Path(sys.argv[1])
    out.mkdir(parents=True, exist_ok=True)
    (out / "TeamUp-Chen-ERD.svg").write_text(svg(), encoding="utf-8")
    (out / "TeamUp-Chen-ERD.drawio").write_text(drawio(), encoding="utf-8")
    c = lambda t: sum(1 for n in nodes.values() if n["t"] == t)
    print("entities", c("entity"), "relationships", c("rel"), "attributes", c("attr"))
