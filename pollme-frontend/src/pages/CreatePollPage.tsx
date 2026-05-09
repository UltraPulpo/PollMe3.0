import { useState } from 'react';
import * as pollsApi from '../api/pollsApi';
import type { PollMode, ResultsVisibility } from '../types';

export function CreatePollPage() {
    const [question, setQuestion] = useState('');
    const [options, setOptions] = useState<string[]>(['', '']);
    const [mode, setMode] = useState<PollMode>('SingleSelect');
    const [visibility, setVisibility] = useState<ResultsVisibility>('Public');
    const [voteLink, setVoteLink] = useState<string | null>(null);
    const [error, setError] = useState<string | null>(null);

    const addOption = () => {
        if (options.length < 10) setOptions([...options, '']);
    };

    const removeOption = (index: number) => {
        if (options.length > 2) setOptions(options.filter((_, i) => i !== index));
    };

    const updateOption = (index: number, value: string) => {
        const updated = [...options];
        updated[index] = value;
        setOptions(updated);
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        try {
            const result = await pollsApi.createPoll({ question, options, mode, visibility });
            setVoteLink(result.voteLink);
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Failed to create poll');
        }
    };

    if (voteLink) {
        return (
            <main>
                <article>
                    <h1>Poll Created!</h1>
                    <p>Vote link: <a href={voteLink}>{window.location.origin + voteLink}</a></p>
                    <button onClick={() => navigator.clipboard.writeText(window.location.origin + voteLink)}>
                        Copy Link
                    </button>
                </article>
            </main>
        );
    }

    return (
        <main>
            <article>
                <h1>Create Poll</h1>
                {error && <p role="alert">{error}</p>}
                <form onSubmit={handleSubmit}>
                    <input
                        type="text"
                        placeholder="Question"
                        value={question}
                        onChange={e => setQuestion(e.target.value)}
                        required
                    />
                    {options.map((opt, i) => (
                        <div key={i}>
                            <input
                                type="text"
                                placeholder={`Option ${i + 1}`}
                                value={opt}
                                onChange={e => updateOption(i, e.target.value)}
                                required
                            />
                            <button type="button" onClick={() => removeOption(i)} disabled={options.length <= 2}>
                                Remove
                            </button>
                        </div>
                    ))}
                    <button type="button" onClick={addOption} disabled={options.length >= 10}>
                        Add Option
                    </button>
                    <select value={mode} onChange={e => setMode(e.target.value as PollMode)}>
                        <option value="SingleSelect">Single Select</option>
                        <option value="MultiSelect">Multi Select</option>
                    </select>
                    <select value={visibility} onChange={e => setVisibility(e.target.value as ResultsVisibility)}>
                        <option value="Public">Public</option>
                        <option value="CreatorOnly">Creator Only</option>
                    </select>
                    <button type="submit">Create Poll</button>
                </form>
            </article>
        </main>
    );
}
