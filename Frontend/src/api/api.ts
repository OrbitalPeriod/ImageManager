export const baseUrl = import.meta.env.VITE_API_URL ?? window.location.origin;

export async function apiFetch(
  input: RequestInfo,
  init?: RequestInit
): Promise<Response> {
  const url = typeof input === "string"
    ? new URL(input, baseUrl).toString()
    : input;         

  const opts: RequestInit = {
    credentials: 'include',            
    headers: { 'Content-Type': 'application/json' },
    ...init,
  };

  return fetch(url, opts);
}