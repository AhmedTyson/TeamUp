/**
 * @date 2026-11-21
 * @purpose Unit tests for TeamUp's shared Handshake UI primitive kit (Button, Input, SearchInput, Modal, ConfirmDialog, Tabs, DataTable, Pagination, Avatar, ErrorBanner).
 */
import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { ApiClientError } from "@/api";
import {
  Avatar,
  Button,
  ConfirmDialog,
  DataTable,
  EmptyState,
  ErrorBanner,
  Input,
  Modal,
  Pagination,
  SearchInput,
  Select,
  SpotlightCard,
  StatusPill,
  Tabs,
  UniversityLogo,
} from "@/components/ui";

describe("Handshake UI primitive kit", () => {
  it("renders Button loading state and form input error accessibility attributes", () => {
    render(
      <div>
        <Button isLoading>Saving</Button>
        <Input label="Email" error="Email is required" />
        <Select
          label="Faculty"
          placeholder="Select faculty"
          options={[{ value: 1, label: "Computer Science" }]}
        />
      </div>,
    );

    const button = screen.getByRole("button", { name: /saving/i });
    expect(button).toBeDisabled();
    expect(button).toHaveAttribute("aria-busy", "true");

    const input = screen.getByLabelText("Email");
    expect(input).toHaveAttribute("aria-invalid", "true");
    expect(screen.getByRole("alert")).toHaveTextContent("Email is required");
    expect(screen.getByLabelText("Faculty")).toBeInTheDocument();
  });

  it("clears SearchInput on Escape and renders StatusPill semantic labels", () => {
    const onChange = vi.fn();
    render(
      <div>
        <SearchInput value="react" onChange={onChange} />
        <StatusPill status="InProgress" />
        <StatusPill status="Waitlisted" />
      </div>,
    );

    const search = screen.getByRole("searchbox");
    fireEvent.keyDown(search, { key: "Escape" });
    expect(onChange).toHaveBeenCalledWith("");
    expect(screen.getByText("In Progress")).toBeInTheDocument();
    expect(screen.getByText("Waitlisted")).toBeInTheDocument();
  });

  it("renders public /images/people Avatar src and falls back to initials when unsafe", () => {
    const { rerender } = render(
      <div>
        <Avatar name="Salma Khaled" src="/images/people/salma-khaled.jpg" />
      </div>,
    );

    const img = screen.getByAltText("Salma Khaled");
    expect(img).toHaveAttribute("src", "/images/people/salma-khaled.jpg");

    rerender(
      <div>
        <Avatar name="Ahmed Elsayed" src="javascript:alert(1)" />
        <Avatar name="Omar Mostafa" src="/images/../secret.jpg" />
      </div>,
    );

    expect(screen.getByRole("img", { name: "Ahmed Elsayed" })).toHaveTextContent("AE");
    expect(screen.getByRole("img", { name: "Omar Mostafa" })).toHaveTextContent("OM");
    expect(document.querySelector("img")).toBeNull();
  });

  it("updates SpotlightCard CSS custom properties on mouse move", () => {
    render(
      <SpotlightCard data-testid="spotlight" showCornerArrow>
        <span>Featured Guild</span>
      </SpotlightCard>,
    );

    const card = screen.getByTestId("spotlight");
    fireEvent.mouseMove(card, { clientX: 50, clientY: 30 });
    expect(card.style.getPropertyValue("--mouse-x")).toBe("50px");
    expect(card.style.getPropertyValue("--mouse-y")).toBe("30px");
  });

  it("closes Modal on Escape and gates ConfirmDialog behind confirmTextMatch", () => {
    const onClose = vi.fn();
    const onConfirm = vi.fn();

    const { rerender } = render(
      <Modal open onClose={onClose} title="Project Details">
        <p>Body content</p>
      </Modal>,
    );

    expect(screen.getByRole("dialog", { name: "Project Details" })).toBeInTheDocument();
    fireEvent.keyDown(window, { key: "Escape" });
    expect(onClose).toHaveBeenCalledTimes(1);

    rerender(
      <ConfirmDialog
        open
        onClose={onClose}
        onConfirm={onConfirm}
        title="Delete Account"
        description="This action soft-deletes your account."
        confirmLabel="Delete Account"
        confirmTextMatch="nour@teamup.edu"
      />,
    );

    const confirmBtn = screen.getByRole("button", { name: "Delete Account" });
    expect(confirmBtn).toBeDisabled();

    fireEvent.change(screen.getByPlaceholderText("nour@teamup.edu"), {
      target: { value: "nour@teamup.edu" },
    });
    expect(confirmBtn).toBeEnabled();
    fireEvent.click(confirmBtn);
    expect(onConfirm).toHaveBeenCalledTimes(1);
  });

  it("supports keyboard navigation in Tabs, DataTable rendering, and Pagination controls", () => {
    const onPageChange = vi.fn();
    const onPageSizeChange = vi.fn();

    render(
      <div>
        <Tabs
          items={[
            { id: "overview", label: "Overview", content: <p>Overview content</p> },
            { id: "board", label: "Board", content: <p>Kanban content</p> },
          ]}
        />
        <DataTable
          columns={[
            { key: "name", header: "Student", cell: (row: { id: number; name: string }) => row.name },
          ]}
          data={[{ id: 1, name: "Youssef" }]}
          keyExtractor={(row) => row.id}
        />
        <Pagination
          page={2}
          totalPages={4}
          totalCount={75}
          pageSize={20}
          pageSizeOptions={[10, 20]}
          onPageChange={onPageChange}
          onPageSizeChange={onPageSizeChange}
        />
        <EmptyState title="No tasks yet" description="Create your first sprint task." />
      </div>,
    );

    const overviewTab = screen.getByRole("tab", { name: "Overview" });
    fireEvent.keyDown(overviewTab, { key: "ArrowRight" });
    expect(screen.getByText("Kanban content")).toBeInTheDocument();

    expect(screen.getByText("Youssef")).toBeInTheDocument();
    fireEvent.click(screen.getByRole("button", { name: "Next page" }));
    expect(onPageChange).toHaveBeenCalledWith(3);

    fireEvent.change(screen.getByLabelText("Items per page"), { target: { value: "10" } });
    expect(onPageSizeChange).toHaveBeenCalledWith(10);
  });

  it("renders rate-limit Retry-After guidance in ErrorBanner", () => {
    const rateLimitError = new ApiClientError("Rate limit exceeded.", 429, [], 45);
    render(<ErrorBanner error={rateLimitError} />);

    expect(screen.getByRole("alert")).toHaveTextContent("Too many requests");
    expect(screen.getByRole("alert")).toHaveTextContent("45s");
  });

  it("renders database logoUrl image in UniversityLogo and falls back on unsafe URL", () => {
    const officialUrl =
      "https://upload.wikimedia.org/wikipedia/commons/3/3c/Cairo_University_new_logo.png";
    const { rerender } = render(
      <UniversityLogo name="Cairo University" logoUrl={officialUrl} />,
    );

    const logoImg = screen.getByAltText("Cairo University official logo");
    expect(logoImg).toHaveAttribute("src", officialUrl);

    rerender(<UniversityLogo name="Cairo University" logoUrl="javascript:alert(1)" />);
    expect(screen.queryByAltText("Cairo University official logo")).not.toBeInTheDocument();
    expect(screen.getByRole("img", { name: "Cairo University crest" })).toBeInTheDocument();
  });
});
