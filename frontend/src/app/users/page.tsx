import { Suspense } from "react";
import { UserManager } from "@/components/user-manager";

export default function UsersPage() {
  return (
    <Suspense fallback={<p className="text-sm text-stone-500">Đang tải người dùng...</p>}>
      <UserManager />
    </Suspense>
  );
}
