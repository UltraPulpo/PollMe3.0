import type { TallyDto } from '../types';

const BASE = '/api/polls';

async function request<T>(url: string, options?: RequestInit): Promise<T> {
  const res = await fetch(url, { credentials: 'include', ...options });
  if (!res.ok) throw new Error(await res.text());
  return res.json() as Promise<T>;
}

export function getResults(slug: string): Promise<TallyDto> {
  return request<TallyDto>(`${BASE}/${slug}/results`);
}
