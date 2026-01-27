// Library references
import { Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './state/AuthContext.jsx';
import { ToastProvider } from './state/ToastContext.jsx';

// Custom components
import Footer from './components/Footer.jsx'
import Header from './components/Header.jsx';
import RequireAuth from './components/RequireAuth.jsx';

// Pages
import LoginScreen from './pages/LoginScreen.jsx'
import RegisterScreen from './pages/RegisterScreen.jsx';
import Dashboard from '../../mek-tasks/src/pages/Dashboard.jsx';

function App() {

  return (
    <ToastProvider>
      <AuthProvider>
        <Header/>
        <Routes>
          <Route path="/" element={<Navigate to="/app" />} />
          <Route path="/app" element={<RequireAuth><Dashboard /></RequireAuth>} />
          <Route path="/login" element={<LoginScreen />} />
          <Route path="/register" element={<RegisterScreen />} />
          <Route path="*" element={<Navigate to="/app" />} />
        </Routes>
        <Footer />
      </AuthProvider>
    </ToastProvider>
  )
}

export default App
