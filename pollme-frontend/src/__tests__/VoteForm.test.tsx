import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { VoteForm } from '../components/VoteForm';
import type { PollVoteDto } from '../types';

const singleSelectPoll: PollVoteDto = {
    id: 1, slug: 'abc', question: 'Q?', mode: 'SingleSelect',
    options: [
        { id: 1, text: 'Option A', position: 0 },
        { id: 2, text: 'Option B', position: 1 },
    ],
};

const multiSelectPoll: PollVoteDto = {
    id: 2, slug: 'def', question: 'Q?', mode: 'MultiSelect',
    options: [
        { id: 10, text: 'Option X', position: 0 },
        { id: 11, text: 'Option Y', position: 1 },
        { id: 12, text: 'Option Z', position: 2 },
    ],
};

describe('VoteForm', () => {
    it('SingleSelect_submitDisabledInitially', () => {
        render(<VoteForm poll={singleSelectPoll} onSubmit={jest.fn()} />);
        expect(screen.getByRole('button', { name: /vote/i })).toBeDisabled();
    });

    it('SingleSelect_selectingOption_enablesSubmit', async () => {
        const user = userEvent.setup();
        render(<VoteForm poll={singleSelectPoll} onSubmit={jest.fn()} />);

        await user.click(screen.getByRole('radio', { name: /Option A/i }));

        expect(screen.getByRole('button', { name: /vote/i })).toBeEnabled();
    });

    it('MultiSelect_selectingMultipleOptions_callsOnSubmitWithAllIds', async () => {
        const user = userEvent.setup();
        const onSubmit = jest.fn();
        render(<VoteForm poll={multiSelectPoll} onSubmit={onSubmit} />);

        await user.click(screen.getByRole('checkbox', { name: /Option X/i }));
        await user.click(screen.getByRole('checkbox', { name: /Option Y/i }));
        await user.click(screen.getByRole('button', { name: /vote/i }));

        expect(onSubmit).toHaveBeenCalledWith(expect.arrayContaining([10, 11]));
        expect(onSubmit.mock.calls[0][0]).toHaveLength(2);
    });

    it('SingleSelect_selectingOneOption_callsOnSubmitWithSingleId', async () => {
        const user = userEvent.setup();
        const onSubmit = jest.fn();
        render(<VoteForm poll={singleSelectPoll} onSubmit={onSubmit} />);

        await user.click(screen.getByRole('radio', { name: /Option B/i }));
        await user.click(screen.getByRole('button', { name: /vote/i }));

        expect(onSubmit).toHaveBeenCalledWith([2]);
    });
});
