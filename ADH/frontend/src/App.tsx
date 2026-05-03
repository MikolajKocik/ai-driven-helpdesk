import { Toaster } from 'sonner';
import { AppRouter } from './routes/Router';
import './index.css';

export default function App() {
  return (
    <>
      <Toaster position="top-right" richColors expand={false} />
      <AppRouter />
    </>
  );
}
