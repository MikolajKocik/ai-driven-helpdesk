import React, { createContext, useContext, useEffect, useState, type ReactNode } from 'react';
import * as signalR from '@microsoft/signalr';
import { useAuth } from './AuthProvider';

interface SignalRContextType {
  connection: signalR.HubConnection | null;
}

const SignalRContext = createContext<SignalRContextType>({ connection: null });

export const SignalRProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const { token } = useAuth();
  const [connection, setConnection] = useState<signalR.HubConnection | null>(null);

  useEffect(() => {
    if (token) {
      const newConnection = new signalR.HubConnectionBuilder()
        .withUrl('http://localhost:5033/hubs/chat', {
          accessTokenFactory: () => token
        })
        .withAutomaticReconnect()
        .build();

      setConnection(newConnection);
    } else {
      setConnection(null);
    }
  }, [token]);

  useEffect(() => {
    if (connection) {
      connection.start()
        .then(() => console.log('SignalR Connected!'))
        .catch(err => console.error('SignalR Connection Error: ', err));

      return () => {
        connection.stop();
      };
    }
  }, [connection]);

  return (
    <SignalRContext.Provider value={{ connection }}>
      {children}
    </SignalRContext.Provider>
  );
};

export const useSignalR = () => useContext(SignalRContext);
