import { api } from "@/lib/api";

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
  return api<PagedRoles>(`/api/roles?${params}`);
}

export function createRole(input: RoleInput) {
  return api<Role>("/api/roles", {
    method: "POST",
    body: JSON.stringify(toBody(input)),
  });
}

export function updateRole(id: string, input: RoleInput) {
  return api<Role>(`/api/roles/${id}`, {
    method: "PUT",
    body: JSON.stringify(toBody(input)),
  });
}

export function deleteRole(id: string) {
  return api<void>(`/api/roles/${id}`, { method: "DELETE" });
}

function toBody(input: RoleInput) {
  const description = input.description.trim();
  return {
    name: input.name.trim(),
    description: description.length === 0 ? null : description,
  };
}
