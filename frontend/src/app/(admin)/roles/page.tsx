import { Suspense } from "react";
import { RoleManager } from "@/components/role-manager";

export default function RolesPage() {
  return (
    <Suspense fallback={<p className="text-sm text-stone-500">Đang tải vai trò...</p>}>
      <RoleManager />
    </Suspense>
  );
}
