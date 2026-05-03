import React, { useState } from 'react';
import { Sidebar } from '@/components/Layout/Sidebar';
import ChatInterface from './components/ChatInterface';
import TicketList from './components/TicketList';
import { useAuth } from '@/providers/AuthProvider';
import { motion, AnimatePresence } from 'framer-motion';

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
          <AnimatePresence mode="wait">
            <motion.div
              key={activeTab}
              initial={{ opacity: 0, x: 20 }}
              animate={{ opacity: 1, x: 0 }}
              exit={{ opacity: 0, x: -20 }}
              transition={{ duration: 0.2 }}
              className="flex-1 flex flex-col overflow-hidden"
            >
              {activeTab === 'chat' ? <ChatInterface /> : <TicketList />}
            </motion.div>
          </AnimatePresence>
        </div>
      </main>
    </div>
  );
};

export default HelpdeskPage;
