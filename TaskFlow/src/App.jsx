import { AuthProvider } from './state/AuthContext.jsx';
import { Routes, Route, Navigate } from 'react-router-dom';
import Footer from './components/Footer.jsx'
import LoginScreen from './pages/LoginScreen.jsx'
import Header from './components/Header.jsx';
import Dashboard from '../../mek-tasks/src/pages/Dashboard.jsx';
import RegisterScreen from './pages/RegisterScreen.jsx';
import RequireAuth from './components/RequireAuth.jsx';

function App() {

  return (
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
  )
}

export default App
