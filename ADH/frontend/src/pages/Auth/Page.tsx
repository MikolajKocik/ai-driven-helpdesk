import React, { useState } from 'react';
import { useAuth } from '@/providers/AuthProvider';
import { authApi } from '@/api';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Card, CardHeader, CardTitle, CardDescription, CardContent, CardFooter } from '@/components/ui/Card';
import { ShieldCheck } from 'lucide-react';

const AuthPage: React.FC = () => {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const { login } = useAuth();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const data = await authApi.login(username, password);
      login(data.token);
    } catch (err) {
      alert("Błąd logowania lub połączenia z serwerem");
    }
  };

  return (
    <div className="min-h-screen w-full flex items-center justify-center bg-background p-4">
      <Card className="w-full max-w-md border-border/50 bg-card/50 backdrop-blur-sm">
        <CardHeader className="space-y-1 flex flex-col items-center">
          <div className="p-3 rounded-2xl bg-primary/10 text-primary mb-2">
            <ShieldCheck size={40} />
          </div>
          <CardTitle className="text-3xl font-bold tracking-tight">Witaj w ADH</CardTitle>
          <CardDescription>
            Zaloguj się do swojego centrum pomocy AI
          </CardDescription>
        </CardHeader>
        <form onSubmit={handleSubmit}>
          <CardContent className="grid gap-4">
            <div className="grid gap-2">
              <label htmlFor="username" className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70">
                Użytkownik
              </label>
              <Input
                id="username"
                placeholder="admin"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                required
              />
            </div>
            <div className="grid gap-2">
              <label htmlFor="password" className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70">
                Hasło
              </label>
              <Input
                id="password"
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
              />
            </div>
          </CardContent>
          <CardFooter>
            <Button className="w-full text-lg h-12" type="submit">
              Zaloguj się
            </Button>
          </CardFooter>
        </form>
      </Card>
    </div>
  );
};

export default AuthPage;
