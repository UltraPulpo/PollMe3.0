const runtimeExpiry =
  (globalThis as typeof globalThis & { __POLLME_VOTE_TOKEN_EXPIRY_DAYS__?: string | number })
    .__POLLME_VOTE_TOKEN_EXPIRY_DAYS__;
const parsedExpiry = Number.parseInt(String(runtimeExpiry ?? ''), 10);
const VOTE_TOKEN_EXPIRY_DAYS = Number.isFinite(parsedExpiry) && parsedExpiry > 0 ? parsedExpiry : 365;

export function markVoted(slug: string): void {
  localStorage.setItem(`voted_${slug}`, JSON.stringify({ ts: Date.now() }));
}

export function hasVoted(slug: string): boolean {
  try {
    const raw = localStorage.getItem(`voted_${slug}`);
    if (!raw) return false;
    const entry = JSON.parse(raw) as { ts: number };
    if (Date.now() - entry.ts >= VOTE_TOKEN_EXPIRY_DAYS * 86_400_000) return false;
    return true;
  } catch {
    return false;
  }
}
