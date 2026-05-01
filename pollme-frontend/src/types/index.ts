export type PollMode = 'SingleSelect' | 'MultiSelect';
export type ResultsVisibility = 'Public' | 'CreatorOnly';

export interface Creator {
  id: number;
  username: string;
}

export interface OptionDto {
  id: number;
  text: string;
  position: number;
}

export interface PollVoteDto {
  id: number;
  slug: string;
  question: string;
  options: OptionDto[];
  mode: PollMode;
}

export interface PollSummaryDto {
  id: number;
  slug: string;
  question: string;
  totalVotes: number;
  leadingOptionText: string | null;
  leadingPercentage: number;
  createdAt: string;
}

export interface OptionTallyDto {
  optionId: number;
  text: string;
  votes: number;
  percentage: number;
}

export interface TallyDto {
  pollId: number;
  options: OptionTallyDto[];
}

export interface CreatePollResponseDto {
  slug: string;
  voteLink: string;
}

export interface RegisterRequest {
  username: string;
  password: string;
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface CreatePollRequest {
  question: string;
  options: string[];
  mode: PollMode;
  visibility: ResultsVisibility;
}

export interface VoteRequest {
  selectedOptionIds: number[];
}
