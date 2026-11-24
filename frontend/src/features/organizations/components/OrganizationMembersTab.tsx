/**
 * @date 2026-11-24
 * @purpose Organization roster tab supporting Egyptian student-activity roles, unit assignment, tier badges, member removal, and Leave Organization.
 */
import { LogOut, UserMinus, Users } from "lucide-react";
import { useState } from "react";
import { Link } from "react-router-dom";
import { toast } from "sonner";
import {
  ORGANIZATION_ROLES,
  type OrganizationMember,
  type OrganizationUnit,
  type UpdateMemberRolePayload,
} from "@/api";
import { Badge, Button, ConfirmDialog, EmptyState, Select, StatusPill } from "@/components/ui";

export interface OrganizationMembersTabProps {
  members: readonly OrganizationMember[];
  units?: readonly OrganizationUnit[];
  currentStudentId?: number;
  canManage: boolean;
  isBusy: boolean;
  onUpdateRole: (
    studentId: number,
    roleOrPayload: string | UpdateMemberRolePayload,
  ) => Promise<void>;
  onRemoveMember: (studentId: number) => Promise<void>;
}

const BASE_ROLE_OPTIONS = Object.values(ORGANIZATION_ROLES).map((role) => ({
  value: role,
  label: role,
}));

export function OrganizationMembersTab({
  members,
  units = [],
  currentStudentId,
  canManage,
  isBusy,
  onUpdateRole,
  onRemoveMember,
}: OrganizationMembersTabProps) {
  const [confirmTarget, setConfirmTarget] = useState<{
    studentId: number;
    label: string;
    isSelfLeave: boolean;
  } | null>(null);

  if (members.length === 0) {
    return (
      <EmptyState
        icon={<Users className="size-6" />}
        title="No members yet"
        description="Accepted organization members will appear in this roster."
      />
    );
  }

  const unitOptions = [
    { value: "0", label: "Organization-wide" },
    ...units.map((u) => ({ value: String(u.organizationUnitId), label: `${u.code} · ${u.name}` })),
  ];

  const handleConfirm = async () => {
    if (!confirmTarget) {
      return;
    }
    try {
      await onRemoveMember(confirmTarget.studentId);
      toast.success(
        confirmTarget.isSelfLeave
          ? "You left the organization."
          : `${confirmTarget.label} was removed from the organization.`,
      );
      setConfirmTarget(null);
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Could not update organization roster.");
    }
  };

  return (
    <div className="space-y-3 pt-2">
      {members.map((member) => {
        const isSelf = currentStudentId === member.studentId;
        const fullName = `${member.firstName} ${member.lastName}`;
        const roleOptions = BASE_ROLE_OPTIONS.some((o) => o.value === member.role)
          ? BASE_ROLE_OPTIONS
          : [{ value: member.role, label: member.role }, ...BASE_ROLE_OPTIONS];

        return (
          <div
            key={member.studentId}
            className="flex flex-wrap items-center justify-between gap-3 rounded-2xl border border-white/10 bg-[#14151c] p-4"
          >
            <div className="min-w-0">
              <div className="flex flex-wrap items-center gap-2">
                <Link
                  to={`/students/${member.studentId}`}
                  className="font-semibold text-white transition-colors hover:text-[#d3fb52]"
                >
                  {fullName}
                </Link>
                {isSelf && <StatusPill status="Confirmed" label="You" showDot={false} />}
                {member.unitCode && (
                  <Badge variant="outline" size="sm">
                    {member.unitCode}
                  </Badge>
                )}
                {member.hierarchyTier && (
                  <span className="rounded-md bg-white/[0.06] px-2 py-0.5 font-mono text-[10px] text-white/65">
                    {member.hierarchyTier}
                  </span>
                )}
              </div>
              {member.email && <p className="text-xs text-white/50">{member.email}</p>}
            </div>

            <div className="flex flex-wrap items-center gap-2">
              {canManage ? (
                <>
                  <Select
                    aria-label={`Role for ${fullName}`}
                    options={roleOptions}
                    value={member.role}
                    disabled={isBusy}
                    containerClassName="w-full sm:w-auto"
                    className="h-9 w-full bg-[#1b1d26] text-xs sm:min-w-[140px]"
                    onChange={async (e) => {
                      const nextRole = e.target.value;
                      try {
                        await onUpdateRole(member.studentId, {
                          role: nextRole,
                          organizationUnitId: member.organizationUnitId ?? null,
                        });
                        toast.success(`Updated ${fullName}'s role to ${nextRole}.`);
                      } catch (err) {
                        toast.error(err instanceof Error ? err.message : "Failed to update role.");
                      }
                    }}
                  />
                  {units.length > 0 && (
                    <Select
                      aria-label={`Unit for ${fullName}`}
                      options={unitOptions}
                      value={member.organizationUnitId ? String(member.organizationUnitId) : "0"}
                      disabled={isBusy}
                      containerClassName="w-full sm:w-auto"
                      className="h-9 w-full bg-[#1b1d26] text-xs sm:min-w-[155px]"
                      onChange={async (e) => {
                        const val = Number(e.target.value);
                        try {
                          await onUpdateRole(member.studentId, {
                            role: member.role,
                            organizationUnitId: val,
                          });
                          toast.success(`Assigned ${fullName}'s unit.`);
                        } catch (err) {
                          toast.error(err instanceof Error ? err.message : "Failed to update unit.");
                        }
                      }}
                    />
                  )}
                  {!isSelf && (
                    <Button
                      size="sm"
                      variant="danger"
                      leftIcon={<UserMinus className="size-3.5" />}
                      disabled={isBusy}
                      onClick={() =>
                        setConfirmTarget({
                          studentId: member.studentId,
                          label: fullName,
                          isSelfLeave: false,
                        })
                      }
                    >
                      Remove
                    </Button>
                  )}
                </>
              ) : (
                <StatusPill status={member.role} showDot={false} />
              )}

              {isSelf && (
                <Button
                  size="sm"
                  variant="outline"
                  leftIcon={<LogOut className="size-3.5" />}
                  disabled={isBusy}
                  onClick={() =>
                    setConfirmTarget({
                      studentId: member.studentId,
                      label: fullName,
                      isSelfLeave: true,
                    })
                  }
                >
                  Leave organization
                </Button>
              )}
            </div>
          </div>
        );
      })}

      <ConfirmDialog
        open={Boolean(confirmTarget)}
        onClose={() => setConfirmTarget(null)}
        onConfirm={handleConfirm}
        isLoading={isBusy}
        title={confirmTarget?.isSelfLeave ? "Leave organization?" : "Remove organization member?"}
        description={
          confirmTarget?.isSelfLeave
            ? "You will lose member access to this organization and need to reapply to rejoin."
            : `Remove ${confirmTarget?.label ?? "this member"} from the organization roster?`
        }
        confirmLabel={confirmTarget?.isSelfLeave ? "Leave organization" : "Remove member"}
      />
    </div>
  );
}
