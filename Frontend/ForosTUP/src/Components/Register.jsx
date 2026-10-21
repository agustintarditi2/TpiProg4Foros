import "./LogIn.css";
import { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { Container, Row, Col, Card, Form, Button, Alert } from "react-bootstrap";

const Register = () => {
  const navigate = useNavigate();

  const [form, setForm] = useState({
    nick: "",
    nombre: "",
    correo: "",
    edad: "",
    password: "",
    confirmPassword: "",
  });

  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [loading, setLoading] = useState(false);

  const handleChange = (e) => {
    setForm({
      ...form,
      [e.target.name]: e.target.value,
    });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    setError("");
    setSuccess("");
    setLoading(true);

    if (form.password !== form.confirmPassword) {
      setError("Las contraseñas no coinciden");
      setLoading(false);
      return;
    }

    try {
      const response = await fetch("http://localhost:3000/register", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          nick: form.nick,
          nombre: form.nombre,
          correo: form.correo,
          edad: Number(form.edad),
          password: form.password,
        }),
      });

      const data = await response.json();

      if (!response.ok) {
        throw new Error(data.message || "Error al registrar usuario");
      }

      setSuccess("Usuario registrado correctamente");

      setTimeout(() => {
        navigate("/login");
      }, 1500);

    } catch (error) {
      setError(error.message);
    } finally {
    setLoading(false);
  }
  };

  return (
    <main className="login-page login-page-light">
      <Container>
        <Row className="justify-content-center">
          <Col xs={12} sm={10} md={8} lg={6} xl={5}>
            <Card className="login-card">
              <Card.Body className="login-card-body">
                <div className="login-header">
                  <div className="login-badge">Foro TUP</div>

                  <h2 className="login-title">Crear cuenta</h2>

                  <p className="login-subtitle">
                    Unite al Gran Foro TUP y empezá a participar
                  </p>
                </div>

                {error && (
                  <Alert variant="danger" className="login-alert">
                    {error}
                  </Alert>
                )}

                {success && (
                  <Alert variant="success" className="login-alert">
                    {success}
                  </Alert>
                )}

                <Form onSubmit={handleSubmit}>
                  <Form.Group className="mb-3">
                    <Form.Label className="login-label">Nick</Form.Label>
                    <Form.Control
                      className="login-input"
                      type="text"
                      name="nick"
                      value={form.nick}
                      onChange={handleChange}
                      placeholder="Ej: Juan123"
                      autoComplete="username"
                      required
                    />
                  </Form.Group>

                  <Form.Group className="mb-3">
                    <Form.Label className="login-label">Nombre</Form.Label>
                    <Form.Control
                      className="login-input"
                      type="text"
                      name="nombre"
                      value={form.nombre}
                      onChange={handleChange}
                      placeholder="Ingresá tu nombre"
                      autoComplete="name"
                      required
                    />
                  </Form.Group>

                  <Form.Group className="mb-3">
                    <Form.Label className="login-label">Correo electrónico</Form.Label>
                    <Form.Control
                      className="login-input"
                      type="email"
                      name="correo"
                      value={form.correo}
                      onChange={handleChange}
                      placeholder="ejemplo@mail.com"
                      autoComplete="email"
                      required
                    />
                  </Form.Group>

                  <Form.Group className="mb-3">
                    <Form.Label className="login-label">Edad</Form.Label>
                    <Form.Control
                      className="login-input"
                      type="number"
                      name="edad"
                      min="13"
                      value={form.edad}
                      onChange={handleChange}
                      placeholder="13+"
                      required
                    />
                  </Form.Group>

                  <Form.Group className="mb-3">
                    <Form.Label className="login-label">Contraseña</Form.Label>
                    <Form.Control
                      className="login-input"
                      type="password"
                      name="password"
                      value={form.password}
                      onChange={handleChange}
                      placeholder="Creá una contraseña"
                      autoComplete="new-password"
                      required
                    />
                  </Form.Group>

                  <Form.Group className="mb-4">
                    <Form.Label className="login-label">Confirmar contraseña</Form.Label>
                    <Form.Control
                      className="login-input"
                      type="password"
                      name="confirmPassword"
                      value={form.confirmPassword}
                      onChange={handleChange}
                      placeholder="Repetí tu contraseña"
                      autoComplete="new-password"
                      required
                    />
                  </Form.Group>

                  <Button
                    type="submit"
                    className="login-button"
                    disabled={loading}
                  >
                    {loading ? "Registrando..." : "Crear cuenta"}
                  </Button>
                </Form>

                <div className="login-footer">
                  <span>¿Ya tenés cuenta?</span>{" "}
                  <Link to="/login">Iniciar sesión</Link>
                </div>
              </Card.Body>
            </Card>
          </Col>
        </Row>
      </Container>
    </main>
  );
};

export default Register;