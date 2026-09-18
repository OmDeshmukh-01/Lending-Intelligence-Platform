/**
 * API service layer.
 * All HTTP calls live here; components never call fetch() directly.
 * The Vite proxy forwards /api/* to the .NET backend at localhost:5114.
 */

const BASE = "/api";

async function request(method, path, body) {
  const res = await fetch(`${BASE}${path}`, {
    method,
    headers: body ? { "Content-Type": "application/json" } : {},
    body: body ? JSON.stringify(body) : undefined,
  });

  const contentType = res.headers.get("content-type") || "";
  const isJson = contentType.includes("application/json");
  const data = isJson ? await res.json() : await res.text();

  if (!res.ok) {
    // Throw an error object that components can inspect.
    const error = new Error(isJson ? (data.message || "Request failed") : data);
    error.status = res.status;
    error.data = data;
    throw error;
  }

  return data;
}

/** Submit a new loan application. */
export async function applyForLoan(payload) {
  return request("POST", "/loans/apply", payload);
}

/** Fetch paginated application list with optional filters. */
export async function getApplications({ decision, from, to, page = 1, pageSize = 20 } = {}) {
  const params = new URLSearchParams();
  if (decision) params.set("decision", decision);
  if (from) params.set("from", from);
  if (to) params.set("to", to);
  params.set("page", String(page));
  params.set("pageSize", String(pageSize));
  return request("GET", `/loans?${params}`);
}

/** Fetch a single application by ID (includes full rule evaluations). */
export async function getApplicationById(id) {
  return request("GET", `/loans/${id}`);
}

/** Fetch dashboard statistics and recent applications. */
export async function getDashboard() {
  return request("GET", "/dashboard");
}
