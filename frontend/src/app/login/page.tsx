"use client";

import { Boxes } from "lucide-react";
import { useRouter } from "next/navigation";
import { FormEvent, useEffect, useState } from "react";
import { getToken } from "@/lib/api";
import { login } from "@/lib/auth";

export default function LoginPage() {
  const router = useRouter();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    if (getToken()) router.replace("/");
  }, [router]);

  async function submit(event: FormEvent) {
    event.preventDefault();
    setSubmitting(true);
    setError(null);
    try {
      await login(email, password);
      router.replace("/");
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : "Không đăng nhập được.");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <main className="flex min-h-screen items-center justify-center px-4">
      <form onSubmit={(event) => void submit(event)} className="w-full max-w-sm rounded-2xl border border-line bg-white p-6 shadow-sm">
        <div className="flex items-center gap-3">
          <span className="flex size-10 items-center justify-center rounded-xl bg-ink text-brass">
            <Boxes size={20} />
          </span>
          <div>
            <h1 className="text-lg font-semibold text-ink">Đăng nhập</h1>
            <p className="text-sm text-stone-500">Quản lý kho</p>
          </div>
        </div>

        {error ? <p className="mt-5 rounded-xl bg-red-50 px-3 py-2 text-sm text-red-700">{error}</p> : null}

        <label className="mt-5 block text-sm font-medium text-ink">
          Email
          <input
            required
            type="email"
            autoComplete="username"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
            className="mt-1 w-full rounded-xl border border-line px-3 py-2.5 font-normal"
          />
        </label>
        <label className="mt-4 block text-sm font-medium text-ink">
          Mật khẩu
          <input
            required
            type="password"
            autoComplete="current-password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            className="mt-1 w-full rounded-xl border border-line px-3 py-2.5 font-normal"
          />
        </label>
        <button
          type="submit"
          disabled={submitting}
          className="mt-5 w-full rounded-xl bg-ink px-4 py-2.5 text-sm font-medium text-white disabled:opacity-60"
        >
          {submitting ? "Đang đăng nhập..." : "Đăng nhập"}
        </button>
      </form>
    </main>
  );
}
