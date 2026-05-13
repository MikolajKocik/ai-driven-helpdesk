import React, { lazy, Suspense } from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import { useAuth } from '@/providers/AuthProvider';
import AuthPage from '@/pages/Auth/Page';
import HelpdeskPage from '@/pages/Helpdesk/Page';

const AdminArticlesPage = lazy(() => import('@/pages/Admin/Articles/Page'));
const AdminDashboardPage = lazy(() => import('@/pages/Admin/Dashboard/Page'));

const ProtectedRoute = ({ children }: { children: React.ReactNode }) => {
  const { isAuthenticated } = useAuth();
  if (!isAuthenticated) return <Navigate to="/auth" />;
  return <>{children}</>;
};

const PublicRoute = ({ children }: { children: React.ReactNode }) => {
  const { isAuthenticated } = useAuth();
  if (isAuthenticated) return <Navigate to="/helpdesk" />;
  return <>{children}</>;
};

export const AppRouter: React.FC = () => {
  return (
    <Suspense fallback={<div>Loading...</div>}>
      <Routes>
        <Route path="/auth" element={
            <PublicRoute>
              <AuthPage />
            </PublicRoute>
          } 
        />
        <Route path="/helpdesk" element={
            <ProtectedRoute>
              <HelpdeskPage />
            </ProtectedRoute>
          } 
        />
        <Route path="/admin/dashboard" element={
            <ProtectedRoute>
              <AdminDashboardPage />
            </ProtectedRoute>
          } 
        />
        <Route path="/admin/articles" element={
            <ProtectedRoute>
              <AdminArticlesPage />
            </ProtectedRoute>
          } 
        />
        <Route path="/" element={<Navigate to="/helpdesk" />} />
      </Routes>
    </Suspense>
  );
};
