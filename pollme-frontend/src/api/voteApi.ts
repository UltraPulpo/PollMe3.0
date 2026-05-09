import type { TallyDto } from '../types';

const BASE = '/api/polls';

export async function submitVote(
  slug: string,
  selectedOptionIds: number[]
): Promise<TallyDto | { message: string }> {
  const res = await fetch(`${BASE}/${slug}/votes`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    credentials: 'include',
    body: JSON.stringify({ selectedOptionIds }),
  });
  if (!res.ok) throw new Error(await res.text());
  return res.json() as Promise<TallyDto | { message: string }>;
}
