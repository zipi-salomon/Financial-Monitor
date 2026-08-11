import React from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import { Layout } from './components/Layout';
import { LiveDashboard } from './components/LiveDashboard';
import { TransactionSimulator } from './components/TransactionSimulator';

export const AppRoutes: React.FC = () => {
  return (
    <Routes>
      {/* ה-Layout עוטף את כל הראוטים של האפליקציה */}
      <Route path="/" element={<Layout />}>
        <Route index element={<Navigate to="/monitor" replace />} />
        <Route path="monitor" element={<LiveDashboard />} />
        <Route path="add" element={<TransactionSimulator />} />
      </Route>
    </Routes>
  );
};