import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Container, Form, Button, Alert, Card } from "react-bootstrap";
import { useAuth } from "../context/AuthContext.jsx";
import "./NewForum.css";

const NewForum = () => {
  const navigate = useNavigate();

  const [form, setForm] = useState({
    name: "",
    desc: "",
    rules: "",
  });

  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [loading, setLoading] = useState(false);

  const { token, isAdmin } = useAuth();

  const handleChange = (event) => {
    setForm({
      ...form,
      [event.target.name]: event.target.value,
    });
  };

  const handleSubmit = async (event) => {
    event.preventDefault();

    setError("");
    setSuccess("");

    if (!isAdmin) {
      setError("No tenés permisos para crear foros.");
      return;
    }

    if (!form.name.trim() || !form.desc.trim()) {
      setError("El nombre y la descripción son obligatorios.");
      return;
    }

    try {
      setLoading(true);

      const response = await fetch("http://localhost:3000/forums", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify(form),
      });

      const data = await response.json();

      if (!response.ok) {
        throw new Error(data.message || data.error || "No se pudo crear el foro");
      }

      setSuccess("Foro creado correctamente.");

      setTimeout(() => {
        navigate("/foros");
      }, 1000);
    } catch (error) {
      setError(error.message);
    } finally {
      setLoading(false);
    }
  };

  if (!isAdmin) {
    return (
      <main className="new-forum-page">
        <Container>
          <Alert variant="danger">
            No tenés permisos para crear foros.
          </Alert>
        </Container>
      </main>
    );
  }

  return (
    <main className="new-forum-page">
      <Container>
        <Card className="new-forum-card">
          <Card.Body>
            <div className="new-forum-header">
              <span className="new-forum-badge">Administración</span>

              <h1>Crear nuevo foro</h1>

              <p>
                Completá los datos principales para crear un nuevo espacio de
                conversación dentro de la comunidad.
              </p>
            </div>

            {error && <Alert variant="danger">{error}</Alert>}
            {success && <Alert variant="success">{success}</Alert>}

            <Form onSubmit={handleSubmit}>
              <Form.Group className="mb-3">
                <Form.Label>Nombre del foro</Form.Label>
                <Form.Control
                  type="text"
                  name="name"
                  placeholder="Ej: Programación, Base de Datos, General..."
                  value={form.name}
                  onChange={handleChange}
                />
              </Form.Group>

              <Form.Group className="mb-3">
                <Form.Label>Descripción</Form.Label>
                <Form.Control
                  as="textarea"
                  rows={3}
                  name="desc"
                  placeholder="Describí brevemente de qué trata este foro..."
                  value={form.desc}
                  onChange={handleChange}
                />
              </Form.Group>

              <Form.Group className="mb-4">
                <Form.Label>Reglas del foro</Form.Label>
                <Form.Control
                  as="textarea"
                  rows={4}
                  name="rules"
                  placeholder="Ej: Respetar a los demás usuarios. No publicar spam. Usar títulos descriptivos."
                  value={form.rules}
                  onChange={handleChange}
                />
              </Form.Group>

              <div className="new-forum-actions">
                <Button
                  variant="outline-secondary"
                  type="button"
                  onClick={() => navigate("/foros")}
                >
                  Cancelar
                </Button>

                <Button
                  variant="success"
                  type="submit"
                  disabled={loading}
                  className="new-forum-submit"
                >
                  {loading ? "Creando..." : "Crear foro"}
                </Button>
              </div>
            </Form>
          </Card.Body>
        </Card>
      </Container>
    </main>
  );
};

export default NewForum;