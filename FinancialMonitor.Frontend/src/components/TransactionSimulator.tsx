import React, { useState } from 'react';
import { sendTransactionApi } from '../services/apiService';

export const TransactionSimulator: React.FC = () => {
  const [amount, setAmount] = useState<string>('1500.50');
  const [currency, setCurrency] = useState<string>('USD');
  const [status, setStatus] = useState<string>('Completed');
  const [loading, setLoading] = useState<boolean>(false);
  const [message, setMessage] = useState<string>('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (loading) return; 

    setLoading(true);
    setMessage('');
    
    try {
      await sendTransactionApi({
        amount: parseFloat(amount) || 100,
        currency,
        status
      });
      setMessage('Transaction sent successfully!');
    } catch (err: unknown) {
  const errorText = err instanceof Error ? err.message : 'Error sending transaction.';
  setMessage(errorText);
  console.error(err);
} finally {
      setLoading(false);
    }
  };

  const handleBulkGenerate = async () => {
    if (loading) return;

    setLoading(true);
    setMessage('Generating 100 transactions...');

    const statuses = ['Pending', 'Completed', 'Failed'];
    const currencies = ['USD', 'EUR', 'GBP', 'ILS'];
    let successCount = 0;

    for (let i = 0; i < 100; i++) {
      try {
        await sendTransactionApi({
          amount: +(Math.random() * 5000).toFixed(2),
          currency: currencies[Math.floor(Math.random() * currencies.length)],
          status: statuses[Math.floor(Math.random() * statuses.length)]
        });
        successCount++;
      } catch (err) {
        console.error(`Failed at index ${i}`, err);
      }
    }

    setLoading(false);
    setMessage(`${successCount}/100 Transactions generated and sent successfully!`);
  };

  return (
    <div style={{ padding: '2rem', maxWidth: '600px', margin: '0 auto', fontFamily: 'Arial, sans-serif' }}>
      <h2>Transaction Simulator (/add)</h2>
      <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '1rem', background: '#f8fafc', padding: '1.5rem', borderRadius: '8px', border: '1px solid #e2e8f0' }}>
        <div>
          <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: 'bold' }}>Amount:</label>
          <input 
            type="number" 
            step="0.01" 
            value={amount} 
            onChange={(e) => setAmount(e.target.value)} 
            disabled={loading}
            style={{ width: '100%', padding: '0.5rem' }} 
          />
        </div>
        <div>
          <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: 'bold' }}>Currency:</label>
          <select 
            value={currency} 
            onChange={(e) => setCurrency(e.target.value)} 
            disabled={loading}
            style={{ width: '100%', padding: '0.5rem' }}
          >
            <option value="USD">USD</option>
            <option value="EUR">EUR</option>
            <option value="GBP">GBP</option>
            <option value="ILS">ILS</option>
          </select>
        </div>
        <div>
          <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: 'bold' }}>Status:</label>
          <select 
            value={status} 
            onChange={(e) => setStatus(e.target.value)} 
            disabled={loading}
            style={{ width: '100%', padding: '0.5rem' }}
          >
            <option value="Pending">Pending</option>
            <option value="Completed">Completed</option>
            <option value="Failed">Failed</option>
          </select>
        </div>

        <button 
          type="submit" 
          disabled={loading} 
          style={{ 
            backgroundColor: loading ? '#94a3b8' : '#3b82f6', 
            color: '#fff', 
            padding: '0.75rem', 
            border: 'none', 
            borderRadius: '4px', 
            cursor: loading ? 'not-allowed' : 'pointer', 
            fontWeight: 'bold' 
          }}
        >
          {loading ? 'Processing...' : 'Send Transaction (POST)'}
        </button>

        <button 
          type="button" 
          onClick={handleBulkGenerate} 
          disabled={loading} 
          style={{ 
            backgroundColor: loading ? '#94a3b8' : '#10b981', 
            color: '#fff', 
            padding: '0.75rem', 
            border: 'none', 
            borderRadius: '4px', 
            cursor: loading ? 'not-allowed' : 'pointer', 
            fontWeight: 'bold' 
          }}
        >
          Generate 100 Transactions (Load Test)
        </button>
      </form>

      {message && (
        <p style={{ 
          marginTop: '1rem', 
          color: message.toLowerCase().includes('error') || message.includes('already') ? 'red' : 'green', 
          fontWeight: 'bold' 
        }}>
          {message}
        </p>
      )}
    </div>
  );
};