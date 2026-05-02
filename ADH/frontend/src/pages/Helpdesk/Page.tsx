import React, { useState } from 'react';
import { Sidebar } from '@/components/Layout/Sidebar';
import ChatInterface from './components/ChatInterface';
import TicketList from './components/TicketList';
import { useAuth } from '@/providers/AuthProvider';

const HelpdeskPage: React.FC = () => {
  const [activeTab, setActiveTab] = useState<'chat' | 'tickets'>('chat');
  const { logout } = useAuth();

  return (
    <div className="flex h-screen w-full bg-background overflow-hidden">
      <Sidebar 
        activeTab={activeTab} 
        onTabChange={setActiveTab} 
        onLogout={logout} 
      />
      <main className="flex-1 flex flex-col min-w-0 bg-background/50 relative">
        <div className="absolute inset-0 bg-grid-white/[0.02] pointer-events-none" />
        <div className="flex-1 flex flex-col relative z-10 p-6 overflow-hidden">
          {activeTab === 'chat' ? <ChatInterface /> : <TicketList />}
        </div>
      </main>
    </div>
  );
};

export default HelpdeskPage;
