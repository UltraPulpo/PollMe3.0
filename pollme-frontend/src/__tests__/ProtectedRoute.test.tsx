import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { ProtectedRoute } from '../components/ProtectedRoute';
import * as useAuthModule from '../hooks/useAuth';

jest.mock('../hooks/useAuth');

describe('ProtectedRoute', () => {
    it('authLoading_rendersLoadingPlaceholder', () => {
        (useAuthModule.useAuth as jest.Mock).mockReturnValue({
            creator: null,
            isLoading: true,
            login: jest.fn(),
            register: jest.fn(),
            logout: jest.fn()
        });

        render(
            <MemoryRouter initialEntries={['/dashboard']}>
                <ProtectedRoute>
                    <div>Protected Content</div>
                </ProtectedRoute>
            </MemoryRouter>
        );

        expect(screen.getByText('Loading...')).toBeInTheDocument();
    });

    it('unauthenticatedUser_redirectsToLogin', () => {
        (useAuthModule.useAuth as jest.Mock).mockReturnValue({
            creator: null,
            isLoading: false,
            login: jest.fn(),
            register: jest.fn(),
            logout: jest.fn()
        });

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
