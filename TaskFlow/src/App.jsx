import { useState } from 'react'
import { AuthProvider } from './state/AuthContext.jsx';
import Footer from './components/Footer.jsx'
import LoginScreen from './components/LoginScreen.jsx'
import Header from './components/Header.jsx';

function App() {
  const [loggedIn, setLoggedIn] = useState(false)

  function onLogin() {
    setLoggedIn(prevState => !prevState);
  }

  return (
    <AuthProvider>
      <Header onClick={onLogin} userState={loggedIn} />
      <LoginScreen onClick={onLogin} userState={loggedIn}/>
      <Footer />
    </AuthProvider>
  )
}

export default App
