import type { TallyDto } from '../types';

const BASE = '/api/polls';

export class ApiError extends Error {
  public readonly status: number;

  constructor(status: number, message: string) {
    super(message);
    this.status = status;
    this.name = 'ApiError';
  }
}

async function request<T>(url: string, options?: RequestInit): Promise<T> {
  const res = await fetch(url, { credentials: 'include', ...options });
  if (!res.ok) {
    const text = await res.text();
    let message = text || `Request failed with status ${res.status}`;
    if (text) {
      try {
        const parsed = JSON.parse(text) as { error?: string };
        if (parsed.error) message = parsed.error;
      } catch {
        // Keep raw text response as message
      }
    }
    throw new ApiError(res.status, message);
  }
  return res.json() as Promise<T>;
}

export function getResults(slug: string): Promise<TallyDto> {
  return request<TallyDto>(`${BASE}/${slug}/results`);
}
