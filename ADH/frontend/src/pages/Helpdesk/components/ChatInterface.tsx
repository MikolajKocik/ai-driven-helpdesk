import React, { useState, useRef, useEffect } from 'react';
import { useSignalR } from '@/providers/SignalRProvider';
import { chatApi } from '@/api';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/Card';
import { User, Bot, Send, Sparkles } from 'lucide-react';
import { cn } from '@/lib/utils';

interface Message {
  role: 'user' | 'assistant';
  content: string;
}

const ChatInterface: React.FC = () => {
  const [messages, setMessages] = useState<Message[]>([]);
  const [input, setInput] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [isAiTyping, setIsAiTyping] = useState(false);
  const { connection } = useSignalR();
  const messagesEndRef = useRef<HTMLDivElement>(null);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  };

  useEffect(() => {
    if (connection) {
      connection.on('AiStatus', (status: string) => {
        setIsAiTyping(status === 'typing');
      });
    }
    return () => {
      connection?.off('AiStatus');
    };
  }, [connection]);

  useEffect(() => {
    scrollToBottom();
  }, [messages, isAiTyping]);

  const handleSend = async () => {
    if (!input.trim() || isLoading) return;

    const userMsg: Message = { role: 'user', content: input };
    const newMessages = [...messages, userMsg];
    setMessages(newMessages);
    setInput('');
    setIsLoading(true);

    try {
      const response = await chatApi.streamChat(newMessages);
      const reader = response.body?.getReader();
      const decoder = new TextDecoder();
      let assistantMsg = '';

      if (reader) {
        while (true) {
          const { done, value } = await reader.read();
          if (done) break;
          
          const chunk = decoder.decode(value);
          const lines = chunk.split('\n');
          
          for (const line of lines) {
            if (!line.startsWith('data: ')) continue;
            const dataStr = line.replace('data: ', '').trim();
            if (dataStr === '[DONE]') break;
            if (!dataStr) continue;
            
            try {
              const data = JSON.parse(dataStr);
              assistantMsg += data.content;
              setMessages([...newMessages, { role: 'assistant', content: assistantMsg }]);
            } catch (e) {
              console.error("Parse error", e);
            }
          }
        }
      }
    } catch (err) {
      console.error(err);
      setMessages([...newMessages, { role: 'assistant', content: 'Wystąpił błąd podczas komunikacji z serwerem.' }]);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <Card className="flex-1 flex flex-col border-none bg-transparent shadow-none overflow-hidden">
      <CardHeader className="px-0 pb-6 pt-0">
        <div className="flex items-center gap-3">
          <div className="p-2.5 rounded-xl bg-primary/10 text-primary animate-pulse">
            <Sparkles size={24} />
          </div>
          <div>
            <CardTitle className="text-2xl font-bold">Asystent AI</CardTitle>
            <p className="text-sm text-muted-foreground">W czym mogę Ci dzisiaj pomóc?</p>
          </div>
        </div>
      </CardHeader>
      
      <CardContent className="flex-1 overflow-y-auto px-0 space-y-6 scrollbar-hide">
        {messages.length === 0 && (
          <div className="h-full flex flex-col items-center justify-center text-center opacity-40 space-y-4">
            <Bot size={64} strokeWidth={1} />
            <p className="max-w-[200px] text-lg font-medium leading-tight">
              Zadaj pytanie, aby rozpocząć rozmowę z asystentem
            </p>
          </div>
        )}
        
        {messages.map((msg, i) => (
          <div key={i} className={cn(
            "flex gap-4 p-4 rounded-2xl transition-all",
            msg.role === 'user' ? "bg-secondary/30 ml-12" : "bg-primary/5 mr-12 border border-primary/10"
          )}>
            <div className={cn(
              "w-10 h-10 rounded-xl flex items-center justify-center shrink-0",
              msg.role === 'user' ? "bg-secondary text-secondary-foreground" : "bg-primary text-primary-foreground"
            )}>
              {msg.role === 'user' ? <User size={20} /> : <Bot size={20} />}
            </div>
            <div className="flex-1 text-base leading-relaxed whitespace-pre-wrap">
              {msg.content}
            </div>
          </div>
        ))}
        {isAiTyping && (
          <div className="flex gap-4 p-4 rounded-2xl bg-primary/5 mr-12 border border-primary/10 animate-pulse">
            <div className="w-10 h-10 rounded-xl bg-primary text-primary-foreground flex items-center justify-center shrink-0">
              <Bot size={20} />
            </div>
            <div className="flex-1 flex items-center">
              <div className="flex gap-1">
                <div className="w-1.5 h-1.5 bg-primary rounded-full animate-bounce" />
                <div className="w-1.5 h-1.5 bg-primary rounded-full animate-bounce [animation-delay:0.2s]" />
                <div className="w-1.5 h-1.5 bg-primary rounded-full animate-bounce [animation-delay:0.4s]" />
              </div>
            </div>
          </div>
        )}
        <div ref={messagesEndRef} />
      </CardContent>

      <div className="pt-6">
        <div className="relative group">
          <Input
            className="pr-14 py-7 text-lg bg-card/50 border-border/50 focus-visible:ring-primary/20 transition-all rounded-2xl"
            placeholder="Napisz w czym masz problem..."
            value={input}
            onChange={(e) => setInput(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && handleSend()}
          />
          <Button 
            size="icon" 
            className="absolute right-2 top-1/2 -translate-y-1/2 h-10 w-10 rounded-xl shadow-lg transition-transform hover:scale-105 active:scale-95"
            onClick={handleSend}
            disabled={isLoading || !input.trim()}
          >
            <Send size={18} />
          </Button>
        </div>
        <p className="text-[11px] text-center mt-3 text-muted-foreground uppercase tracking-widest opacity-50">
          AI może generować błędy. Zawsze weryfikuj ważne informacje.
        </p>
      </div>
    </Card>
  );
};

export default ChatInterface;
