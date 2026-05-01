import { useState } from 'react';
import type { PollVoteDto } from '../types';

interface VoteFormProps {
    poll: PollVoteDto;
    onSubmit: (selectedIds: number[]) => void;
    isSubmitting?: boolean;
}

export function VoteForm({ poll, onSubmit, isSubmitting = false }: VoteFormProps) {
    const [selectedIds, setSelectedIds] = useState<number[]>([]);

    const handleSingleChange = (id: number) => {
        setSelectedIds([id]);
    };

    const handleMultiChange = (id: number, checked: boolean) => {
        setSelectedIds(prev =>
            checked ? [...prev, id] : prev.filter(x => x !== id)
        );
    };

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        onSubmit(selectedIds);
    };

    return (
        <form onSubmit={handleSubmit}>
            {poll.options.map(opt => (
                <label key={opt.id}>
                    {poll.mode === 'SingleSelect' ? (
                        <input
                            type="radio"
                            name="vote"
                            value={opt.id}
                            onChange={() => handleSingleChange(opt.id)}
                        />
                    ) : (
                        <input
                            type="checkbox"
                            value={opt.id}
                            onChange={e => handleMultiChange(opt.id, e.target.checked)}
                        />
                    )}
                    {opt.text}
                </label>
            ))}
            <button type="submit" disabled={selectedIds.length === 0 || isSubmitting}>
                {isSubmitting ? 'Submitting...' : 'Vote'}
            </button>
        </form>
    );
}
