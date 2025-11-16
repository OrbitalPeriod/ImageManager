import type { GetUserInfoResponse, LoginRequest, RegisterRequest } from "../types/auth";
import { apiFetch } from "./api";



export async function registerUser(payload: RegisterRequest): Promise<void> {
    const res = await apiFetch("/api/auth/register", {
        method: 'POST',
        body: JSON.stringify(payload),
    });


  if (!res.ok) {

    const errorText = await res.text();
    throw new Error(
      `Registration failed (${res.status} ${res.statusText}): ${errorText}`
    );
  }
}

export async function login(req: LoginRequest): Promise<void> {
  const res = await apiFetch('/api/auth/login', {
    method: 'POST',
    body: JSON.stringify(req),
  });

  if (!res.ok) {
    const errText = await res.text();
    throw new Error(`Login failed (${res.status} ${res.statusText}): ${errText}`);
  }
}

export async function logout(): Promise<void> {
  const res = await apiFetch('/api/auth/logout', { method: 'GET' });

  if (!res.ok) {
    const errText = await res.text();
    throw new Error(`Logout failed (${res.status} ${res.statusText}): ${errText}`);
  }
}

export async function getCurrentUser(): Promise<GetUserInfoResponse> {
  const res = await apiFetch('/api/users/me', { method: 'GET' });

  if (!res.ok) {
    const errText = await res.text();
    throw new Error(`Failed to fetch user (${res.status} ${res.statusText}): ${errText}`);
  }

  return res.json() as Promise<GetUserInfoResponse>;
}