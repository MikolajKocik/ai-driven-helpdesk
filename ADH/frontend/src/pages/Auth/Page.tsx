import React, { useState } from 'react';
import { authApi } from '@/api';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Card, CardHeader, CardTitle, CardDescription, CardContent, CardFooter } from '@/components/ui/Card';
import { ShieldCheck } from 'lucide-react';
import { GoogleLogin } from '@react-oauth/google';
import { toast } from 'sonner';
import { useAuth } from '@/contexts/Auth/useAuth';

export default function AuthPage() {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const { login } = useAuth();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const data = await authApi.login(username, password);
      login(data.token);
    } catch (err) {
      toast.error("Błąd logowania lub połączenia z serwerem");
      console.error(err);
    }
  };

  const handleGoogleSuccess = async (credentialResponse: any) => {
    try {
      const data = await authApi.googleLogin(credentialResponse.credential);
      login(data.token);
      toast.success("Zalogowano pomyślnie przez Google");
    } catch (err) {
      toast.error("Błąd logowania przez Google");
      console.error(err);
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

        <CardContent className="grid gap-4">
          <div className="flex flex-col items-center justify-center py-2 border-b border-border/50 mb-2">
            <GoogleLogin
              onSuccess={handleGoogleSuccess}
              onError={() => toast.error("Błąd logowania Google")}
              useOneTap
              theme="filled_blue"
              shape="pill"
            />
          </div>

          <div className="relative">
            <div className="absolute inset-0 flex items-center">
              <span className="w-full border-t" />
            </div>
            <div className="relative flex justify-center text-xs uppercase">
              <span className="bg-card px-2 text-muted-foreground">lub zaloguj się tradycyjnie</span>
            </div>
          </div>

          <form onSubmit={handleSubmit} className="grid gap-4">
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
            <Button className="w-full text-lg h-12 mt-2" type="submit">
              Zaloguj się
            </Button>
          </form>
        </CardContent>
        <CardFooter className="flex justify-center">
          <p className="text-xs text-muted-foreground">© 2026 AI-Driven Helpdesk</p>
        </CardFooter>
      </Card>
    </div>
  );
};
