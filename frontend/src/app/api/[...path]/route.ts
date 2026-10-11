import { connection, NextRequest, NextResponse } from "next/server";

const hopByHop = ["connection", "keep-alive", "transfer-encoding", "content-length", "host"];

async function proxy(request: NextRequest, context: { params: Promise<{ path: string[] }> }) {
  await connection();

  const { path } = await context.params;
  const base = (process.env.API_URL ?? "http://localhost:5161").replace(/\/$/, "");
  const target = new URL(`${base}/api/${path.join("/")}`);
  target.search = request.nextUrl.search;

  const headers = new Headers(request.headers);
  for (const name of hopByHop) headers.delete(name);

  try {
    const response = await fetch(target, {
      method: request.method,
      headers,
      body: request.method === "GET" || request.method === "HEAD" ? undefined : await request.arrayBuffer(),
      redirect: "manual",
    });

    const outHeaders = new Headers(response.headers);
    for (const name of hopByHop) outHeaders.delete(name);

    return new NextResponse(response.body, {
      status: response.status,
      headers: outHeaders,
    });
  } catch {
    return NextResponse.json({ message: "Không kết nối được API." }, { status: 502 });
  }
}

export const GET = proxy;
export const POST = proxy;
export const PUT = proxy;
export const PATCH = proxy;
export const DELETE = proxy;
