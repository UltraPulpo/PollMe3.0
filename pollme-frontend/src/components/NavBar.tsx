import { Link } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';

export function NavBar() {
    const { creator, logout } = useAuth();

    return (
        <nav>
            <Link to="/">PollMe</Link>
            {creator ? (
                <>
                    <Link to="/dashboard">Dashboard</Link>
                    <Link to="/polls/new">Create Poll</Link>
                    <button onClick={logout}>Logout</button>
                </>
            ) : (
                <>
                    <Link to="/login">Login</Link>
                    <Link to="/register">Register</Link>
                </>
            )}
        </nav>
    );
}
