import { Navigate, Outlet } from "react-router-dom";
import { useState, useEffect } from "react";
import { toast } from "react-toastify";
import { useAuth } from "../context/AuthContext.jsx";
import { Spinner } from "react-bootstrap";

const Protected = () => {
  const { isAuthenticated } = useAuth();
  const [redirect, setRedirect] = useState(false);


  useEffect(() => {
    
    if (!isAuthenticated) {
      toast.warning("Debe iniciar sesión para realizar esta acción. Será redirigido prontamente.", {
      toastId: "protected-login-required",
    });
      const timer = setTimeout(() => {
        setRedirect(true);
      }, 3500);
      return () => clearTimeout(timer);
    }}, [isAuthenticated])
  
  if (!isAuthenticated && !redirect) {
    return <div style={{ padding: "2rem", textAlign: "center" }}>
      <Spinner animation="border" variant="success" />
      <p className="mt-2">Redirigiendo...</p>
    </div>};

  if (!isAuthenticated && redirect) {
    return <Navigate to="/login" replace />;
  };

  return <Outlet />;
};

export default Protected;
