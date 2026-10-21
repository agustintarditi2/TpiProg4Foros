import { Navigate, Outlet } from "react-router-dom";
import { useAuth } from "../context/AuthContext.jsx";
import { toast } from "react-toastify";
import { useEffect } from "react";


const ProtectedOnlyAdmin = () => {
  const { isAdmin } = useAuth();
  
  useEffect(() => {
    if (!isAdmin) {
      toast.warning("Solo los administradores tienen acceso.");
    }
  }, [isAdmin]);


   if (!isAdmin) {
    // toast.warning("Solo los administradores tienen acceso.");
    return <Navigate to="/main" replace />;
  }

  return <Outlet />;
};

export default ProtectedOnlyAdmin;
