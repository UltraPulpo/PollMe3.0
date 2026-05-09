import { Navigate } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';

export function ProtectedRoute({ children }: { children: React.ReactNode }) {
    const { creator, isLoading } = useAuth();
    if (isLoading) return <p>Loading...</p>;
    if (!creator) return <Navigate to="/login" replace />;
    return <>{children}</>;
}
