import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { ProtectedRoute } from '../components/ProtectedRoute';

// Mock the useAuth hook
jest.mock('../hooks/useAuth', () => ({
    useAuth: () => ({ creator: null, login: jest.fn(), register: jest.fn(), logout: jest.fn() }),
}));

describe('ProtectedRoute', () => {
    it('unauthenticatedUser_redirectsToLogin', () => {
        render(
            <MemoryRouter initialEntries={['/dashboard']}>
                <ProtectedRoute>
                    <div>Protected Content</div>
                </ProtectedRoute>
            </MemoryRouter>
        );

        // Protected content should not be rendered
        expect(screen.queryByText('Protected Content')).not.toBeInTheDocument();
    });
});
