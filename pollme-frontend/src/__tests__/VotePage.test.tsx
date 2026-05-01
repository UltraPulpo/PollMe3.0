import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { VotePage } from '../pages/VotePage';
import type { PollVoteDto, TallyDto } from '../types';

jest.mock('../api/pollsApi');
jest.mock('../api/voteApi');
jest.mock('../hooks/useVoteStatus');

import * as pollsApi from '../api/pollsApi';
import * as voteApi from '../api/voteApi';
import * as useVoteStatusModule from '../hooks/useVoteStatus';

const mockPoll: PollVoteDto = {
    id: 1, slug: 'test-slug', question: 'Test Question?', mode: 'SingleSelect',
    options: [{ id: 1, text: 'Option A', position: 0 }],
};

const mockTally: TallyDto = {
    pollId: 1,
    options: [{ optionId: 1, text: 'Option A', votes: 1, percentage: 100 }],
};

function renderVotePage(slug = 'test-slug') {
    return render(
        <MemoryRouter initialEntries={[`/vote/${slug}`]}>
            <Routes>
                <Route path="/vote/:slug" element={<VotePage />} />
                <Route path="/results/:slug" element={<div>Results Page</div>} />
            </Routes>
        </MemoryRouter>
    );
}

describe('VotePage', () => {
    beforeEach(() => {
        jest.clearAllMocks();
        (pollsApi.getPollBySlug as jest.Mock).mockResolvedValue(mockPoll);
        (useVoteStatusModule.useVoteStatus as jest.Mock).mockReturnValue({
            hasVoted: () => false,
            markVoted: jest.fn(),
        });
    });

    it('alreadyVoted_showsThankYouWithResultsLink', async () => {
        (useVoteStatusModule.useVoteStatus as jest.Mock).mockReturnValue({
            hasVoted: () => true,
            markVoted: jest.fn(),
        });

        renderVotePage();

        expect(screen.getByText(/thank you for voting/i)).toBeInTheDocument();
        expect(screen.getByRole('link', { name: /view results/i })).toBeInTheDocument();
    });

    it('notVoted_showsVoteForm', async () => {
        renderVotePage();

        await waitFor(() => {
            expect(screen.getByText('Test Question?')).toBeInTheDocument();
        });
        expect(screen.getByRole('button', { name: /vote/i })).toBeInTheDocument();
    });

    it('afterVoting_publicPoll_navigatesToResults', async () => {
        const user = userEvent.setup();
        (voteApi.submitVote as jest.Mock).mockResolvedValue(mockTally);

        renderVotePage();

        await waitFor(() => screen.getByText('Test Question?'));

        await user.click(screen.getByRole('radio'));
        await user.click(screen.getByRole('button', { name: /vote/i }));

        await waitFor(() => {
            expect(screen.getByText('Results Page')).toBeInTheDocument();
        });
    });
});
