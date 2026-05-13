import { useQuery } from '@tanstack/react-query';
import { BarChart3, Users, Ticket, BookOpen, CheckCircle, ArrowUpRight, ArrowLeft, Loader2 } from 'lucide-react';
import { Link } from 'react-router-dom';
import { api } from '@/api';
import { Card, CardHeader, CardTitle, CardContent, CardDescription } from '@/components/ui/Card';
import Badge from '@/components/ui/Badge';
import { motion } from 'framer-motion';
import { Button } from '@/components/ui/Button';
import { StatCard } from './components/StatCard';
import { containerVariants, itemVariants } from '@/lib/utils';
import { useSignalR } from '@/contexts/SignalR/useSignalR';
import { useEffect, useState } from 'react';

interface SystemLog {
  id: string,
  message: string,
  timestamp: Date
}

export default function AdminDashboardPage() {
  const { connection } = useSignalR();
  const [logs, setLogs] = useState<SystemLog[]>([]);

  const { data: stats, isLoading } = useQuery({
    queryKey: ['admin-stats'],
    queryFn: async () => {
      const res = await api.get('/stats');
      return res.data;
    }
  });

  useEffect(() => {
    if (!connection) return;

    const handleNewLog = (message: string) => {
      const newLog: SystemLog = {
        id: crypto.randomUUID(),
        message: message,
        timestamp: new Date()
      };

      setLogs(prevLogs => [newLog, ...prevLogs].slice(0, 10));
    };

    connection.on("NewSystemLog", handleNewLog);
    
    connection.on("TicketUpdated", () => {
       handleNewLog("JIRA ticket status updated");
    });

    return () => {
      connection.off("NewSystemLog", handleNewLog);
      connection.off("TicketUpdated"); 
    }
  }, [connection])

  if (isLoading) return (
    <div className="flex-1 flex flex-col items-center justify-center">
      <Loader2 className="h-8 w-8 animate-spin text-primary mb-2" />
      <p className="text-muted-foreground animate-pulse">Pobieranie statystyk...</p>
    </div>
  );

  return (
    <motion.div 
      variants={containerVariants}
      initial="hidden"
      animate="visible"
      className="flex-1 flex flex-col gap-8"
    >
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Link to="/helpdesk">
            <Button variant="ghost" size="icon" className="rounded-full hover:bg-primary/10 hover:text-primary transition-colors">
              <ArrowLeft size={20} />
            </Button>
          </Link>
          <div className="p-2.5 rounded-xl bg-primary/10 text-primary shadow-lg shadow-primary/5">
            <BarChart3 size={24} />
          </div>
          <div>
            <h1 className="text-2xl font-bold tracking-tight">Panel Analityczny</h1>
            <p className="text-sm text-muted-foreground">Monitoruj wydajność swojego lokalnego asystenta AI</p>
          </div>
        </div>
        <Badge variant="outline" className="gap-1.5 px-3 py-1 bg-background/50 backdrop-blur-md">
          <div className="w-2 h-2 rounded-full bg-green-500 animate-pulse" />
          System Online
        </Badge>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <StatCard title="Wszystkie Tickety" value={stats?.totalTickets} icon={Ticket} color="#6366f1" description="+12% od ostatniego tygodnia" />
        <StatCard title="Rozwiązane przez AI" value={stats?.resolvedTickets} icon={CheckCircle} color="#10b981" description="84% skuteczności" />
        <StatCard title="Baza Wiedzy" value={stats?.totalArticles} icon={BookOpen} color="#f59e0b" description="Aktywne artykuły" />
        <StatCard title="Aktywni Użytkownicy" value={stats?.totalUsers} icon={Users} color="#8b5cf6" description="W tej instancji" />
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        <motion.div variants={itemVariants} className="lg:col-span-2">
          <Card className="bg-card/40 border-border/50 h-[300px] flex flex-col items-center justify-center text-center p-8 backdrop-blur-sm">
            <div className="p-4 rounded-full bg-secondary/50 mb-4">
              <BarChart3 size={32} className="text-muted-foreground" />
            </div>
            <h3 className="text-lg font-semibold">Wykresy aktywności</h3>
            <p className="text-muted-foreground max-w-sm mt-2 text-sm">
              Dane statystyczne są zbierane. Wykresy czasowe zostaną odblokowane po zebraniu danych z pełnych 7 dni.
            </p>
          </Card>
        </motion.div>

        <motion.div variants={itemVariants}>
          <Card className="bg-card/40 border-border/50 overflow-hidden backdrop-blur-sm h-full">
            <CardHeader>
              <CardTitle className="text-lg">Ostatnie Akcje</CardTitle>
              <CardDescription>Logi systemowe w czasie rzeczywistym</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              {logs.length === 0 ? (
                <p className="text-sm text-muted-foreground italic text-center py-4">Brak nowych zdarzeń...</p>
              ) : (
                logs.map((log) => (
                <div key={log.id} className="flex items-center gap-3 text-sm group">
                  <div className="w-2 h-2 rounded-full bg-primary group-hover:scale-125 transition-transform" />
                  <div className="flex-1">
                    <p className="font-medium">{log.message}</p>
                    <p className="text-xs text-muted-foreground text-opacity-70">
                      {log.timestamp.toLocaleDateString([], { hour: '2-digit', minute: '2-digit', second: '2-digit' })} 
                    </p>
                  </div>
                  <ArrowUpRight size={14} className="text-muted-foreground group-hover:text-primary transition-colors" />
                </div>
                ))
              )}
            </CardContent>
          </Card>
        </motion.div>
      </div>
    </motion.div>
  );
};
