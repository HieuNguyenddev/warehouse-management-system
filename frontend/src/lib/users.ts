export interface User {
  id: string;
  fullName: string;
  email: string;
  phone: string | null;
  roleId: string | null;
  roleName: string | null;
  isActive: boolean;
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
}

export function searchUsers(search: string, page: number, pageSize: number) {
  const params = new URLSearchParams({
    page: String(page),
    pageSize: String(pageSize),
  });
  const keyword = search.trim();
  if (keyword) params.set("search", keyword);
  return request<PagedUsers>(`/api/users?${params}`);
}

export function createUser(input: UserInput) {
  return request<User>("/api/users", {
    method: "POST",
    body: JSON.stringify(toBody(input)),
  });
}

export function updateUser(id: string, input: UserInput) {
  return request<User>(`/api/users/${id}`, {
    method: "PUT",
    body: JSON.stringify(toBody(input)),
  });
}

export function deleteUser(id: string) {
  return request<void>(`/api/users/${id}`, { method: "DELETE" });
}

function toBody(input: UserInput) {
  const phone = input.phone.trim();
  return {
    fullName: input.fullName.trim(),
    email: input.email.trim(),
    phone: phone.length === 0 ? null : phone,
    roleId: input.roleId || null,
    isActive: input.isActive,
  };
}

async function request<T>(url: string, init?: RequestInit): Promise<T> {
  const response = await fetch(url, {
    ...init,
    headers: {
      "Content-Type": "application/json",
      ...init?.headers,
    },
  });

  if (!response.ok) {
    const body = (await response.json().catch(() => null)) as { message?: string } | null;
    throw new Error(body?.message ?? "Không kết nối được API.");
  }

  if (response.status === 204) return undefined as T;
  return (await response.json()) as T;
}
