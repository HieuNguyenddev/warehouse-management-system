"use client";

import { ArrowRight, Search, Shield, Users } from "lucide-react";
import Link from "next/link";
import { useEffect, useState } from "react";
import { useToast } from "@/components/toast-provider";
import { Role, searchRoles } from "@/lib/roles";
import { searchUsers } from "@/lib/users";

export function Dashboard() {
  const notify = useToast();
  const [total, setTotal] = useState<number | null>(null);
  const [userTotal, setUserTotal] = useState<number | null>(null);
  const [recent, setRecent] = useState<Role[]>([]);

  useEffect(() => {
    Promise.all([searchRoles("", 1, 5), searchUsers("", 1, 1)])
      .then(([roles, users]) => {
        setTotal(roles.totalCount);
        setRecent(roles.items);
        setUserTotal(users.totalCount);
      })
      .catch((reason: Error) => notify(reason.message, "error"));
  }, [notify]);

  return (
    <div className="mx-auto flex max-w-6xl flex-col gap-6">
      <div>
        <h1 className="text-2xl font-semibold text-ink">Tổng quan</h1>
        <p className="mt-1 text-sm text-stone-500">Theo dõi vai trò đang dùng trong kho.</p>
      </div>

      <div className="grid gap-4 md:grid-cols-3">
        <article className="rounded-2xl bg-ink p-5 text-white">
          <Shield size={20} className="text-brass" />
          <p className="mt-6 text-3xl font-semibold">{total ?? "—"}</p>
          <p className="mt-1 text-sm text-stone-300">Vai trò trong hệ thống</p>
        </article>
        <Link href="/roles" className="rounded-2xl border border-line bg-white p-5 hover:border-brass">
          <Search size={20} className="text-brass" />
          <p className="mt-6 font-medium text-ink">Tìm gần đúng</p>
          <p className="mt-1 text-sm text-stone-500">Không phân biệt hoa thường và dấu.</p>
        </Link>
        <Link href="/users" className="rounded-2xl border border-line bg-white p-5 hover:border-brass">
          <Users size={20} className="text-brass" />
          <p className="mt-6 text-3xl font-semibold text-ink">{userTotal ?? "—"}</p>
          <p className="mt-1 text-sm text-stone-500">Người dùng</p>
        </Link>
      </div>

      <section className="overflow-hidden rounded-2xl border border-line bg-white">
        <div className="flex items-center justify-between border-b border-line px-5 py-4">
          <h2 className="font-medium text-ink">Vai trò gần đây</h2>
          <Link href="/roles" className="inline-flex items-center gap-1 text-sm text-brass">
            Xem tất cả <ArrowRight size={14} />
          </Link>
        </div>
        {recent.length === 0 ? (
          <p className="px-5 py-10 text-center text-sm text-stone-500">Chưa có vai trò nào.</p>
        ) : (
          <ul>
            {recent.map((role) => (
              <li key={role.id} className="flex items-center gap-3 border-b border-line px-5 py-3 last:border-0">
                <span className="flex size-9 items-center justify-center rounded-full bg-brass-soft text-sm font-semibold text-ink">
                  {role.name.trim().charAt(0).toUpperCase()}
                </span>
                <div>
                  <p className="font-medium text-ink">{role.name}</p>
                  <p className="text-sm text-stone-500">{role.description || "Chưa có mô tả"}</p>
                </div>
              </li>
            ))}
          </ul>
        )}
      </section>
    </div>
  );
}
