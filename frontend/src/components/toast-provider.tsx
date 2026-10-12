"use client";

import { X } from "lucide-react";
import { createContext, useCallback, useContext, useRef, useState } from "react";

type ToastTone = "success" | "error";

type ToastItem = {
  id: number;
  message: string;
  tone: ToastTone;
};

const ToastContext = createContext<(message: string, tone?: ToastTone) => void>(() => {});

export function useToast() {
  return useContext(ToastContext);
}

export function ToastProvider({ children }: { children: React.ReactNode }) {
  const [toasts, setToasts] = useState<ToastItem[]>([]);
  const nextId = useRef(0);

  const dismiss = useCallback((id: number) => {
    setToasts((current) => current.filter((item) => item.id !== id));
  }, []);

  const notify = useCallback(
    (message: string, tone: ToastTone = "success") => {
      const id = nextId.current + 1;
      nextId.current = id;
      setToasts((current) => [...current, { id, message, tone }]);
      window.setTimeout(() => dismiss(id), 3600);
    },
    [dismiss],
  );

  return (
    <ToastContext.Provider value={notify}>
      {children}
      <div className="pointer-events-none fixed top-4 right-4 z-[80] flex w-[min(100%-2rem,22rem)] flex-col gap-2">
        {toasts.map((toast) => (
          <div
            key={toast.id}
            role="status"
            className={`pointer-events-auto flex items-start gap-3 rounded-2xl border px-4 py-3 text-sm shadow-lg ${
              toast.tone === "error" ? "border-red-200 bg-red-50 text-red-700" : "border-line bg-white text-ink"
            }`}
          >
            <p className="flex-1">{toast.message}</p>
            <button type="button" aria-label="Đóng thông báo" className="shrink-0" onClick={() => dismiss(toast.id)}>
              <X size={16} />
            </button>
          </div>
        ))}
      </div>
    </ToastContext.Provider>
  );
}
