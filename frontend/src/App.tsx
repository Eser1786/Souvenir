import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './context';
import { LoginScreen } from './components/Login/LoginScreen';
import { PosScreen } from './components/Pos/PosScreen';
import { CustomerManagementScreen } from './components/Customer/CustomerManagementScreen';
import { ReturnsScreen } from './components/Returns/ReturnsScreen';
import { UsersManagementScreen } from './components/UsersManagement/UsersManagementScreen';
import { ReportDashboardScreen } from './components/Reports/ReportDashboardScreen';

const ProtectedRoute: React.FC<{ children: React.ReactElement }> = ({ children }) => {
  const { isAuthenticated } = useAuth();
  return isAuthenticated ? children : <Navigate to="/login" replace />;
};

const ManagerRoute: React.FC<{ children: React.ReactElement }> = ({ children }) => {
  const { user, isAuthenticated } = useAuth();
  if (!isAuthenticated) return <Navigate to="/login" replace />;
  const role = user?.vaiTro?.toLowerCase() || '';
  const isManager = role.includes('quản lý') || role.includes('admin') || role.includes('quanly');
  return isManager ? children : <Navigate to="/pos" replace />;
};

const PublicRoute: React.FC<{ children: React.ReactElement }> = ({ children }) => {
  const { isAuthenticated } = useAuth();
  return isAuthenticated ? <Navigate to="/pos" replace /> : children;
};

export const AppContent: React.FC = () => {
  return (
    <Routes>
      <Route
        path="/login"
        element={
          <PublicRoute>
            <LoginScreen />
          </PublicRoute>
        }
      />
      <Route
        path="/pos"
        element={
          <ProtectedRoute>
            <PosScreen />
          </ProtectedRoute>
        }
      />
      <Route
        path="/customers"
        element={
          <ProtectedRoute>
            <CustomerManagementScreen />
          </ProtectedRoute>
        }
      />
      <Route
        path="/returns"
        element={
          <ProtectedRoute>
            <ReturnsScreen />
          </ProtectedRoute>
        }
      />
      <Route
        path="/reports"
        element={
          <ManagerRoute>
            <ReportDashboardScreen />
          </ManagerRoute>
        }
      />
      <Route
        path="/users"
        element={
          <ManagerRoute>
            <UsersManagementScreen />
          </ManagerRoute>
        }
      />
      <Route path="/dashboard" element={<Navigate to="/reports" replace />} />
      <Route path="*" element={<Navigate to="/pos" replace />} />
    </Routes>
  );
};

export function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <AppContent />
      </AuthProvider>
    </BrowserRouter>
  );
}

export default App;