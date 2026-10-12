"use client";

import { Eye, EyeOff, Pencil, Plus, Search, Trash2, X } from "lucide-react";
import { useSearchParams } from "next/navigation";
import { useEffect, useState } from "react";
import { useToast } from "@/components/toast-provider";
import { getSession } from "@/lib/api";
import { Role, searchRoles } from "@/lib/roles";
import { User, UserInput, createUser, deleteUser, getUserPassword, searchUsers, updateUser } from "@/lib/users";

const emptyForm: UserInput = { fullName: "", email: "", phone: "", roleId: "", isActive: true, password: "" };

export function UserManager() {
  const notify = useToast();
  const query = useSearchParams().get("q") ?? "";
  const [keyword, setKeyword] = useState(query);
  const [applied, setApplied] = useState(query);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [items, setItems] = useState<User[]>([]);
  const [roles, setRoles] = useState<Role[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(0);
  const [loading, setLoading] = useState(true);
  const [editing, setEditing] = useState<User | null>(null);
  const [creating, setCreating] = useState(false);
  const [form, setForm] = useState<UserInput>(emptyForm);
  const [formError, setFormError] = useState<string | null>(null);
  const [showPassword, setShowPassword] = useState(false);
  const [nextPassword, setNextPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [showNextPassword, setShowNextPassword] = useState(false);
  const [isAdmin, setIsAdmin] = useState(false);
  const [currentPassword, setCurrentPassword] = useState<string | null>(null);
  const [showCurrentPassword, setShowCurrentPassword] = useState(false);
  const [saving, setSaving] = useState(false);
  const [pendingDelete, setPendingDelete] = useState<User | null>(null);

  useEffect(() => {
    setIsAdmin((getSession()?.roleName ?? "").toLowerCase() === "admin");
    searchRoles("", 1, 100)
      .then((result) => setRoles(result.items))
      .catch(() => setRoles([]));
  }, []);

  useEffect(() => {
    setKeyword(query);
    setApplied(query);
    setPage(1);
    setLoading(true);
  }, [query]);

  useEffect(() => {
    let active = true;
    searchUsers(applied, page, pageSize)
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
    setShowPassword(false);
    resetPasswordChange();
  }

  function openEdit(user: User) {
    setCreating(false);
    setEditing(user);
    setForm({
      fullName: user.fullName,
      email: user.email,
      phone: user.phone ?? "",
      roleId: user.roleId ?? "",
      isActive: user.isActive,
      password: "",
    });
    setFormError(null);
    setShowPassword(false);
    resetPasswordChange();
    void loadCurrentPassword(user.id);
  }

  async function loadCurrentPassword(id: string) {
    if ((getSession()?.roleName ?? "").toLowerCase() !== "admin") {
      setCurrentPassword(null);
      return;
    }

    try {
      const result = await getUserPassword(id);
      setCurrentPassword(result.password);
      setShowCurrentPassword(false);
    } catch (reason) {
      setCurrentPassword(null);
      notify(reason instanceof Error ? reason.message : "Không xem được mật khẩu.", "error");
    }
  }

  function resetPasswordChange() {
    setNextPassword("");
    setConfirmPassword("");
    setShowNextPassword(false);
  }

  function closeForm() {
    setCreating(false);
    setEditing(null);
    setFormError(null);
    setShowPassword(false);
    setCurrentPassword(null);
    setShowCurrentPassword(false);
    resetPasswordChange();
  }

  async function reload(nextPage = page) {
    const result = await searchUsers(applied, nextPage, pageSize);
    setItems(result.items);
    setTotalCount(result.totalCount);
    setTotalPages(result.totalPages);
  }

  async function save() {
    setSaving(true);
    setFormError(null);
    try {
      if (editing) {
        await updateUser(editing.id, form);
        notify("Đã cập nhật người dùng.");
      } else {
        await createUser(form);
        notify("Đã thêm người dùng.");
      }
      closeForm();
      setLoading(true);
      await reload();
    } catch (reason) {
      const message = reason instanceof Error ? reason.message : "Không lưu được.";
      setFormError(message);
      notify(message, "error");
    } finally {
      setSaving(false);
      setLoading(false);
    }
  }

  async function changePassword() {
    if (!editing) return;
    if (nextPassword.length < 8 || nextPassword.length > 100) {
      notify("Mật khẩu từ 8 đến 100 ký tự.", "error");
      return;
    }
    if (nextPassword !== confirmPassword) {
      notify("Mật khẩu nhập lại chưa khớp.", "error");
      return;
    }

    setSaving(true);
    try {
      const updated = await updateUser(editing.id, { ...form, password: nextPassword });
      setEditing({ ...editing, ...updated, hasPassword: true });
      setCurrentPassword(nextPassword);
      setShowCurrentPassword(true);
      resetPasswordChange();
      notify("Đã đặt lại mật khẩu.");
      setLoading(true);
      await reload();
    } catch (reason) {
      notify(reason instanceof Error ? reason.message : "Không đổi được mật khẩu.", "error");
    } finally {
      setSaving(false);
      setLoading(false);
    }
  }

  async function confirmDelete() {
    if (!pendingDelete) return;
    try {
      await deleteUser(pendingDelete.id);
      const nextPage = items.length === 1 && page > 1 ? page - 1 : page;
      setPage(nextPage);
      notify("Đã xóa người dùng.");
      setPendingDelete(null);
      setLoading(true);
      await reload(nextPage);
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
          <h1 className="text-2xl font-semibold text-ink">Người dùng</h1>
          <p className="mt-1 text-sm text-stone-500">{totalCount} người dùng</p>
        </div>
        <button type="button" onClick={openCreate} className="inline-flex items-center gap-2 rounded-xl bg-ink px-4 py-2.5 text-sm font-medium text-white">
          <Plus size={16} /> Thêm người dùng
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
            placeholder="Tìm theo tên, email, số điện thoại hoặc vai trò"
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
              <th className="px-5 py-3 font-medium">Họ tên</th>
              <th className="px-5 py-3 font-medium">Liên hệ</th>
              <th className="px-5 py-3 font-medium">Vai trò</th>
              <th className="px-5 py-3 font-medium">Trạng thái</th>
              <th className="px-5 py-3 text-right font-medium">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            {loading ? (
              <tr>
                <td colSpan={5} className="px-5 py-12 text-center text-stone-500">
                  Đang tải...
                </td>
              </tr>
            ) : items.length === 0 ? (
              <tr>
                <td colSpan={5} className="px-5 py-12 text-center text-stone-500">
                  Không có người dùng khớp bộ lọc.
                </td>
              </tr>
            ) : (
              items.map((user) => (
                <tr key={user.id} className="border-t border-line transition-colors hover:bg-paper">
                  <td className="px-5 py-3">
                    <div className="flex items-center gap-3">
                      <span className="flex size-8 items-center justify-center rounded-full bg-brass-soft text-xs font-semibold text-ink">
                        {user.fullName.trim().charAt(0).toUpperCase()}
                      </span>
                      <span className="font-medium text-ink">{user.fullName}</span>
                    </div>
                  </td>
                  <td className="px-5 py-3 text-stone-500">
                    <div>{user.email}</div>
                    <div className="text-xs">{user.phone || "Chưa có số điện thoại"}</div>
                  </td>
                  <td className="px-5 py-3 text-stone-600">{user.roleName || "Chưa gán"}</td>
                  <td className="px-5 py-3">
                    <span className={`rounded-full px-2.5 py-1 text-xs font-medium ${user.isActive ? "bg-emerald-50 text-emerald-700" : "bg-stone-100 text-stone-500"}`}>
                      {user.isActive ? "Đang hoạt động" : "Đã khóa"}
                    </span>
                  </td>
                  <td className="px-5 py-3 text-right">
                    <button type="button" aria-label="Sửa" className="rounded-lg p-2 text-stone-500 hover:bg-stone-100" onClick={() => openEdit(user)}>
                      <Pencil size={16} />
                    </button>
                    <button type="button" aria-label="Xóa" className="rounded-lg p-2 text-red-600 hover:bg-red-50" onClick={() => setPendingDelete(user)}>
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
              <h2 className="font-semibold text-ink">{editing ? "Sửa người dùng" : "Thêm người dùng"}</h2>
              <button type="button" aria-label="Đóng" onClick={closeForm}>
                <X size={18} />
              </button>
            </div>
            <div className="flex flex-1 flex-col gap-4 overflow-auto px-5 py-5">
              {formError ? <p className="rounded-xl bg-red-50 px-3 py-2 text-sm text-red-700">{formError}</p> : null}
              <label className="text-sm font-medium text-ink">
                Họ tên
                <input
                  required
                  maxLength={150}
                  value={form.fullName}
                  onChange={(event) => setForm({ ...form, fullName: event.target.value })}
                  className="mt-1 w-full rounded-xl border border-line px-3 py-2.5 font-normal"
                />
              </label>
              <label className="text-sm font-medium text-ink">
                Email
                <input
                  required
                  type="email"
                  maxLength={200}
                  value={form.email}
                  onChange={(event) => setForm({ ...form, email: event.target.value })}
                  className="mt-1 w-full rounded-xl border border-line px-3 py-2.5 font-normal"
                />
              </label>
              {editing && isAdmin ? (
                <div className="rounded-2xl border border-line p-4">
                  <p className="text-sm font-medium text-ink">Mật khẩu hiện tại</p>
                  <div className="mt-2 flex items-center gap-2">
                    <input
                      readOnly
                      type={showCurrentPassword ? "text" : "password"}
                      value={currentPassword ?? ""}
                      placeholder="Chưa có mật khẩu để xem. Hãy đặt lại."
                      className="w-full rounded-xl border border-line bg-paper px-3 py-2.5 font-normal"
                    />
                    <button
                      type="button"
                      disabled={!currentPassword}
                      className="inline-flex items-center gap-1 rounded-xl border border-line px-3 py-2.5 text-sm text-stone-500 disabled:opacity-40"
                      onClick={() => setShowCurrentPassword((current) => !current)}
                    >
                      {showCurrentPassword ? <EyeOff size={16} /> : <Eye size={16} />}
                      {showCurrentPassword ? "Ẩn" : "Xem"}
                    </button>
                  </div>
                  <p className="mt-4 text-sm font-medium text-ink">Đặt lại mật khẩu</p>
                  <label className="mt-2 block text-sm text-stone-500">
                    Mật khẩu mới
                    <span className="relative mt-1 block">
                      <input
                        type={showNextPassword ? "text" : "password"}
                        minLength={nextPassword ? 8 : undefined}
                        maxLength={100}
                        autoComplete="new-password"
                        value={nextPassword}
                        onChange={(event) => setNextPassword(event.target.value)}
                        className="w-full rounded-xl border border-line px-3 py-2.5 pr-11 font-normal text-ink"
                      />
                      <button
                        type="button"
                        aria-label={showNextPassword ? "Ẩn mật khẩu mới" : "Hiện mật khẩu mới"}
                        className="absolute top-1/2 right-3 -translate-y-1/2 text-stone-500"
                        onClick={() => setShowNextPassword((current) => !current)}
                      >
                        {showNextPassword ? <EyeOff size={16} /> : <Eye size={16} />}
                      </button>
                    </span>
                  </label>
                  <label className="mt-3 block text-sm text-stone-500">
                    Nhập lại mật khẩu mới
                    <input
                      type={showNextPassword ? "text" : "password"}
                      maxLength={100}
                      autoComplete="new-password"
                      value={confirmPassword}
                      onChange={(event) => setConfirmPassword(event.target.value)}
                      className="mt-1 w-full rounded-xl border border-line px-3 py-2.5 font-normal text-ink"
                    />
                  </label>
                  <button
                    type="button"
                    disabled={saving}
                    onClick={() => void changePassword()}
                    className="mt-3 rounded-xl border border-ink px-3 py-2 text-sm text-ink disabled:opacity-60"
                  >
                    Đặt lại mật khẩu
                  </button>
                </div>
              ) : (
                <label className="text-sm font-medium text-ink">
                  Mật khẩu
                  <span className="relative mt-1 block">
                    <input
                      required
                      type={showPassword ? "text" : "password"}
                      minLength={8}
                      maxLength={100}
                      autoComplete="new-password"
                      value={form.password}
                      onChange={(event) => setForm({ ...form, password: event.target.value })}
                      className="w-full rounded-xl border border-line px-3 py-2.5 pr-11 font-normal"
                    />
                    <button
                      type="button"
                      aria-label={showPassword ? "Ẩn mật khẩu" : "Hiện mật khẩu"}
                      className="absolute top-1/2 right-3 -translate-y-1/2 text-stone-500"
                      onClick={() => setShowPassword((current) => !current)}
                    >
                      {showPassword ? <EyeOff size={16} /> : <Eye size={16} />}
                    </button>
                  </span>
                </label>
              )}
              <label className="text-sm font-medium text-ink">
                Số điện thoại
                <input
                  maxLength={20}
                  value={form.phone}
                  onChange={(event) => setForm({ ...form, phone: event.target.value })}
                  className="mt-1 w-full rounded-xl border border-line px-3 py-2.5 font-normal"
                />
              </label>
              <label className="text-sm font-medium text-ink">
                Vai trò
                <select
                  value={form.roleId}
                  onChange={(event) => setForm({ ...form, roleId: event.target.value })}
                  className="mt-1 w-full rounded-xl border border-line px-3 py-2.5 font-normal"
                >
                  <option value="">Chưa gán</option>
                  {roles.map((role) => (
                    <option key={role.id} value={role.id}>
                      {role.name}
                    </option>
                  ))}
                </select>
              </label>
              <label className="flex items-center gap-2 text-sm text-ink">
                <input
                  type="checkbox"
                  checked={form.isActive}
                  onChange={(event) => setForm({ ...form, isActive: event.target.checked })}
                />
                Đang hoạt động
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
            <h2 className="font-semibold text-ink">Xóa người dùng</h2>
            <p className="mt-2 text-sm text-stone-500">Xóa “{pendingDelete.fullName}”? Thao tác này không hoàn tác.</p>
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
