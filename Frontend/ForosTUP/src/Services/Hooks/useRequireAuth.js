import { useNavigate, useLocation } from "react-router-dom";
import { useAuth } from "../../context/AuthContext.jsx";
import { useRef, useEffect, useState } from "react";
import { toast } from "react-toastify";

export const useRequireAuth = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { isAuthenticated } = useAuth();
  const timerRef = useRef(null);
  const [redirecting, setRedirecting] = useState(false)

  useEffect(() => {
    return () => {
      if (timerRef.current) {
        clearTimeout(timerRef.current);
        timerRef.current = null;
      }
      setRedirecting(false)
    };
  }, []);

  const requireAuth = (callback) => {
    if (timerRef.current) {
      clearTimeout(timerRef.current);
      timerRef.current = null;
    }

    if (!isAuthenticated) {
      setRedirecting(true)
      toast.warning(
        "Debe iniciar sesión para realizar esta acción."
      );
      
      timerRef.current = setTimeout(() => {
        navigate("/login", {
          state: { from: location.pathname },
        });
      }, 1000);
      return; 
    }

    setRedirecting(false)
    callback();
  };

  return { requireAuth, redirecting };
};