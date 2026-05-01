import type { TallyDto } from '../types';

interface ResultsChartProps {
    tally: TallyDto;
}

export function ResultsChart({ tally }: ResultsChartProps) {
    return (
        <div>
            {tally.options.map(option => (
                <div key={option.optionId}>
                    <span>{option.text}</span>
                    <progress value={option.percentage} max={100} />
                    <span>{option.votes} votes ({option.percentage}%)</span>
                </div>
            ))}
        </div>
    );
}
