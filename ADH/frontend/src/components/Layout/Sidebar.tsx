import React from 'react';
import { MessageSquare, Ticket as TicketIcon, ShieldCheck, Database, LogOut, BarChart3 } from 'lucide-react';
import { useNavigate, useLocation } from 'react-router-dom';
import { Button } from '../ui/Button';
import { NavItem } from './NavItem';

type SidebarProps = {
  activeTab: 'chat' | 'tickets';
  onTabChange: (tab: 'chat' | 'tickets') => void;
  onLogout: () => void;
};

export const Sidebar: React.FC<SidebarProps> = ({ activeTab, onTabChange, onLogout }) => {
  const navigate = useNavigate();
  const location = useLocation();
  const isAdminPage = location.pathname.startsWith('/admin');

  return (
    <aside className="w-72 border-r border-border bg-card/30 flex flex-col p-6 gap-8">
      <div className="flex items-center gap-3 px-2">
        <div className="p-2 rounded-xl bg-primary/10 text-primary">
          <ShieldCheck size={28} />
        </div>
        <h1 className="text-xl font-bold tracking-tight">ADH System</h1>
      </div>
      
      <nav className="flex-1 flex flex-col gap-2">
        <div className="text-xs font-semibold text-muted-foreground uppercase tracking-wider px-2 mb-2">
          Użytkownik
        </div>
        <NavItem
          icon={MessageSquare} 
          label="AI Helpdesk" 
          isActive={activeTab === 'chat' && !isAdminPage}
          onClick={() => { onTabChange('chat'); navigate('/helpdesk'); }}
        />
        <NavItem 
          icon={TicketIcon} 
          label="Moje Zgłoszenia" 
          isActive={activeTab === 'tickets' && !isAdminPage}
          onClick={() => { onTabChange('tickets'); navigate('/helpdesk'); }}
        />

        <div className="my-4 border-t border-border/50" />

        <div className="text-xs font-semibold text-muted-foreground uppercase tracking-wider px-2 mb-2">
          Administracja
        </div>
        <NavItem 
          icon={BarChart3} 
          label="Analityka" 
          isActive={location.pathname === '/admin/dashboard'}
          onClick={() => navigate('/admin/dashboard')}
        />
        <NavItem 
          icon={Database} 
          label="Baza Wiedzy" 
          isActive={location.pathname === '/admin/articles'}
          onClick={() => navigate('/admin/articles')}
        />
      </nav>

      <div className="pt-4 border-t border-border/50">
        <Button 
          variant="destructive" 
          className="w-full justify-start gap-3 h-11 opacity-80 hover:opacity-100"
          onClick={onLogout}
        >
          <LogOut size={20} /> Wyloguj się
        </Button>
      </div>
    </aside>
  );
};
