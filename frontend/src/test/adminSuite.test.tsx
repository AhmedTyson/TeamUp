/**
 * @date 2026-11-22
 * @purpose Vitest integration suite for adminApi and the M1 Phase 2 PlatformAdmin governance suite (/admin, /admin/catalog, /admin/roles, /admin/restore, /admin/audit-logs).
 */
import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import {
  adminApi,
  apiClient,
  ApiClientError,
  universitiesApi,
  USER_ROLES,
  type AuthStudentSummary,
} from "@/api";
import { AppCoreProviders } from "@/app/providers";
import {
  AdminAuditLogsPage,
  AdminCatalogPage,
  AdminDashboardPage,
  AdminRestorePage,
  AdminRolesPage,
} from "@/features/admin";

const ADMIN_USER: AuthStudentSummary = {
  studentId: 1,
  firstName: "Tyson",
  lastName: "Admin",
  email: "admin@teamup.edu",
  academicYear: 4,
  role: USER_ROLES.PlatformAdmin,
  facultyId: 1,
  facultyName: "Computer Science",
  universityName: "Cairo University",
};

describe("M1 Phase 2 — PlatformAdmin Governance Suite", () => {
  beforeEach(() => {
    vi.restoreAllMocks();
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it("wraps all 5 Admin endpoints cleanly in adminApi", async () => {
    const getSpy = vi.spyOn(apiClient, "get").mockResolvedValue({
      data: {
        success: true,
        message: "OK",
        statusCode: 200,
        data: {},
        timestamp: new Date().toISOString(),
      },
      status: 200,
    } as never);
    const patchSpy = vi.spyOn(apiClient, "patch").mockResolvedValue({
      data: {
        success: true,
        message: "Updated",
        statusCode: 200,
        data: ADMIN_USER,
        timestamp: new Date().toISOString(),
      },
      status: 200,
    } as never);
    const postSpy = vi.spyOn(apiClient, "post").mockResolvedValue({
      data: {
        success: true,
        message: "student with ID 5 has been restored successfully.",
        statusCode: 200,
        timestamp: new Date().toISOString(),
      },
      status: 200,
    } as never);

    await adminApi.getDashboard();
    expect(getSpy).toHaveBeenCalledWith("/api/v1/admin/dashboard", { signal: undefined });

    await adminApi.updateStudentRole(5, { role: USER_ROLES.OrgAdmin });
    expect(patchSpy).toHaveBeenCalledWith("/api/v1/admin/students/5/role", {
      role: "OrgAdmin",
    });

    await adminApi.getSoftDeleted("university");
    expect(getSpy).toHaveBeenCalledWith("/api/v1/admin/soft-deleted", {
      params: { entityType: "university" },
      signal: undefined,
    });

    const msg = await adminApi.restoreEntity("student", 5);
    expect(postSpy).toHaveBeenCalledWith("/api/v1/admin/restore/student/5");
    expect(msg).toContain("restored");

    await adminApi.getAuditLogs({ action: "RoleChanged", entityType: "Student", page: 1 });
    expect(getSpy).toHaveBeenCalledWith("/api/v1/admin/audit-logs", {
      params: { action: "RoleChanged", entityType: "Student", page: 1 },
      signal: undefined,
    });
  });

  it("renders AdminDashboardPage KPIs and role distribution percentages", async () => {
    vi.spyOn(adminApi, "getDashboard").mockResolvedValueOnce({
      activeStudentsCount: 100,
      activeUniversitiesCount: 5,
      activeFacultiesCount: 18,
      softDeletedStudentsCount: 2,
      softDeletedUniversitiesCount: 1,
      softDeletedFacultiesCount: 3,
      roleDistribution: {
        Student: 80,
        OrgAdmin: 15,
        PlatformAdmin: 5,
      },
    });

    render(
      <MemoryRouter initialEntries={["/admin"]}>
        <AppCoreProviders initialUser={ADMIN_USER}>
          <AdminDashboardPage />
        </AppCoreProviders>
      </MemoryRouter>,
    );

    expect(await screen.findByText("100")).toBeInTheDocument();
    expect(screen.getByText("5")).toBeInTheDocument();
    expect(screen.getByText("18")).toBeInTheDocument();
    expect(screen.getByText("80 (80%)")).toBeInTheDocument();
    expect(screen.getByText("15 (15%)")).toBeInTheDocument();
  });

  it("enforces THR-T-03 safe URL validation and manages Universities + nested Faculties in AdminCatalogPage", async () => {
    vi.spyOn(universitiesApi, "list").mockResolvedValue({
      items: [
        {
          universityId: 1,
          name: "Cairo University",
          city: "Giza",
          country: "Egypt",
          website: "https://cu.edu.eg",
          facultyCount: 1,
        },
      ],
      page: 1,
      pageSize: 10,
      totalCount: 1,
      totalPages: 1,
    });
    const createUniSpy = vi.spyOn(universitiesApi, "create").mockResolvedValueOnce({
      universityId: 2,
      name: "Alexandria University",
      city: "Alexandria",
      country: "Egypt",
      website: "https://alexu.edu.eg",
      faculties: [],
    });
    vi.spyOn(universitiesApi, "getFaculties").mockResolvedValueOnce([
      {
        facultyId: 11,
        name: "Computer Science",
        code: "CS",
        universityId: 1,
        universityName: "Cairo University",
        studentCount: 42,
      },
    ]);
    const createFacultySpy = vi.spyOn(universitiesApi, "createFaculty").mockResolvedValueOnce({
      facultyId: 12,
      name: "Engineering",
      code: "ENG",
      universityId: 1,
      universityName: "Cairo University",
      studentCount: 0,
    });

    render(
      <MemoryRouter initialEntries={["/admin/catalog"]}>
        <AppCoreProviders initialUser={ADMIN_USER}>
          <AdminCatalogPage />
        </AppCoreProviders>
      </MemoryRouter>,
    );

    expect(await screen.findByText("Cairo University")).toBeInTheDocument();

    // Create University with unsafe URL blocked client-side first
    fireEvent.click(screen.getByRole("button", { name: /add university/i }));
    fireEvent.change(screen.getByLabelText(/^university name/i), {
      target: { value: "Alexandria University" },
    });
    fireEvent.change(screen.getByLabelText(/^city/i), { target: { value: "Alexandria" } });
    fireEvent.change(screen.getByLabelText(/^country/i), { target: { value: "Egypt" } });
    fireEvent.change(screen.getByLabelText(/official website/i), {
      target: { value: "http://localhost:8080" },
    });
    fireEvent.click(screen.getByRole("button", { name: /create university/i }));

    await waitFor(() => {
      expect(
        screen.getByText(/website must be a valid public http or https url/i),
      ).toBeInTheDocument();
    });

    fireEvent.change(screen.getByLabelText(/official website/i), {
      target: { value: "https://alexu.edu.eg" },
    });
    fireEvent.click(screen.getByRole("button", { name: /create university/i }));

    await waitFor(() => {
      expect(createUniSpy).toHaveBeenCalledWith({
        name: "Alexandria University",
        city: "Alexandria",
        country: "Egypt",
        website: "https://alexu.edu.eg",
      });
    });

    // Manage Faculties for Cairo University
    fireEvent.click(screen.getByRole("button", { name: /manage faculties/i }));
    expect(await screen.findByText(/faculties · cairo university/i)).toBeInTheDocument();

    fireEvent.click(screen.getByRole("button", { name: /add faculty/i }));
    fireEvent.change(screen.getByLabelText(/^faculty name/i), {
      target: { value: "Engineering" },
    });
    fireEvent.change(screen.getByLabelText(/^academic code/i), {
      target: { value: "eng" },
    });
    fireEvent.click(screen.getByRole("button", { name: /create faculty/i }));

    await waitFor(() => {
      expect(createFacultySpy).toHaveBeenCalledWith(1, {
        name: "Engineering",
        code: "ENG",
      });
    });
  });

  it("handles THR-E-03 403 Forbidden guard and SecurityStamp role update in AdminRolesPage", async () => {
    vi.spyOn(adminApi, "searchStudents").mockResolvedValueOnce({
      items: [
        {
          studentId: 12,
          firstName: "Sara",
          lastName: "Adel",
          email: "sara@cu.edu.eg",
          academicYear: 2,
          role: USER_ROLES.Student,
          facultyId: 11,
          facultyName: "Computer Science",
          universityName: "Cairo University",
          interests: [],
        },
      ],
      page: 1,
      pageSize: 10,
      totalCount: 1,
      totalPages: 1,
    });
    const roleSpy = vi
      .spyOn(adminApi, "updateStudentRole")
      .mockRejectedValueOnce(
        new ApiClientError(
          "Runtime elevation to PlatformAdmin is restricted. Provision PlatformAdmin accounts via bootstrap configuration.",
          403,
        ),
      )
      .mockResolvedValueOnce({
        ...ADMIN_USER,
        studentId: 12,
        firstName: "Sara",
        lastName: "Adel",
        email: "sara@cu.edu.eg",
        role: USER_ROLES.OrgAdmin,
      });

    render(
      <MemoryRouter initialEntries={["/admin/roles"]}>
        <AppCoreProviders initialUser={ADMIN_USER}>
          <AdminRolesPage />
        </AppCoreProviders>
      </MemoryRouter>,
    );

    expect(await screen.findByText(/sara adel/i)).toBeInTheDocument();
    fireEvent.click(screen.getByRole("button", { name: /change role/i }));

    // Trigger PlatformAdmin guard -> 403 Forbidden banner inside modal
    fireEvent.change(screen.getByLabelText(/target role/i), {
      target: { value: USER_ROLES.PlatformAdmin },
    });
    expect(screen.getByText(/system administrator restriction/i)).toBeInTheDocument();

    fireEvent.click(screen.getByRole("button", { name: /confirm role update/i }));
    await waitFor(() => {
      expect(
        screen.getByText(/runtime elevation to platformadmin is restricted/i),
      ).toBeInTheDocument();
    });

    // Switch to OrgAdmin -> succeeds
    fireEvent.change(screen.getByLabelText(/target role/i), {
      target: { value: USER_ROLES.OrgAdmin },
    });
    fireEvent.click(screen.getByRole("button", { name: /confirm role update/i }));

    await waitFor(() => {
      expect(roleSpy).toHaveBeenLastCalledWith(12, { role: USER_ROLES.OrgAdmin });
    });
  });

  it("filters 8 restorable entities and invokes instant Restore in AdminRestorePage", async () => {
    const softDeletedSpy = vi.spyOn(adminApi, "getSoftDeleted").mockResolvedValue([
      {
        id: 7,
        entityType: "University",
        displayName: "Helwan University",
        deletedAt: "2026-11-21T12:00:00Z",
      },
    ]);
    const restoreSpy = vi
      .spyOn(adminApi, "restoreEntity")
      .mockResolvedValueOnce("university with ID 7 has been restored successfully.");

    render(
      <MemoryRouter initialEntries={["/admin/restore"]}>
        <AppCoreProviders initialUser={ADMIN_USER}>
          <AdminRestorePage />
        </AppCoreProviders>
      </MemoryRouter>,
    );

    expect(await screen.findByText("Helwan University")).toBeInTheDocument();

    fireEvent.click(screen.getByRole("button", { name: /^faculty$/i }));
    await waitFor(() => {
      expect(softDeletedSpy).toHaveBeenCalledWith("faculty", expect.any(AbortSignal));
    });

    const restoreBtn = await screen.findByRole("button", { name: /^restore$/i });
    fireEvent.click(restoreBtn);
    await waitFor(() => {
      expect(restoreSpy).toHaveBeenCalledWith("university", 7);
    });
  });

  it("filters immutable audit logs and inspects Old/New JSON diff in AdminAuditLogsPage", async () => {
    const logsSpy = vi.spyOn(adminApi, "getAuditLogs").mockResolvedValue({
      items: [
        {
          auditLogId: 44,
          actorStudentId: 1,
          actorEmail: "admin@teamup.edu",
          action: "RoleChanged",
          entityType: "Student",
          entityId: "12",
          oldValues: JSON.stringify({ Role: "Student" }),
          newValues: JSON.stringify({ Role: "OrgAdmin" }),
          clientIp: "127.0.0.1",
          occurredAt: "2026-11-21T15:00:00Z",
        },
      ],
      page: 1,
      pageSize: 20,
      totalCount: 1,
      totalPages: 1,
    });

    render(
      <MemoryRouter initialEntries={["/admin/audit-logs"]}>
        <AppCoreProviders initialUser={ADMIN_USER}>
          <AdminAuditLogsPage />
        </AppCoreProviders>
      </MemoryRouter>,
    );

    expect(await screen.findByText("RoleChanged")).toBeInTheDocument();

    fireEvent.change(screen.getByLabelText(/^entity type/i), {
      target: { value: "Student" },
    });
    await waitFor(() => {
      expect(logsSpy).toHaveBeenCalledWith(
        expect.objectContaining({ entityType: "Student", page: 1, pageSize: 20 }),
        expect.any(AbortSignal),
      );
    });

    const inspectBtn = await screen.findByRole("button", { name: /inspect diff/i });
    fireEvent.click(inspectBtn);
    expect(await screen.findByText(/field-by-field comparison/i)).toBeInTheDocument();
    expect(screen.getByText(/previous values/i)).toBeInTheDocument();
    expect(screen.getByText(/updated values/i)).toBeInTheDocument();
    expect(screen.getByText(/"Role": "OrgAdmin"/)).toBeInTheDocument();
  });
});
