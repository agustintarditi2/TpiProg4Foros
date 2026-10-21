import { useEffect, useRef, useState } from "react";
import { Alert, Button, Card, Col, Form, FormGroup, Row } from "react-bootstrap";
import { useNavigate, useSearchParams } from "react-router-dom";
import { toast } from "react-toastify";
import { useAuth } from "../context/AuthContext.jsx";

const NewBan = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const { token, user } = useAuth();

  const selectedUserId = searchParams.get("userId") || "";
  const selectedUserName = searchParams.get("userName") || "Usuario seleccionado";

  const userIdRef = useRef(null);
  const reasonRef = useRef(null);
  const durationRef = useRef(null);

  const [formData, setFormData] = useState({
    userId: selectedUserId,
    reason: "",
    duration: "",
  });

  const [formErrors, setFormErrors] = useState({
    userId: "",
    reason: "",
    duration: "",
  });

  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  useEffect(() => {
  setFormData((prev) => ({
    ...prev,
    userId: selectedUserId,
  }));

  setFormErrors((prev) => ({
    ...prev,
    userId: "",
  }));
}, [selectedUserId]);

  const handleChange = (e) => {
    const { name, value } = e.target;

    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));

    setFormErrors((prev) => ({
      ...prev,
      [name]: "",
    }));

    setError("");
  };

  const validateForm = () => {
    const newErrors = {
      userId: "",
      reason: "",
      duration: "",
    };

    if (!formData.userId) {
      newErrors.userId = "No se seleccionó ningún usuario para banear.";
    }

    if (!formData.reason.trim()) {
      newErrors.reason = "El motivo del baneo es obligatorio.";
    }

    if (!formData.duration) {
      newErrors.duration = "La duración es obligatoria.";
    } else if (Number(formData.duration) <= 0) {
      newErrors.duration = "La duración debe ser mayor a 0 días.";
    }

    setFormErrors(newErrors);

    if (newErrors.userId) {
      userIdRef.current?.focus();
      return false;
    }

    if (newErrors.reason) {
      reasonRef.current?.focus();
      return false;
    }

    if (newErrors.duration) {
      durationRef.current?.focus();
      return false;
    }

    return true;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

   if (!validateForm()) {
    return;
  }

    try {
      setLoading(true);
      setError("");

      const response = await fetch("http://localhost:3000/bans", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({
        userId: Number(formData.userId),
        reason: formData.reason.trim(),
        duration: Number(formData.duration),
      }),
      });

      const data = await response.json();

      if (!response.ok) {
        throw new Error(data.message || "Error al crear el ban");
      }

      toast.success("Usuario baneado correctamente", {
      className: "toast-success-custom",
      progressClassName: "toast-progress-custom",
    });

      navigate(-1);
      
    } catch (error) {
        const message = error.message || "Error conectando con el servidor";

        setError(message);
        toast.error(message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="d-flex justify-content-center mt-5">
      <Card
        className="p-4 shadow text-light"
        style={{ width: "700px", backgroundColor: "#074621" }}
      >
        <Card.Body>
          <h2 className="text-center mb-4">Nuevo Ban</h2>

          {error && <Alert variant="danger">{error}</Alert>}

          <Form onSubmit={handleSubmit} noValidate>
            <FormGroup className="mb-3">
              <Form.Label>Usuario a banear</Form.Label>

              <Form.Control
                ref={userIdRef}
                type="text"
                value={
                  selectedUserId
                    ? `${selectedUserName} (ID: ${selectedUserId})`
                    : "No se seleccionó ningún usuario"
                }
                readOnly
                isInvalid={!!formErrors.userId}
              />
              <Form.Control.Feedback type="invalid" className="d-block">
                {formErrors.userId}
              </Form.Control.Feedback>
            </FormGroup>

            <FormGroup className="mb-3">
              <Form.Label>Administrador</Form.Label>

              <Form.Control
                type="text"
                value={
                  user?.id
                    ? `${user?.name || "Admin"} (ID: ${user.id})`
                    : "Administrador logueado"
                }
                readOnly
              />
            </FormGroup>

            <FormGroup className="mb-3">
              <Form.Label>Motivo</Form.Label>

              <Form.Control
                ref={reasonRef}
                type="text"
                name="reason"
                value={formData.reason}
                onChange={handleChange}
                placeholder="Ej: comportamiento ofensivo"
                isInvalid={!!formErrors.reason}
              />
              <Form.Control.Feedback type="invalid" className="d-block">
                {formErrors.reason}
              </Form.Control.Feedback>
            </FormGroup>

            <FormGroup className="mb-4">
              <Form.Label>Duración</Form.Label>

              <Form.Control
                ref={durationRef}
                type="number"
                name="duration"
                value={formData.duration}
                onChange={handleChange}
                min="1"
                placeholder="Cantidad de días"
                isInvalid={!!formErrors.duration}
              />
              <Form.Control.Feedback type="invalid" className="d-block">
                {formErrors.duration}
              </Form.Control.Feedback>
            </FormGroup>

            <Row>
              <Col md={6}>
                <Button
                  variant="danger"
                  type="submit"
                  className="w-100"
                  disabled={loading}
                >
                  {loading ? "Baneando..." : "Banear"}
                </Button>
              </Col>

              <Col md={6}>
                <Button
                  variant="secondary"
                  type="button"
                  className="w-100"
                  onClick={() => navigate(-1)}
                >
                  Cancelar
                </Button>
              </Col>
            </Row>
          </Form>
        </Card.Body>
      </Card>
    </div>
  );
};

export default NewBan;