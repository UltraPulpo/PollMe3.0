import type { PollSummaryDto, PollVoteDto, CreatePollRequest, CreatePollResponseDto } from '../types';

const BASE = '/api/polls';

async function request<T>(url: string, options?: RequestInit): Promise<T> {
  const res = await fetch(url, { credentials: 'include', ...options });
  if (!res.ok) throw new Error(await res.text());
  return res.json() as Promise<T>;
}

export function getPolls(): Promise<PollSummaryDto[]> {
  return request<PollSummaryDto[]>(BASE);
}

export function createPoll(req: CreatePollRequest): Promise<CreatePollResponseDto> {
  return request<CreatePollResponseDto>(BASE, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(req),
  });
}

export function getPollBySlug(slug: string): Promise<PollVoteDto> {
  return request<PollVoteDto>(`${BASE}/${slug}`);
}
