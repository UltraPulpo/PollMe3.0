import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { PollCard } from '../components/PollCard';
import * as pollsApi from '../api/pollsApi';
import type { PollSummaryDto } from '../types';

export function DashboardPage() {
    const [polls, setPolls] = useState<PollSummaryDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        pollsApi.getPolls()
            .then(setPolls)
            .catch(err => setError(err instanceof Error ? err.message : 'Failed to load polls'))
            .finally(() => setLoading(false));
    }, []);

    if (loading) return <main><p>Loading...</p></main>;
    if (error) return <main><p role="alert">{error}</p></main>;

    return (
        <main>
            <h1>My Polls</h1>
            <Link to="/polls/new">Create New Poll</Link>
            {polls.length === 0
                ? <p>No polls yet. Create your first poll!</p>
                : polls.map(poll => <PollCard key={poll.id} poll={poll} />)
            }
        </main>
    );
}
