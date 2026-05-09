import { render, screen } from '@testing-library/react';
import { ResultsChart } from '../components/ResultsChart';
import type { TallyDto } from '../types';

describe('ResultsChart', () => {
    it('renders_progressBarsForEachOption', () => {
        const tally: TallyDto = {
            pollId: 1,
            options: [
                { optionId: 1, text: 'Option A', votes: 3, percentage: 75 },
                { optionId: 2, text: 'Option B', votes: 1, percentage: 25 },
            ],
        };

        const { container } = render(<ResultsChart tally={tally} />);

        const progressBars = container.querySelectorAll('progress');
        expect(progressBars).toHaveLength(2);
    });

    it('renders_zeroPercentageBarsCorrectly', () => {
        const tally: TallyDto = {
            pollId: 1,
            options: [
                { optionId: 1, text: 'Option A', votes: 0, percentage: 0 },
                { optionId: 2, text: 'Option B', votes: 0, percentage: 0 },
            ],
        };

        const { container } = render(<ResultsChart tally={tally} />);

        const progressBars = container.querySelectorAll('progress');
        progressBars.forEach(bar => {
            expect(bar).toHaveAttribute('value', '0');
        });
    });
});
