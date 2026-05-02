import React from 'react';
import { useQuery } from '@tanstack/react-query';
import { BarChart3, Users, Ticket, BookOpen, CheckCircle, ArrowUpRight } from 'lucide-react';
import { Card, CardHeader, CardTitle, CardContent, CardDescription } from '@/components/ui/Card';
import { useAuth } from '@/providers/AuthProvider';
import { Badge } from '@/components/ui/Badge';

const AdminDashboardPage: React.FC = () => {
  const { token } = useAuth();

  const { data: stats, isLoading } = useQuery({
    queryKey: ['admin-stats'],
    queryFn: async () => {
      const res = await fetch('http://localhost:5033/api/stats', {
        headers: { 'Authorization': `Bearer ${token}` }
      });
      return res.json();
    }
  });

  const StatCard = ({ title, value, icon: Icon, color, description }: any) => (
    <Card className="bg-card/40 border-border/50 overflow-hidden relative">
      <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
        <CardTitle className="text-sm font-medium">{title}</CardTitle>
        <Icon className="h-4 w-4 text-muted-foreground" />
      </CardHeader>
      <CardContent>
        <div className="text-3xl font-bold">{value}</div>
        <p className="text-xs text-muted-foreground mt-1">
          {description}
        </p>
        <div 
          className="absolute bottom-0 right-0 w-24 h-24 -mr-8 -mb-8 opacity-[0.03]" 
          style={{ color }}
        >
          <Icon size={96} />
        </div>
      </CardContent>
    </Card>
  );

  if (isLoading) return (
    <div className="flex-1 flex items-center justify-center">
      <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
    </div>
  );

  return (
    <div className="flex-1 flex flex-col gap-8">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <div className="p-2.5 rounded-xl bg-primary/10 text-primary">
            <BarChart3 size={24} />
          </div>
          <div>
            <h1 className="text-2xl font-bold">Panel Analityczny</h1>
            <p className="text-sm text-muted-foreground">Monitoruj wydajność swojego lokalnego asystenta AI</p>
          </div>
        </div>
        <Badge variant="outline" className="gap-1.5 px-3 py-1 bg-background">
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
        <Card className="lg:col-span-2 bg-card/40 border-border/50 h-[300px] flex flex-col items-center justify-center text-center p-8">
          <div className="p-4 rounded-full bg-secondary/50 mb-4">
            <BarChart3 size={32} className="text-muted-foreground" />
          </div>
          <h3 className="text-lg font-semibold">Wykresy aktywności</h3>
          <p className="text-muted-foreground max-w-sm mt-2">
            Dane statystyczne są zbierane. Wykresy czasowe zostaną odblokowane po zebraniu danych z pełnych 7 dni.
          </p>
        </Card>

        <Card className="bg-card/40 border-border/50 overflow-hidden">
          <CardHeader>
            <CardTitle className="text-lg">Ostatnie Akcje</CardTitle>
            <CardDescription>Logi systemowe w czasie rzeczywistym</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            {[1, 2, 3].map((_, i) => (
              <div key={i} className="flex items-center gap-3 text-sm">
                <div className="w-2 h-2 rounded-full bg-primary" />
                <div className="flex-1">
                  <p className="font-medium">Utworzono ticket JIRA</p>
                  <p className="text-xs text-muted-foreground text-opacity-70">2 minuty temu</p>
                </div>
                <ArrowUpRight size={14} className="text-muted-foreground" />
              </div>
            ))}
          </CardContent>
        </Card>
      </div>
    </div>
  );
};

export default AdminDashboardPage;
