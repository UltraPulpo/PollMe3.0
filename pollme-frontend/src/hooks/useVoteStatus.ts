import { hasVoted, markVoted } from '../utils/voteStorage';

export function useVoteStatus(slug: string) {
    return {
        hasVoted: () => hasVoted(slug),
        markVoted: () => markVoted(slug),
    };
}
