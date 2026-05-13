import { createContext } from "react";
import * as signalR from '@microsoft/signalr';

interface SignalRContextType {
  connection: signalR.HubConnection | null;
}

export const SignalRContext = createContext<SignalRContextType>({ 
  connection: null 
});