import { api, clearSession, saveSession, SessionUser } from "@/lib/api";

export interface LoginResult {
  token: string;
  expiresAt: string;
  user: SessionUser;
}

export async function login(email: string, password: string) {
  const result = await api<LoginResult>("/api/auth/login", {
    method: "POST",
    body: JSON.stringify({ email: email.trim(), password }),
  });
  saveSession(result.token, result.user);
  return result.user;
}

export function logout() {
  clearSession();
}
