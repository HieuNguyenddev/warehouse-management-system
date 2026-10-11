"use client";

import { AdminShell } from "@/components/admin-shell";
import { getToken } from "@/lib/api";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";

export default function AdminLayout({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const [ready, setReady] = useState(false);

  useEffect(() => {
    if (!getToken()) {
      router.replace("/login");
      return;
    }
    setReady(true);
  }, [router]);

  return (
    <>
      {ready ? null : <p className="px-6 py-10 text-sm text-stone-500">Đang kiểm tra phiên đăng nhập...</p>}
      <div hidden={!ready}>
        <AdminShell>{children}</AdminShell>
      </div>
    </>
  );
}
