import { Navigate } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';

export function ProtectedRoute({ children }: { children: React.ReactNode }) {
    const { creator } = useAuth();
    if (!creator) return <Navigate to="/login" replace />;
    return <>{children}</>;
}
