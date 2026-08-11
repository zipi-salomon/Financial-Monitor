import React, { useEffect, useState } from 'react';
import { fetchTransactionsApi, signalRService } from '../services/apiService';
import type { Transaction } from '../services/apiService';
export const LiveDashboard: React.FC = () => {
  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [filterStatus, setFilterStatus] = useState<string>('All');
  const [connectionStatus, setConnectionStatus] = useState<string>('Connecting...');

  useEffect(() => {
    // שליפה ראשונית דרך השירות
    fetchTransactionsApi()
      .then(data => setTransactions(data))
      .catch(err => console.error(err));

    signalRService.startConnection((newTransaction) => {
      setTransactions(prev => [newTransaction, ...prev.slice(0, 99)]);
    }).then(status => setConnectionStatus(status));

    return () => {
      signalRService.stopConnection();
    };
  }, []);

  const filteredTransactions = transactions.filter(tx => {
    if (filterStatus === 'All') return true;
    return tx.status.toLowerCase() === filterStatus.toLowerCase();
  });

  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case 'completed': return '#10b981';
      case 'failed': return '#ef4444';
      case 'pending': return '#f59e0b';
      default: return '#64748b';
    }
  };

  return (
    <div style={{ padding: '2rem', maxWidth: '1000px', margin: '0 auto', fontFamily: 'Arial, sans-serif' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem' }}>
        <div>
          <h2>Live Dashboard (/monitor)</h2>
          <p style={{ margin: 0, fontSize: '0.9rem', color: connectionStatus.includes('Connected') ? '#10b981' : '#ef4444' }}>
            Status: <strong>{connectionStatus}</strong>
          </p>
        </div>
        <div>
          <label style={{ marginRight: '0.5rem', fontWeight: 'bold' }}>Filter Status:</label>
          <select value={filterStatus} onChange={(e) => setFilterStatus(e.target.value)} style={{ padding: '0.5rem', borderRadius: '4px', border: '1px solid #cbd5e1' }}>
            <option value="All">All</option>
            <option value="Completed">Completed</option>
            <option value="Failed">Failed (Errors)</option>
            <option value="Pending">Pending</option>
          </select>
        </div>
      </div>

      <div style={{ overflowX: 'auto', background: '#fff', borderRadius: '8px', boxShadow: '0 1px 3px rgba(0,0,0,0.1)', border: '1px solid #e2e8f0' }}>
        <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left' }}>
          <thead>
            <tr style={{ background: '#f1f5f9', borderBottom: '1px solid #cbd5e1' }}>
              <th style={{ padding: '0.75rem 1rem' }}>Amount</th>
              <th style={{ padding: '0.75rem 1rem' }}>Currency</th>
              <th style={{ padding: '0.75rem 1rem' }}>Status</th>
              <th style={{ padding: '0.75rem 1rem' }}>Timestamp</th>
            </tr>
          </thead>
          <tbody>
            {filteredTransactions.length === 0 ? (
              <tr><td colSpan={5} style={{ padding: '2rem', textAlign: 'center', color: '#64748b' }}>No transactions found.</td></tr>
            ) : (
              filteredTransactions.map((tx) => (
                <tr key={tx.transactionId} style={{ borderBottom: '1px solid #f1f5f9' }}>
                  <td style={{ padding: '0.75rem 1rem', fontWeight: 'bold' }}>{tx.amount.toLocaleString()}</td>
                  <td style={{ padding: '0.75rem 1rem' }}>{tx.currency}</td>
                  <td style={{ padding: '0.75rem 1rem' }}>
                    <span style={{ backgroundColor: getStatusColor(tx.status), color: '#fff', padding: '0.25rem 0.75rem', borderRadius: '12px', fontSize: '0.8rem', fontWeight: 'bold' }}>
                      {tx.status}
                    </span>
                  </td>
                  <td style={{ padding: '0.75rem 1rem', color: '#64748b', fontSize: '0.85rem' }}>{new Date(tx.timestamp).toLocaleTimeString()}</td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
};