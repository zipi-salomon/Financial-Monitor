import React from 'react';

interface NavbarProps {
  currentRoute: 'add' | 'monitor';
  onRouteChange: (route: 'add' | 'monitor') => void;
}

export const Navbar: React.FC<NavbarProps> = ({ currentRoute, onRouteChange }) => {
  return (
    <nav style={{ backgroundColor: '#1e293b', padding: '1rem 2rem', color: '#fff', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
      <h2 style={{ margin: 0, fontSize: '1.25rem' }}>Financial Monitor MVP</h2>
      <div style={{ display: 'flex', gap: '1rem' }}>
        <button
          onClick={() => onRouteChange('add')}
          style={{
            background: currentRoute === 'add' ? '#3b82f6' : 'transparent',
            border: 'none',
            color: '#fff',
            padding: '0.5rem 1rem',
            borderRadius: '4px',
            cursor: 'pointer',
            fontWeight: 'bold'
          }}
        >
          Transaction Simulator (/add)
        </button>
        <button
          onClick={() => onRouteChange('monitor')}
          style={{
            background: currentRoute === 'monitor' ? '#3b82f6' : 'transparent',
            border: 'none',
            color: '#fff',
            padding: '0.5rem 1rem',
            borderRadius: '4px',
            cursor: 'pointer',
            fontWeight: 'bold'
          }}
        >
          Live Dashboard (/monitor)
        </button>
      </div>
    </nav>
  );
};