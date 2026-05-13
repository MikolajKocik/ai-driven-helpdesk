import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Trash2, BookOpen, Search, ArrowLeft, Loader2 } from 'lucide-react';
import { articlesApi } from '@/api';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Card, CardHeader, CardTitle, CardContent, CardDescription } from '@/components/ui/Card';
import { Link } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import { toast } from 'sonner';
import Badge from '@/components/ui/Badge';

export default function AdminArticlesPage() {
  const queryClient = useQueryClient();
  const [newArticle, setNewArticle] = useState({ title: '', content: '' });

  const { data: articles = [], isLoading } = useQuery({
    queryKey: ['admin-articles'],
    queryFn: () => articlesApi.getArticles()
  });

  const createMutation = useMutation({
    mutationFn: (art: { title: string, content: string }) => articlesApi.createArticle(art),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-articles'] });
      setNewArticle({ title: '', content: '' });
      toast.success('Artykuł został dodany do bazy wiedzy');
    },
    onError: () => {
      toast.error('Błąd podczas dodawania artykułu');
    }
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => articlesApi.deleteArticle(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-articles'] });
      toast.info('Artykuł został usunięty');
    },
    onError: () => {
      toast.error('Nie udało się usunąć artykułu');
    }
  });

  return (
    <motion.div 
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.4 }}
      className="flex-1 flex flex-col gap-8"
    >
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Link to="/helpdesk">
            <Button variant="ghost" size="icon" className="rounded-full hover:bg-primary/10 hover:text-primary transition-colors">
              <ArrowLeft size={20} />
            </Button>
          </Link>
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
        <Card className="lg:col-span-1 bg-card/40 border-border/50 backdrop-blur-md h-fit">
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
                  className="flex min-h-[200px] w-full rounded-md border border-input bg-background/50 px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50 transition-all focus:bg-background"
                  placeholder="Pełna treść artykułu..." 
                  value={newArticle.content}
                  onChange={e => setNewArticle({...newArticle, content: e.target.value})}
                  required
                />
              </div>
              <Button className="w-full" type="submit" disabled={createMutation.isPending}>
                {createMutation.isPending ? (
                  <>
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                    Dodawanie...
                  </>
                ) : 'Dodaj do bazy wiedzy'}
              </Button>
            </form>
          </CardContent>
        </Card>

        <div className="lg:col-span-2 space-y-4">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground" size={18} />
            <Input className="pl-10 bg-background/50" placeholder="Filtruj artykuły..." />
          </div>

          <div className="space-y-3">
            {isLoading && (
              <div className="flex flex-col items-center justify-center p-12 text-muted-foreground">
                <Loader2 className="h-8 w-8 animate-spin mb-2" />
                <p>Ładowanie artykułów...</p>
              </div>
            )}
            
            <AnimatePresence mode="popLayout">
              {articles.map((art: any) => (
                <motion.div
                  key={art.id}
                  layout
                  initial={{ opacity: 0, scale: 0.95 }}
                  animate={{ opacity: 1, scale: 1 }}
                  exit={{ opacity: 0, scale: 0.95 }}
                  transition={{ duration: 0.2 }}
                >
                  <Card className="bg-card/30 border-border/40 hover:border-primary/20 hover:bg-card/40 transition-all cursor-default group">
                    <CardContent className="p-4">
                      <div className="flex items-start justify-between gap-4">
                        <div className="flex-1 space-y-1">
                          <div className="flex items-center gap-2">
                            <h3 className="font-semibold text-lg group-hover:text-primary transition-colors">{art.title}</h3>
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
                          onClick={() => {
                            if (window.confirm('Czy na pewno chcesz usunąć ten artykuł?')) {
                              deleteMutation.mutate(art.id);
                            }
                          }}
                        >
                          <Trash2 size={18} />
                        </Button>
                      </div>
                    </CardContent>
                  </Card>
                </motion.div>
              ))}
            </AnimatePresence>
          </div>
        </div>
      </div>
    </motion.div>
  );
};
