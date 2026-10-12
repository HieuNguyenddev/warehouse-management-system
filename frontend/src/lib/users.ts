import { api } from "@/lib/api";

export interface User {
  id: string;
  fullName: string;
  email: string;
  phone: string | null;
  roleId: string | null;
  roleName: string | null;
  isActive: boolean;
  hasPassword: boolean;
}

export interface PagedUsers {
  items: User[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface UserInput {
  fullName: string;
  email: string;
  phone: string;
  roleId: string;
  isActive: boolean;
  password: string;
}

export function searchUsers(search: string, page: number, pageSize: number) {
  const params = new URLSearchParams({
    page: String(page),
    pageSize: String(pageSize),
  });
  const keyword = search.trim();
  if (keyword) params.set("search", keyword);
  return api<PagedUsers>(`/api/users?${params}`);
}

export function createUser(input: UserInput) {
  return api<User>("/api/users", {
    method: "POST",
    body: JSON.stringify(toBody(input)),
  });
}

export function updateUser(id: string, input: UserInput) {
  return api<User>(`/api/users/${id}`, {
    method: "PUT",
    body: JSON.stringify(toBody(input)),
  });
}

export function getUserPassword(id: string) {
  return api<{ password: string | null }>(`/api/users/${id}/password`);
}

export function deleteUser(id: string) {
  return api<void>(`/api/users/${id}`, { method: "DELETE" });
}

function toBody(input: UserInput) {
  const phone = input.phone.trim();
  return {
    fullName: input.fullName.trim(),
    email: input.email.trim(),
    phone: phone.length === 0 ? null : phone,
    roleId: input.roleId || null,
    isActive: input.isActive,
    password: input.password.trim() || null,
  };
}
