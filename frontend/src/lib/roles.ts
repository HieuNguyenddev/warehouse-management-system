export interface Role {
  id: string;
  name: string;
  description: string | null;
}

export interface PagedRoles {
  items: Role[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface RoleInput {
  name: string;
  description: string;
}

export async function searchRoles(search: string, page: number, pageSize: number): Promise<PagedRoles> {
  const params = new URLSearchParams({
    page: String(page),
    pageSize: String(pageSize),
  });
  const keyword = search.trim();
  if (keyword) params.set("search", keyword);
  return request<PagedRoles>(`/api/roles?${params}`);
}

export function createRole(input: RoleInput) {
  return request<Role>("/api/roles", {
    method: "POST",
    body: JSON.stringify(toBody(input)),
  });
}

export function updateRole(id: string, input: RoleInput) {
  return request<Role>(`/api/roles/${id}`, {
    method: "PUT",
    body: JSON.stringify(toBody(input)),
  });
}

export function deleteRole(id: string) {
  return request<void>(`/api/roles/${id}`, { method: "DELETE" });
}

function toBody(input: RoleInput) {
  const description = input.description.trim();
  return {
    name: input.name.trim(),
    description: description.length === 0 ? null : description,
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
