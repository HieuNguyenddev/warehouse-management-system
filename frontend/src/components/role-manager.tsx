"use client";

import { Pencil, Plus, Search, Trash2, X } from "lucide-react";
import { useSearchParams } from "next/navigation";
import { useEffect, useState } from "react";
import { useToast } from "@/components/toast-provider";
import { Role, RoleInput, createRole, deleteRole, searchRoles, updateRole } from "@/lib/roles";

const emptyForm: RoleInput = { name: "", description: "" };

export function RoleManager() {
  const notify = useToast();
  const query = useSearchParams().get("q") ?? "";
  const [keyword, setKeyword] = useState(query);
  const [applied, setApplied] = useState(query);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [items, setItems] = useState<Role[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(0);
  const [loading, setLoading] = useState(true);
  const [editing, setEditing] = useState<Role | null>(null);
  const [creating, setCreating] = useState(false);
  const [form, setForm] = useState<RoleInput>(emptyForm);
  const [formError, setFormError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);
  const [pendingDelete, setPendingDelete] = useState<Role | null>(null);

  useEffect(() => {
    setKeyword(query);
    setApplied(query);
    setPage(1);
    setLoading(true);
  }, [query]);

  useEffect(() => {
    let active = true;
    searchRoles(applied, page, pageSize)
      .then((result) => {
        if (!active) return;
        setItems(result.items);
        setTotalCount(result.totalCount);
        setTotalPages(result.totalPages);
      })
      .catch((reason: Error) => {
        if (active) notify(reason.message, "error");
      })
      .finally(() => {
        if (active) setLoading(false);
      });
    return () => {
      active = false;
    };
  }, [applied, page, pageSize]);

  function openCreate() {
    setCreating(true);
    setEditing(null);
    setForm(emptyForm);
    setFormError(null);
  }

  function openEdit(role: Role) {
    setCreating(false);
    setEditing(role);
    setForm({ name: role.name, description: role.description ?? "" });
    setFormError(null);
  }

  function closeForm() {
    setCreating(false);
    setEditing(null);
    setFormError(null);
  }

  async function save() {
    setSaving(true);
    setFormError(null);
    try {
      if (editing) {
        await updateRole(editing.id, form);
        notify("Đã cập nhật vai trò.");
      } else {
        await createRole(form);
        notify("Đã thêm vai trò.");
      }
      closeForm();
      setLoading(true);
      const result = await searchRoles(applied, page, pageSize);
      setItems(result.items);
      setTotalCount(result.totalCount);
      setTotalPages(result.totalPages);
    } catch (reason) {
      const message = reason instanceof Error ? reason.message : "Không lưu được.";
      setFormError(message);
      notify(message, "error");
    } finally {
      setSaving(false);
      setLoading(false);
    }
  }

  async function confirmDelete() {
    if (!pendingDelete) return;
    try {
      await deleteRole(pendingDelete.id);
      const nextPage = items.length === 1 && page > 1 ? page - 1 : page;
      setPage(nextPage);
      notify("Đã xóa vai trò.");
      setPendingDelete(null);
      setLoading(true);
      const result = await searchRoles(applied, nextPage, pageSize);
      setItems(result.items);
      setTotalCount(result.totalCount);
      setTotalPages(result.totalPages);
    } catch (reason) {
      notify(reason instanceof Error ? reason.message : "Không xóa được.", "error");
      setPendingDelete(null);
    } finally {
      setLoading(false);
    }
  }

  const formOpen = creating || editing !== null;

  return (
    <div className="mx-auto flex max-w-6xl flex-col gap-5">
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h1 className="text-2xl font-semibold text-ink">Vai trò</h1>
          <p className="mt-1 text-sm text-stone-500">{totalCount} vai trò</p>
        </div>
        <button
          type="button"
          onClick={openCreate}
          className="inline-flex items-center gap-2 rounded-xl bg-ink px-4 py-2.5 text-sm font-medium text-white"
        >
          <Plus size={16} /> Thêm vai trò
        </button>
      </div>

      <form
        className="flex flex-col gap-3 sm:flex-row"
        onSubmit={(event) => {
          event.preventDefault();
          setPage(1);
          setApplied(keyword);
          setLoading(true);
        }}
      >
        <label className="relative flex-1">
          <Search size={16} className="absolute top-1/2 left-3 -translate-y-1/2 text-stone-400" />
          <input
            value={keyword}
            onChange={(event) => setKeyword(event.target.value)}
            placeholder="Tìm gần đúng theo tên hoặc mô tả"
            className="w-full rounded-xl border border-line bg-white py-2.5 pr-3 pl-9 text-sm"
          />
        </label>
        <select
          value={pageSize}
          onChange={(event) => {
            setPageSize(Number(event.target.value));
            setPage(1);
            setLoading(true);
          }}
          className="rounded-xl border border-line bg-white px-3 py-2.5 text-sm"
        >
          <option value={5}>5 / trang</option>
          <option value={10}>10 / trang</option>
          <option value={20}>20 / trang</option>
        </select>
        <button type="submit" className="rounded-xl border border-ink px-4 py-2.5 text-sm font-medium text-ink">
          Lọc
        </button>
      </form>

      <div className="overflow-hidden rounded-2xl border border-line bg-white">
        <table className="w-full text-left text-sm">
          <thead className="bg-stone-50 text-stone-500">
            <tr>
              <th className="px-5 py-3 font-medium">Tên</th>
              <th className="px-5 py-3 font-medium">Mô tả</th>
              <th className="px-5 py-3 text-right font-medium">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            {loading ? (
              <tr>
                <td colSpan={3} className="px-5 py-12 text-center text-stone-500">
                  Đang tải...
                </td>
              </tr>
            ) : items.length === 0 ? (
              <tr>
                <td colSpan={3} className="px-5 py-12 text-center text-stone-500">
                  Không có vai trò khớp bộ lọc.
                </td>
              </tr>
            ) : (
              items.map((role) => (
                <tr key={role.id} className="border-t border-line transition-colors hover:bg-paper">
                  <td className="px-5 py-3">
                    <div className="flex items-center gap-3">
                      <span className="flex size-8 items-center justify-center rounded-full bg-brass-soft text-xs font-semibold text-ink">
                        {role.name.trim().charAt(0).toUpperCase()}
                      </span>
                      <span className="font-medium text-ink">{role.name}</span>
                    </div>
                  </td>
                  <td className="px-5 py-3 text-stone-500">{role.description || "—"}</td>
                  <td className="px-5 py-3 text-right">
                    <button type="button" aria-label="Sửa" className="rounded-lg p-2 text-stone-500 hover:bg-stone-100" onClick={() => openEdit(role)}>
                      <Pencil size={16} />
                    </button>
                    <button type="button" aria-label="Xóa" className="rounded-lg p-2 text-red-600 hover:bg-red-50" onClick={() => setPendingDelete(role)}>
                      <Trash2 size={16} />
                    </button>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
        <div className="flex items-center justify-between border-t border-line px-5 py-3 text-sm">
          <span className="text-stone-500">
            Trang {page} / {totalPages || 1}
          </span>
          <div className="flex gap-2">
            <button
              type="button"
              disabled={page <= 1}
              className="rounded-lg border border-line px-3 py-1.5 disabled:opacity-40"
              onClick={() => {
                setPage((current) => current - 1);
                setLoading(true);
              }}
            >
              Trước
            </button>
            <button
              type="button"
              disabled={page >= totalPages}
              className="rounded-lg border border-line px-3 py-1.5 disabled:opacity-40"
              onClick={() => {
                setPage((current) => current + 1);
                setLoading(true);
              }}
            >
              Sau
            </button>
          </div>
        </div>
      </div>

      {formOpen ? (
        <div className="fixed inset-0 z-50 flex justify-end bg-ink/30">
          <form
            className="flex h-full w-full max-w-md flex-col bg-white shadow-xl"
            onSubmit={(event) => {
              event.preventDefault();
              void save();
            }}
          >
            <div className="flex items-center justify-between border-b border-line px-5 py-4">
              <h2 className="font-semibold text-ink">{editing ? "Sửa vai trò" : "Thêm vai trò"}</h2>
              <button type="button" aria-label="Đóng" onClick={closeForm}>
                <X size={18} />
              </button>
            </div>
            <div className="flex flex-1 flex-col gap-4 px-5 py-5">
              {formError ? <p className="rounded-xl bg-red-50 px-3 py-2 text-sm text-red-700">{formError}</p> : null}
              <label className="text-sm font-medium text-ink">
                Tên
                <input
                  required
                  maxLength={150}
                  value={form.name}
                  onChange={(event) => setForm({ ...form, name: event.target.value })}
                  className="mt-1 w-full rounded-xl border border-line px-3 py-2.5 font-normal"
                  placeholder="Ví dụ: Quản lý kho"
                />
              </label>
              <label className="text-sm font-medium text-ink">
                Mô tả
                <textarea
                  maxLength={500}
                  rows={4}
                  value={form.description}
                  onChange={(event) => setForm({ ...form, description: event.target.value })}
                  className="mt-1 w-full rounded-xl border border-line px-3 py-2.5 font-normal"
                  placeholder="Mô tả ngắn về vai trò"
                />
              </label>
            </div>
            <div className="flex gap-2 border-t border-line px-5 py-4">
              <button type="submit" disabled={saving} className="rounded-xl bg-ink px-4 py-2.5 text-sm text-white disabled:opacity-60">
                {saving ? "Đang lưu..." : "Lưu"}
              </button>
              <button type="button" onClick={closeForm} className="rounded-xl border border-line px-4 py-2.5 text-sm">
                Hủy
              </button>
            </div>
          </form>
        </div>
      ) : null}

      {pendingDelete ? (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-ink/30 px-4">
          <div className="w-full max-w-sm rounded-2xl bg-white p-5 shadow-xl">
            <h2 className="font-semibold text-ink">Xóa vai trò</h2>
            <p className="mt-2 text-sm text-stone-500">Xóa “{pendingDelete.name}”? Thao tác này không hoàn tác.</p>
            <div className="mt-5 flex justify-end gap-2">
              <button type="button" className="rounded-xl border border-line px-3 py-2 text-sm" onClick={() => setPendingDelete(null)}>
                Hủy
              </button>
              <button type="button" className="rounded-xl bg-red-600 px-3 py-2 text-sm text-white" onClick={() => void confirmDelete()}>
                Xóa
              </button>
            </div>
          </div>
        </div>
      ) : null}
    </div>
  );
}
