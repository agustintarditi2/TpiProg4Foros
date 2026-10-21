import { useState } from "react";
import { useNavigate, useParams, Link } from "react-router-dom";
import { Container, Card, Form, Button, Alert } from "react-bootstrap";
import { toast } from "react-toastify";
import { useAuth } from "../context/AuthContext.jsx";
import "./NewPost.css";

const NewPost = () => {
  const { forumId } = useParams();
  const navigate = useNavigate();
  const { token } = useAuth();

  const [form, setForm] = useState({
    title: "",
    body: "",
  });

  const [errors, setErrors] = useState({
    title: "",
    body: "",
  });

  const [errorGeneral, setErrorGeneral] = useState("");
  const [loading, setLoading] = useState(false);

  const validateForm = () => {
    const newErrors = {
      title: "",
      body: "",
    };

    if (!form.title.trim()) {
      newErrors.title = "El título es obligatorio";
    } else if (form.title.trim().length < 5) {
      newErrors.title = "El título debe tener al menos 5 caracteres";
    }

    if (!form.body.trim()) {
      newErrors.body = "El contenido de la publicación es obligatorio";
    } else if (form.body.trim().length < 10) {
      newErrors.body = "La publicación debe tener al menos 10 caracteres";
    }

    setErrors(newErrors);

    return !newErrors.title && !newErrors.body;
  };

  const handleChange = (event) => {
    const { name, value } = event.target;

    setForm({
      ...form,
      [name]: value,
    });

    setErrors({
      ...errors,
      [name]: "",
    });

    setErrorGeneral("");
  };

  const handleSubmit = async (event) => {
    event.preventDefault();

    setErrorGeneral("");

    const isValid = validateForm();

    if (!isValid) {
      return;
    }

    setLoading(true);

    try {
      const response = await fetch(
        `http://localhost:3000/forums/${forumId}/posts`,
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${token}`,
          },
          body: JSON.stringify(form),
        }
      );

      const data = await response.json();

      if (!response.ok) {
        throw new Error(
          data.message || data.error || "No se pudo crear la publicación"
        );
      }
      toast.success("Publicación creada correctamente.", {
        className: "toast-success-custom",
        progressClassName: "toast-progress-custom",
    });

      navigate(`/post/${data.id}`);
    } catch (error) {
      setErrorGeneral(error.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <main className="new-post-page">
      <Container>
        <Card className="new-post-card">
          <Card.Body>
            <div className="new-post-header">
              <div className="new-post-breadcrumb">
                <Link to="/foros">Foros</Link>
                <span>{">"}</span>
                <Link to={`/forum/${forumId}`}>Foro</Link>
                <span>{">"}</span>
                <span>Nueva publicación</span>
              </div>

              <span className="new-post-badge">Publicación</span>

              <h1>Crear nueva publicación</h1>

              <p>
                Escribí un título claro y desarrollá el contenido para iniciar
                una conversación dentro del foro.
              </p>
            </div>

            {errorGeneral && (
              <Alert variant="danger" className="new-post-alert">
                {errorGeneral}
              </Alert>
            )}

            <Form onSubmit={handleSubmit} noValidate>
              <Form.Group className="mb-3" controlId="title">
                <Form.Label className="new-post-label">Título</Form.Label>

                <Form.Control
                  className="new-post-input"
                  type="text"
                  name="title"
                  placeholder="Ej: Duda sobre React Router"
                  value={form.title}
                  onChange={handleChange}
                  isInvalid={!!errors.title}
                />

                <Form.Control.Feedback type="invalid" className="new-post-error">
                  {errors.title}
                </Form.Control.Feedback>
              </Form.Group>

              <Form.Group className="mb-4" controlId="body">
                <Form.Label className="new-post-label">Contenido</Form.Label>

                <Form.Control
                  className="new-post-textarea"
                  as="textarea"
                  rows={7}
                  name="body"
                  placeholder="Escribí el contenido de tu publicación..."
                  value={form.body}
                  onChange={handleChange}
                  isInvalid={!!errors.body}
                />

                <Form.Control.Feedback type="invalid" className="new-post-error">
                  {errors.body}
                </Form.Control.Feedback>
              </Form.Group>

              <div className="new-post-actions">
                <Button
                  variant="outline-secondary"
                  type="button"
                  onClick={() => navigate(`/forum/${forumId}`)}
                >
                  Cancelar
                </Button>

                <Button
                  variant="success"
                  type="submit"
                  disabled={loading}
                  className="new-post-submit"
                >
                  {loading ? "Publicando..." : "Crear publicación"}
                </Button>
              </div>
            </Form>
          </Card.Body>
        </Card>
      </Container>
    </main>
  );
};

export default NewPost;