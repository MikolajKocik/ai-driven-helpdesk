import React, { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Trash2, BookOpen, Search } from 'lucide-react';
import { articlesApi } from '@/api';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Card, CardHeader, CardTitle, CardContent, CardDescription } from '@/components/ui/Card';
import { useAuth } from '@/providers/AuthProvider';
import { Badge } from '@/components/ui/Badge';

const AdminArticlesPage: React.FC = () => {
  const { token } = useAuth();
  const queryClient = useQueryClient();
  const [newArticle, setNewArticle] = useState({ title: '', content: '' });

  const { data: articles = [], isLoading } = useQuery({
    queryKey: ['admin-articles'],
    queryFn: () => articlesApi.getArticles(token!)
  });

  const createMutation = useMutation({
    mutationFn: (art: { title: string, content: string }) => articlesApi.createArticle(token!, art),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-articles'] });
      setNewArticle({ title: '', content: '' });
    }
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => articlesApi.deleteArticle(token!, id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-articles'] });
    }
  });

  return (
    <div className="flex-1 flex flex-col gap-8">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <div className="p-2.5 rounded-xl bg-primary/10 text-primary">
            <BookOpen size={24} />
          </div>
          <div>
            <h1 className="text-2xl font-bold">Baza Wiedzy</h1>
            <p className="text-sm text-muted-foreground">Zarządzaj artykułami, które zasilają inteligencję asystenta</p>
          </div>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        <Card className="lg:col-span-1 bg-card/40 border-border/50 h-fit">
          <CardHeader>
            <CardTitle>Nowy Artykuł</CardTitle>
            <CardDescription>Dodaj nową treść do indeksu wektorowego</CardDescription>
          </CardHeader>
          <CardContent>
            <form className="space-y-4" onSubmit={(e) => { e.preventDefault(); createMutation.mutate(newArticle); }}>
              <div className="space-y-2">
                <label className="text-sm font-medium leading-none">Tytuł</label>
                <Input 
                  placeholder="np. Jak zresetować hasło?" 
                  value={newArticle.title}
                  onChange={e => setNewArticle({...newArticle, title: e.target.value})}
                  required
                />
              </div>
              <div className="space-y-2">
                <label className="text-sm font-medium leading-none">Treść</label>
                <textarea 
                  className="flex min-h-[200px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                  placeholder="Pełna treść artykułu..." 
                  value={newArticle.content}
                  onChange={e => setNewArticle({...newArticle, content: e.target.value})}
                  required
                />
              </div>
              <Button className="w-full" type="submit" disabled={createMutation.isPending}>
                {createMutation.isPending ? 'Dodawanie...' : 'Dodaj do bazy wiedzy'}
              </Button>
            </form>
          </CardContent>
        </Card>

        <div className="lg:col-span-2 space-y-4">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground" size={18} />
            <Input className="pl-10" placeholder="Filtruj artykuły..." />
          </div>

          <div className="space-y-3">
            {isLoading && <p>Ładowanie artykułów...</p>}
            {articles.map((art: any) => (
              <Card key={art.id} className="bg-card/30 border-border/40 hover:border-primary/20 transition-all">
                <CardContent className="p-4">
                  <div className="flex items-start justify-between gap-4">
                    <div className="flex-1 space-y-1">
                      <div className="flex items-center gap-2">
                        <h3 className="font-semibold text-lg">{art.title}</h3>
                        {art.title.startsWith('[LOCAL]') && (
                          <Badge variant="secondary" className="text-[10px] uppercase">Local File</Badge>
                        )}
                      </div>
                      <p className="text-sm text-muted-foreground line-clamp-2">
                        {art.content}
                      </p>
                    </div>
                    <Button 
                      variant="ghost" 
                      size="icon"
                      className="text-muted-foreground hover:text-destructive transition-colors"
                      onClick={() => deleteMutation.mutate(art.id)}
                    >
                      <Trash2 size={18} />
                    </Button>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
};

export default AdminArticlesPage;
