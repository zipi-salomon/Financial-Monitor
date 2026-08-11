import * as signalR from '@microsoft/signalr';
import { v4 as uuidv4 } from 'uuid';

const API_BASE_URL = 'http://localhost:5259/api/transactions';
const HUB_URL = 'http://localhost:5259/hubs/transactions';

export interface Transaction {
  transactionId: string;
  amount: number;
  currency: string;
  status: string;
  timestamp: string;
}

export const sendTransactionApi = async (transaction: { amount: number; currency: string; status: string }) => {
  const idempotencyKey = uuidv4();

  const response = await fetch(API_BASE_URL, {
    method: 'POST',
    headers: { 
      'Content-Type': 'application/json',
      'X-Idempotency-Key': idempotencyKey 
    },
    body: JSON.stringify(transaction)
  });

  if (response.status === 409) {
    throw new Error('העסקה כבר נשלחה ונמצאת בטיפול.');
  }

  if (!response.ok) throw new Error('Failed to send transaction');
  return response.json();
};

export const fetchTransactionsApi = async (): Promise<Transaction[]> => {
  const response = await fetch(API_BASE_URL);
  if (!response.ok) throw new Error('Failed to fetch transactions');
  return response.json();
};

export class SignalRService {
  private connection: signalR.HubConnection;

  constructor() {
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(HUB_URL)
      .withAutomaticReconnect()
      .build();
  }

  public async startConnection(onReceive: (transaction: Transaction) => void): Promise<string> {
    if (this.connection.state === signalR.HubConnectionState.Connected || 
        this.connection.state === signalR.HubConnectionState.Connecting) {
      return 'Connected (Real-Time Active)';
    }

    try {
      await this.connection.start();
      
      this.connection.off('ReceiveTransaction'); // מניעת כפילות מאזינים
      this.connection.on('ReceiveTransaction', onReceive);
      
      return 'Connected (Real-Time Active)';
    } catch (err) {
      console.error('SignalR Connection Error: ', err);
      return 'Connection Failed';
    }
  }

  public async stopConnection() {
    if (this.connection.state === signalR.HubConnectionState.Connected) {
      await this.connection.stop();
    }
  }
}

export const signalRService = new SignalRService();