import React from 'react';
import { Outlet, Link, useLocation } from 'react-router-dom';

export const Layout: React.FC = () => {
  const location = useLocation();

  const getButtonStyle = (path: string) => ({
    background: location.pathname === path ? '#3b82f6' : 'transparent',
    border: '1px solid #3b82f6',
    color: '#fff',
    padding: '0.5rem 1rem',
    borderRadius: '4px',
    cursor: 'pointer',
    fontWeight: 'bold',
    textDecoration: 'none',
    display: 'inline-block'
  });

  return (
    <div style={{ minHeight: '100vh', backgroundColor: '#f8fafc', fontFamily: 'Arial, sans-serif' }}>
      <nav style={{ backgroundColor: '#1e293b', padding: '1rem 2rem', color: '#fff', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h2 style={{ margin: 0, fontSize: '1.25rem' }}>Financial Monitor MVP</h2>
        <div style={{ display: 'flex', gap: '1rem' }}>
          <Link to="/add" style={getButtonStyle('/add')}>
            /add (Simulator)
          </Link>
          <Link to="/monitor" style={getButtonStyle('/monitor')}>
            /monitor (Live Dashboard)
          </Link>
        </div>
      </nav>

      <main style={{ padding: '2rem 0' }}>
        <Outlet />
      </main>
    </div>
  );
};