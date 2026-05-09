import type { Creator } from '../types';

const BASE = '/api/auth';

async function request<T>(url: string, options?: RequestInit): Promise<T> {
  const res = await fetch(url, { credentials: 'include', ...options });
  if (!res.ok) throw new Error(await res.text());
  return res.json() as Promise<T>;
}

export function register(username: string, password: string): Promise<Creator> {
  return request<Creator>(`${BASE}/register`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ username, password }),
  });
}

export function login(username: string, password: string): Promise<Creator> {
  return request<Creator>(`${BASE}/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ username, password }),
  });
}

export async function logout(): Promise<void> {
  await fetch(`${BASE}/logout`, { method: 'POST', credentials: 'include' });
}

export function me(): Promise<Creator> {
  return request<Creator>(`${BASE}/me`);
}
