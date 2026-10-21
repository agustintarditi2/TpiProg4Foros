
import { createContext, useContext, useEffect, useState } from "react";
import { Spinner } from "react-bootstrap";
import { toast } from "react-toastify";

const AuthContext = createContext();

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [token, setToken] = useState(null);
  const [isAuthenticated, setIsAuthenticated] = useState(false); // constante -> estado que puede cambiar
  const [loading, setLoading] = useState(true); // para que no cambie de golpe

  // Abrimos un useEffect para que vuelva a ejecutarse cada vez que se cambia uno de los estados.
  useEffect(() => {
    const storedUser = localStorage.getItem("user");
    const storedToken = localStorage.getItem("token");
	// Si hay un token y usuario, se verifica con el backend.
    if (storedToken && storedUser) {
      verifyTokenWithBackend(storedToken)
        .then((userData) => {
	// si no hay error, pasa isAuthenticated a true y se guarda el valor de localStorage al estado del contexto.
          setUser(userData); 
          setToken(storedToken);
          setIsAuthenticated(true);
        })
        .catch((error) => {
	// Si hay errores, levanta un toast, pasa isAuthenticated a false y vacía el localStorage.
          toast.error(error.message || "Sesión expirada, inicia sesión nuevamente.");
          localStorage.removeItem("user");
          localStorage.removeItem("token");
          setIsAuthenticated(false);
        })
        .finally(() => setLoading(false)); // De cualquier modo, se desactiva el estado de loading.
    } else {
      setLoading(false);
    }
  }, []);
	// el verify, que es un fetch sencillo. No se exporta porque solo se uza adentro del contexto.
  const verifyTokenWithBackend = async (token) => {
    const res = await fetch("http://localhost:3000/token", {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });
    if (!res.ok) {
      const err = await res.json();
      throw new Error(err.message || "Token inválido");
    }
    const data = await res.json();
    return data.user; 
  };
	
  const login = (userData, userToken) => {
    localStorage.setItem("user", JSON.stringify(userData));
    localStorage.setItem("token", userToken);
    setUser(userData);
    setToken(userToken);
    setIsAuthenticated(true); // isAuthenticated es true si se hizo login.
  };

  const logout = () => {
    localStorage.removeItem("user");
    localStorage.removeItem("token");
    setUser(null);
    setToken(null);
    setIsAuthenticated(false); // isAuthenticated es false si se hizo logout.
  };

  const isAdmin = user?.role === "ADMIN" || user?.role === "SYSADMIN";
  const isSysAdmin = user?.role === "SYSADMIN";

  // Algo para mostrar cuando está loading.
  if (loading) {
    return <Spinner /> 
  }

  return (
    <AuthContext.Provider
      value={{
        user,
        token,
        isAuthenticated,
        isAdmin,
        isSysAdmin,
        login,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => useContext(AuthContext);