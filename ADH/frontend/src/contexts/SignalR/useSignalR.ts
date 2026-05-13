import { useContext } from "react";
import { SignalRContext } from "./SignalRContext";

export const useSignalR = () => useContext(SignalRContext);
