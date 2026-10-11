"use client";

import { Boxes, LayoutDashboard, Menu, PanelLeftClose, PanelLeftOpen, Search, UserRound, X } from "lucide-react";
import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { FormEvent, useEffect, useState } from "react";

const links = [
  { href: "/", label: "Tổng quan", icon: LayoutDashboard },
  { href: "/roles", label: "Vai trò", icon: UserRound },
];

export function AdminShell({ children }: { children: React.ReactNode }) {
  const pathname = usePathname();
  const router = useRouter();
  const [open, setOpen] = useState(false);
  const [collapsed, setCollapsed] = useState(false);
  const [keyword, setKeyword] = useState("");

  useEffect(() => {
    setCollapsed(localStorage.getItem("sidebar-collapsed") === "1");
  }, []);

  function toggleCollapsed() {
    setCollapsed((current) => {
      localStorage.setItem("sidebar-collapsed", current ? "0" : "1");
      return !current;
    });
  }

  function search(event: FormEvent) {
    event.preventDefault();
    const text = keyword.trim();
    router.push(text ? `/roles?q=${encodeURIComponent(text)}` : "/roles");
    setOpen(false);
  }

  return (
    <div className="min-h-screen">
      {open ? (
        <button
          type="button"
          className="fixed inset-0 z-30 bg-ink/40 lg:hidden"
          aria-label="Đóng menu"
          onClick={() => setOpen(false)}
        />
      ) : null}

      <aside
        className={`fixed inset-y-0 left-0 z-40 flex flex-col bg-ink text-stone-100 transition-[width,transform] duration-200 ${
          collapsed ? "w-[4.5rem]" : "w-64"
        } ${open ? "translate-x-0" : "-translate-x-full"} lg:translate-x-0`}
      >
        <div className={`flex items-center py-5 ${collapsed ? "justify-center px-2" : "gap-3 px-5"}`}>
          <span className="flex size-10 shrink-0 items-center justify-center rounded-xl bg-brass text-ink">
            <Boxes size={20} />
          </span>
          {collapsed ? null : (
            <div className="min-w-0">
              <p className="truncate text-sm font-semibold tracking-wide">Quản lý kho</p>
              <p className="truncate text-xs text-stone-400">Vai trò và phân quyền</p>
            </div>
          )}
        </div>

        <nav className="flex flex-1 flex-col gap-1 px-2">
          {links.map((link) => {
            const active = link.href === "/" ? pathname === "/" : pathname.startsWith(link.href);
            const Icon = link.icon;
            return (
              <Link
                key={link.href}
                href={link.href}
                title={link.label}
                onClick={() => setOpen(false)}
                className={`flex items-center rounded-xl py-2.5 text-sm ${
                  collapsed ? "justify-center px-2" : "gap-3 px-3"
                } ${active ? "bg-white/10 text-white" : "text-stone-300 hover:bg-white/5"}`}
              >
                <Icon size={18} />
                {collapsed ? null : link.label}
              </Link>
            );
          })}
        </nav>

        <div className={`py-4 ${collapsed ? "px-2" : "px-3"}`}>
          <button
            type="button"
            onClick={toggleCollapsed}
            title={collapsed ? "Phóng to sidebar" : "Thu gọn sidebar"}
            aria-label={collapsed ? "Phóng to sidebar" : "Thu gọn sidebar"}
            className={`hidden w-full items-center rounded-xl py-2.5 text-sm text-stone-300 hover:bg-white/5 lg:flex ${
              collapsed ? "justify-center px-2" : "gap-3 px-3"
            }`}
          >
            {collapsed ? <PanelLeftOpen size={18} /> : <PanelLeftClose size={18} />}
            {collapsed ? null : "Thu gọn"}
          </button>
          {collapsed ? null : <p className="px-3 pt-3 text-xs text-stone-500">Kho vận nội bộ</p>}
        </div>
      </aside>

      <div className={`transition-[padding] duration-200 ${collapsed ? "lg:pl-[4.5rem]" : "lg:pl-64"}`}>
        <header className="sticky top-0 z-20 flex items-center gap-3 border-b border-line bg-paper/90 px-4 py-3 backdrop-blur">
          <button
            type="button"
            className="rounded-lg p-2 text-ink hover:bg-white lg:hidden"
            aria-label={open ? "Đóng menu" : "Mở menu"}
            onClick={() => setOpen((current) => !current)}
          >
            {open ? <X size={18} /> : <Menu size={18} />}
          </button>
          <button
            type="button"
            className="hidden rounded-lg p-2 text-ink hover:bg-white lg:inline-flex"
            aria-label={collapsed ? "Phóng to sidebar" : "Thu gọn sidebar"}
            title={collapsed ? "Phóng to sidebar" : "Thu gọn sidebar"}
            onClick={toggleCollapsed}
          >
            {collapsed ? <PanelLeftOpen size={18} /> : <PanelLeftClose size={18} />}
          </button>
          <p className="hidden text-sm text-stone-500 sm:block">Hệ thống quản lý kho</p>
          <form onSubmit={search} className="ml-auto w-full max-w-xs">
            <label className="relative block">
              <Search size={15} className="absolute top-1/2 left-3 -translate-y-1/2 text-stone-400" />
              <input
                value={keyword}
                onChange={(event) => setKeyword(event.target.value)}
                placeholder="Tìm vai trò"
                className="w-full rounded-xl border border-line bg-white py-2 pr-3 pl-9 text-sm shadow-sm"
              />
            </label>
          </form>
        </header>
        <main className="px-4 py-6 sm:px-6">{children}</main>
      </div>
    </div>
  );
}
