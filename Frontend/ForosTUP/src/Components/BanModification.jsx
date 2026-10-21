import { useState } from "react";
import "./Bans.css";
import { Alert, Button, Card, Col, Form, FormGroup, Row } from "react-bootstrap";
import { useAuth } from "../context/AuthContext.jsx";


const EditBan = ({ ban, onClose, onUpdated }) => {
    const { token } = useAuth();
    const [formData, setFormData] = useState({
        reason: ban.reason,
        duration: ban.duration
    });
    const [error, setError] = useState("");
    const [success, setSuccess] = useState("");
    const [loading, setLoading] = useState(false);

    const handleChange = (e) => {
        const { name, value } = e.target;

        setFormData(prev => ({
            ...prev,
            [name]: value
        }));
        setError("");
        setSuccess("");
    };
    const handleSubmit = async (e) => {
        e.preventDefault();
        if (!formData.reason.trim()) {
            setError("El motivo del baneo es obligatorio.");
            return;
        }

        if (!formData.duration || Number(formData.duration) <= 0) {
            setError("La duración debe ser mayor a 0 días.");
            return;
        }
        try {
            setLoading(true);
            setError("");
            setSuccess("");

            const response = await fetch(`http://localhost:3000/bans/${ban.id}`, {
                method: "PATCH",
                headers: {
                    "Content-Type": "application/json",
                    Authorization: `Bearer ${token}`,
                },
                body: JSON.stringify({
                    duration: Number(formData.duration),
                    reason: formData.reason.trim(),
                }),
            })
            const data = await response.json();

            if (!response.ok) {
                throw new Error(data.message || "Error al modificar el ban");
            }

            setSuccess("Ban modificado correctamente.");
            await onUpdated();
            setTimeout(() => {
                onClose();
            }, 800);
        } catch (error) {
            setError(error.message || "Error conectando con el servidor");
        } finally {
            setLoading(false);
        }
    }
    return (
        <div>
            <Card className="p-4 shadow bg-success text-light">
                <Card.Body>
                    <h2 className="text-center mb-4">Estas modificando el baneo de {ban.bannedUser?.name}</h2>
                    <h2 className="text-center mb-4">Ban Nº:{ban.id}</h2>
                    {error && (
                        <Alert variant="danger">
                            {error}
                        </Alert>
                    )}

                    {success && (
                        <Alert variant="success">
                            {success}
                        </Alert>
                    )}
                    <Form onSubmit={handleSubmit}>
                        <FormGroup className="mb-3">
                            <Form.Label>Motivo</Form.Label>

                            <Form.Control
                                type="text"
                                name="reason"
                                value={formData.reason}
                                onChange={handleChange}
                                required
                            />
                        </FormGroup>
                        <FormGroup className="mb-3">
                            <Form.Label>Duracion</Form.Label>
                            <br />
                            {ban.status === "activo"? (
                            <Form.Control
                                type="number"
                                name="duration"
                                value={formData.duration}
                                onChange={handleChange}
                                required
                                min="1"
                            />): (<span className="text-muted" className="me-2"> Ban inactivo</span>)}
                        </FormGroup>
                        <Row>
                            <Col md={6}>
                                <Button
                                    variant="danger"
                                    type="submit"
                                    className="w-100"
                                    disabled={loading}
                                >
                                    {loading ? "Modificando" : "Modificar"}
                                </Button>
                            </Col>

                            <Col md={6}>
                                <Button
                                    variant="secondary"
                                    type="button"
                                    className="w-100"
                                    disabled={loading}
                                    onClick={onClose}
                                >
                                    Cancelar
                                </Button>
                            </Col>
                        </Row>
                    </Form>
                </Card.Body>
            </Card>
        </div>
    )
}

export default EditBan;