
import { useCallback, useEffect, useState } from "react";
import { Alert, Button, Table, Modal, Badge } from "react-bootstrap";
import { useAuth } from "../context/AuthContext.jsx";
import { toast } from "react-toastify";
import EditBan from "./BanModification.jsx";
import "./Bans.css";

const Bans = () => {
  const { token } = useAuth();

  const [showEditModal, setShowEditModal] = useState(false);
  const [selectedBan, setSelectedBan] = useState(null);

  const [bans, setBans] = useState([]);
  const [banLoadingId, setBanLoadingId] = useState(null);
  const [error, setError] = useState("");

  // Carga los baneos desde el backend.
  const cargarBans = useCallback(async () => {
    try {
      setError("");

      const response = await fetch("http://localhost:5073/bans", {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      const data = await response.json();

      if (!response.ok) {
        throw new Error(data.message || "No se pudieron cargar los baneos");
      }

      setBans(data);
    } catch (error) {
      setError(error.message);
      setBans([]);
    }
  }, [token]);

  useEffect(() => {
    if (!token) return;

    cargarBans();
  }, [cargarBans, token]);

  // Desbanea al usuario.
  const desban = async (ban) => {
    try {
      setBanLoadingId(ban.id);

      const response = await fetch(`http://localhost:5073/bans/${ban.id}`, {
        method: "PATCH",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({
          status: "desbaneado",
        }),
      });

      const data = await response.json();

      if (!response.ok) {
        throw new Error(data.message || "Error al actualizar el ban");
      }

      // Actualiza el estado visual sin volver a cargar toda la lista.
      setBans((prevBans) =>
        prevBans.map((item) =>
          item.id === ban.id
            ? { ...item, status: "desbaneado" }
            : item
        )
      );

      toast.success("Usuario desbaneado correctamente", {
        className: "toast-success-custom",
        progressClassName: "toast-progress-custom",
      });

    } catch (error) {
      toast.error(error.message || "No se pudo desbanear el usuario");
    } finally {
      setBanLoadingId(null);
    }
  };

  const openEditModal = (ban) => {
    setSelectedBan(ban);
    setShowEditModal(true);
  };

  const closeEditModal = () => {
    setShowEditModal(false);
    setSelectedBan(null);
  };

  // Formatea las fechas recibidas del backend.
  const formatDate = (date) => {
    if (!date) return "No disponible";

    return new Date(date).toLocaleDateString("es-AR");
  };

  return (
    <main className="bans-page">
      <div className="container mt-4">
        <h1 className="mb-4" style={{ color: "green" }}>
          Lista de baneos
        </h1>

        {error && (
          <Alert variant="danger">
            {error}
          </Alert>
        )}

        <Table striped bordered hover>
          <thead className="table-success">
            <tr>
              <th>Usuario (ID)</th>
              <th>Admin (ID)</th>
              <th>Motivo</th>
              <th>Fecha de baneo</th>
              <th>Fecha de finalización</th>
              <th>Duración</th>
              <th>Estado</th>
              <th>Acción</th>
            </tr>
          </thead>

          <tbody>
            {bans.map((item) => (
              <tr key={item.id}>
                <td>{item.userId}</td>

                <td>{item.adminId}</td>

                <td>{item.reason}</td>

                <td>{formatDate(item.startDate)}</td>

                <td>{formatDate(item.endDate)}</td>

                <td>
                  {item.duration === 1
                    ? "1 día"
                    : `${item.duration} días`}
                </td>

                <td>
                  <Badge
                    bg={
                      item.status === "activo"
                        ? "success"
                        : item.status === "desbaneado"
                          ? "secondary"
                          : "danger"
                    }
                  >
                    {item.status}
                  </Badge>
                </td>

                <td>
                  <Button
                    size="sm"
                    onClick={() => openEditModal(item)}
                    className="me-2 btn btn-success"
                  >
                    Modificar
                  </Button>

                  {item.status === "activo" ? (
                    <Button
                      size="sm"
                      onClick={() => desban(item)}
                      disabled={banLoadingId === item.id}
                      className="me-2 btn btn-outline-danger btn btn-light"
                    >
                      {banLoadingId === item.id
                        ? "Desbaneando..."
                        : "Desbanear"}
                    </Button>
                  ) : (
                    <span className="text-muted me-2">
                      Ban inactivo
                    </span>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </Table>
      </div>

      <Modal
        show={showEditModal}
        onHide={closeEditModal}
        centered
      >
        <Modal.Body>
          {selectedBan && (
            <EditBan
              ban={selectedBan}
              onClose={closeEditModal}
              onUpdated={cargarBans}
            />
          )}
        </Modal.Body>
      </Modal>
    </main>
  );
};

export default Bans;
