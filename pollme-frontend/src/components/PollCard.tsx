import { Link } from 'react-router-dom';
import type { PollSummaryDto } from '../types';

interface PollCardProps {
    poll: PollSummaryDto;
}

export function PollCard({ poll }: PollCardProps) {
    const handleCopyLink = () => {
        navigator.clipboard.writeText(`${window.location.origin}/vote/${poll.slug}`);
    };

    return (
        <article>
            <h3>{poll.question}</h3>
            <p>{poll.totalVotes} votes</p>
            <p>
                Leading: {poll.leadingOptionText ?? 'No votes yet'}
                {poll.totalVotes > 0 && ` (${poll.leadingPercentage}%)`}
            </p>
            <p>Created: {new Date(poll.createdAt).toLocaleDateString()}</p>
            <Link to={`/results/${poll.slug}`}>View Results</Link>
            <button onClick={handleCopyLink}>Copy Vote Link</button>
        </article>
    );
}
