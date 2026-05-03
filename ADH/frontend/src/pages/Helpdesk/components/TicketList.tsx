import React from 'react';
import { useQuery } from '@tanstack/react-query';
import { ticketsApi } from '@/api';
import { Card, CardContent } from '@/components/ui/Card';
import { Badge } from '@/components/ui/Badge';
import { Ticket as TicketIcon, Calendar, Clock, ChevronRight } from 'lucide-react';

const TicketList: React.FC = () => {
  const { data: tickets = [], isLoading } = useQuery({
    queryKey: ['tickets'],
    queryFn: () => ticketsApi.getTickets()
  });

  if (isLoading) return (
    <div className="flex-1 flex items-center justify-center">
      <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
    </div>
  );

  return (
    <div className="flex-1 flex flex-col gap-6 overflow-hidden">
      <div className="flex items-center gap-3">
        <div className="p-2.5 rounded-xl bg-primary/10 text-primary">
          <TicketIcon size={24} />
        </div>
        <div>
          <h2 className="text-2xl font-bold">Twoje Zgłoszenia</h2>
          <p className="text-sm text-muted-foreground">Śledź status swoich zgłoszeń w czasie rzeczywistym</p>
        </div>
      </div>

      <div className="flex-1 overflow-y-auto space-y-4 pr-2">
        {tickets.length === 0 ? (
          <div className="h-full flex flex-col items-center justify-center text-center opacity-30 py-20">
            <TicketIcon size={48} className="mb-4" />
            <p className="text-lg">Brak aktywnych zgłoszeń</p>
          </div>
        ) : (
          tickets.map((ticket: any) => (
            <Card key={ticket.id} className="group hover:border-primary/30 transition-all cursor-pointer bg-card/40 border-border/50">
              <CardContent className="p-5 flex items-center justify-between">
                <div className="flex flex-col gap-3 flex-1">
                  <div className="flex items-center gap-3">
                    <Badge variant={ticket.isResolved ? "default" : "secondary"}>
                      {ticket.isResolved ? "Rozwiązany" : "W toku"}
                    </Badge>
                    <span className="text-xs text-muted-foreground font-mono uppercase tracking-tighter">
                      ID: {ticket.id.substring(0, 8)}
                    </span>
                  </div>
                  <p className="text-lg font-medium leading-tight group-hover:text-primary transition-colors">
                    {ticket.description}
                  </p>
                  <div className="flex items-center gap-4 text-xs text-muted-foreground">
                    <div className="flex items-center gap-1.5">
                      <Calendar size={14} />
                      {new Date(ticket.createdAt).toLocaleDateString()}
                    </div>
                    <div className="flex items-center gap-1.5">
                      <Clock size={14} />
                      {new Date(ticket.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                    </div>
                  </div>
                </div>
                <ChevronRight size={20} className="text-muted-foreground group-hover:text-primary group-hover:translate-x-1 transition-all" />
              </CardContent>
            </Card>
          ))
        )}
      </div>
    </div>
  );
};

export default TicketList;
