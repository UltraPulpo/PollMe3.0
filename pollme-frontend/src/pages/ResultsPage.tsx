import { useCallback, useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { ResultsChart } from '../components/ResultsChart';
import { useSignalR } from '../hooks/useSignalR';
import * as resultsApi from '../api/resultsApi';
import { ApiError } from '../api/resultsApi';
import type { TallyDto } from '../types';

export function ResultsPage() {
    const { slug } = useParams<{ slug: string }>();
    const [tally, setTally] = useState<TallyDto | null>(null);
    const [pollId, setPollId] = useState<number | null>(null);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        resultsApi.getResults(slug!)
            .then(t => {
                setTally(t);
                setPollId(t.pollId);
            })
            .catch(err => {
                if (err instanceof ApiError && err.status === 403) {
                    setError('Results are not public for this poll');
                } else {
                    const msg = err instanceof Error ? err.message : 'Failed to load results';
                    setError(msg);
                }
            });
    }, [slug]);

    const handleTallyUpdate = useCallback((t: TallyDto) => setTally(t), []);
    useSignalR(pollId, handleTallyUpdate);

    if (error) return <main><p role="alert">{error}</p></main>;
    if (!tally) return <main><p>Loading...</p></main>;

    return (
        <main>
            <h1>Results</h1>
            <ResultsChart tally={tally} />
        </main>
    );
}
