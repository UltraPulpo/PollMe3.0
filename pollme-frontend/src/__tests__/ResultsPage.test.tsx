import { render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { ResultsPage } from '../pages/ResultsPage';
import { ApiError } from '../api/resultsApi';

jest.mock('../api/resultsApi', () => {
    const actual = jest.requireActual('../api/resultsApi');
    return {
        ...actual,
        getResults: jest.fn()
    };
});
jest.mock('../hooks/useSignalR', () => ({ useSignalR: jest.fn() }));

import * as resultsApi from '../api/resultsApi';

describe('ResultsPage', () => {
    it('creatorOnlyResults_showsFriendlyForbiddenMessage', async () => {
        (resultsApi.getResults as jest.Mock).mockRejectedValue(new ApiError(403, ''));

        render(
            <MemoryRouter initialEntries={['/results/abc123']}>
                <Routes>
                    <Route path="/results/:slug" element={<ResultsPage />} />
                </Routes>
            </MemoryRouter>
        );

        await waitFor(() => {
            expect(screen.getByRole('alert')).toHaveTextContent('Results are not public for this poll');
        });
    });
});
