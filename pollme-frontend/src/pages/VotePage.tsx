import { useEffect, useState } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { VoteForm } from '../components/VoteForm';
import { useVoteStatus } from '../hooks/useVoteStatus';
import * as pollsApi from '../api/pollsApi';
import * as voteApi from '../api/voteApi';
import type { PollVoteDto } from '../types';

export function VotePage() {
    const { slug } = useParams<{ slug: string }>();
    const navigate = useNavigate();
    const { hasVoted, markVoted } = useVoteStatus(slug!);
    const [poll, setPoll] = useState<PollVoteDto | null>(null);
    const [submitted, setSubmitted] = useState(false);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        pollsApi.getPollBySlug(slug!)
            .then(setPoll)
            .catch(err => setError(err instanceof Error ? err.message : 'Failed to load poll'));
    }, [slug]);

    const handleSubmit = async (selectedIds: number[]) => {
        setIsSubmitting(true);
        try {
            const response = await voteApi.submitVote(slug!, selectedIds);
            markVoted();
            if ('options' in response) {
                navigate(`/results/${slug}`);
            } else {
                setSubmitted(true);
            }
        } finally {
            setIsSubmitting(false);
        }
    };

    if (hasVoted() || submitted) {
        return (
            <main>
                <article>
                    <p>Thank you for voting!</p>
                    <Link to={`/results/${slug}`}>View Results</Link>
                </article>
            </main>
        );
    }

    if (error) return <main><p role="alert">{error}</p></main>;
    if (!poll) return <main><p>Loading...</p></main>;

    return (
        <main>
            <article>
                <h1>{poll.question}</h1>
                <VoteForm poll={poll} onSubmit={handleSubmit} isSubmitting={isSubmitting} />
            </article>
        </main>
    );
}
