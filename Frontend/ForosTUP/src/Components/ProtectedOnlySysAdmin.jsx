import { Navigate, Outlet } from "react-router-dom";
import { useAuth } from "../context/AuthContext.jsx";
import { toast } from "react-toastify";
import { useEffect } from "react";

const ProtectedOnlySysAdmin = () => {
  const { isSysAdmin } = useAuth();

    useEffect(() => {
      if (!isSysAdmin) {
        toast.warning("Solo SYSADMIN tiene acceso.");
      }
    }, [isSysAdmin]);

   if (!isSysAdmin) {
    return <Navigate to="/main" replace />;
  }

  return <Outlet />;
};

export default ProtectedOnlySysAdmin;
