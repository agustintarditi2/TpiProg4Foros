import { useState, useEffect, useRef } from "react";
import {
    Alert,
    Button,
    Card,
    Col,
    Container,
    Form,
    FormGroup,
    Row
} from "react-bootstrap";
import { toast } from "react-toastify";
import { useAuth } from "../context/AuthContext.jsx";
import { useNavigate } from "react-router-dom";
import "./UserEdit.css";

const UserEdit = () => {
    const { token } = useAuth();
    const navigate = useNavigate();

    const nameRef = useRef(null);
    const ageRef = useRef(null);
    const emailRef = useRef(null);
    const currentPasswordRef = useRef(null);
    const passwordRef = useRef(null);

    const [loading, setLoading] = useState(false);
    const [error, setError] = useState("");

    const [fieldErrors, setFieldErrors] = useState({
        name: "",
        age: "",
        email: "",
        currentPassword: "",
        password: "",
    });

    const [formData, setFormData] = useState({
        name: "",
        age: "",
        email: "",
        currentPassword: "",
        password: "",
    });

    useEffect(() => {
        if (!token) return;

        const loadProfile = async () => {
            try {
                setLoading(true);

                const response = await fetch(
                    "http://localhost:3000/persons/me",
                    {
                        headers: {
                            Authorization: `Bearer ${token}`,
                        },
                    }
                );

                const data = await response.json();

                if (!response.ok) {
                    throw new Error(data.message);
                }

                setFormData({
                    name: data.name,
                    age: data.age,
                    email: data.email,
                    currentPassword: "",
                    password: "",
                });

            } catch (error) {
                setError(error.message);
            } finally {
                setLoading(false);
            }
        };

        loadProfile();
    }, [token]);

    const validateForm = () => {
        const errors = {
            name: "",
            age: "",
            email: "",
            currentPassword: "",
            password: "",
        };

        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

        if (!formData.name.trim()) {
            errors.name = "El nombre es obligatorio.";
        }

        if (formData.age === "" || formData.age === null || formData.age === undefined) {
            errors.age = "La edad es obligatoria.";
        } else if (Number.isNaN(Number(formData.age)) || Number(formData.age) <= 13) {
            errors.age = "La edad debe ser mayor a 13.";
        }

        if (!formData.email.trim()) {
            errors.email = "El correo electrónico es obligatorio.";
        } else if (!emailRegex.test(formData.email)) {
            errors.email = "Ingresá un correo electrónico válido.";
        }

         if (formData.currentPassword.trim() !== "" && formData.password.trim() === "") {
        errors.password = "Ingresá una nueva contraseña.";
        }

        if (formData.password.trim() !== "") {
            if (!formData.currentPassword.trim()) {
                errors.currentPassword = "Ingresá tu contraseña actual.";
            }

            if (formData.password.trim().length < 6) {
                errors.password = "La contraseña debe tener al menos 6 caracteres.";
            }
        }

        setFieldErrors(errors);

        if (errors.name) {
            nameRef.current.focus();
            return false;
        }

        if (errors.age) {
            ageRef.current.focus();
            return false;
        }

        if (errors.email) {
            emailRef.current.focus();
            return false;
        }
        
        if (errors.currentPassword) {
            currentPasswordRef.current.focus();
            return false;
            }

        if (errors.password) {
            passwordRef.current.focus();
            return false;
        }

        return true;
    };

    const handleChange = (e) => {
        const { name, value } = e.target;

        setFormData(prev => ({
            ...prev,
            [name]: value,
        }));

        setFieldErrors(prev => ({
            ...prev,
            [name]: "",
        }));

        setError("");
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        if (!validateForm()) {
            return;
        }

        try {
            setLoading(true);
            setError("");

            const body = {
                name: formData.name,
                age: Number(formData.age),
                email: formData.email,
            };

            if (formData.password.trim() !== "") {
                body.currentPassword = formData.currentPassword;
                body.password = formData.password;
            }

            const response = await fetch(
                "http://localhost:3000/persons/me",
                {
                    method: "PATCH",
                    headers: {
                        "Content-Type": "application/json",
                        Authorization: `Bearer ${token}`,
                    },
                    body: JSON.stringify(body),
                }
            );

            const data = await response.json();

            if (!response.ok) {
                if (response.status === 401 && data.message === "La contraseña actual es incorrecta.") {
                    setFieldErrors(prev => ({
                        ...prev,
                        currentPassword: "Contraseña actual incorrecta."
                    }));

                    currentPasswordRef.current.focus();
                    return;
                }

                throw new Error(data.message);
            }

            toast.success("Perfil actualizado correctamente.", {
                className: "toast-success-custom",
                progressClassName: "toast-progress-custom",
            });

            setFormData(prev => ({
                ...prev,
                currentPassword: "",
                password: "",
            }));
            
            setTimeout(() => {
                navigate(-1);
            }, 800);

        } catch (error) {
            setError(error.message || "Error conectando con el servidor");

        } finally {
            setLoading(false);
        }
    };

    return (
        <Container className="py-5">
            <Row className="justify-content-center">
                <Col xs={12} sm={10} md={8} lg={6} xl={5}>
                    <Card className="p-4 shadow text-light user-edit-card">
                        <Card.Body>

                            <h2 className="text-center mb-4">
                                Editar perfil
                            </h2>

                            {error && (
                                <Alert variant="danger">
                                    {error}
                                </Alert>
                            )}

                            <Form onSubmit={handleSubmit} noValidate>

                                <FormGroup className="mb-3">
                                    <Form.Label>Nombre</Form.Label>

                                    <Form.Control
                                        ref={nameRef}
                                        type="text"
                                        name="name"
                                        value={formData.name}
                                        onChange={handleChange}
                                        isInvalid={!!fieldErrors.name}
                                        required
                                    />

                                    <Form.Control.Feedback type="invalid" className="user-edit-error">
                                        {fieldErrors.name}
                                    </Form.Control.Feedback>
                                </FormGroup>

                                <FormGroup className="mb-3">
                                    <Form.Label>Edad</Form.Label>

                                    <Form.Control
                                        ref={ageRef}
                                        type="number"
                                        name="age"
                                        value={formData.age}
                                        onChange={handleChange}
                                        min="14"
                                        isInvalid={!!fieldErrors.age}
                                        required
                                    />

                                    <Form.Control.Feedback type="invalid" className="user-edit-error">
                                        {fieldErrors.age}
                                    </Form.Control.Feedback>
                                </FormGroup>

                                <FormGroup className="mb-3">
                                    <Form.Label>Correo electrónico</Form.Label>

                                    <Form.Control
                                        ref={emailRef}
                                        type="email"
                                        name="email"
                                        value={formData.email}
                                        onChange={handleChange}
                                        isInvalid={!!fieldErrors.email}
                                        required
                                    />

                                    <Form.Control.Feedback type="invalid" className="user-edit-error">
                                        {fieldErrors.email}
                                    </Form.Control.Feedback>
                                </FormGroup>
                                <FormGroup className="mb-3">
                                    <Form.Label>Contraseña actual</Form.Label>

                                    <Form.Control
                                        ref={currentPasswordRef}
                                        type="password"
                                        name="currentPassword"
                                        value={formData.currentPassword}
                                        onChange={handleChange}
                                        placeholder="Ingresá tu contraseña actual"
                                        isInvalid={!!fieldErrors.currentPassword}
                                    />

                                    <Form.Control.Feedback type="invalid" className="user-edit-error">
                                        {fieldErrors.currentPassword}
                                    </Form.Control.Feedback>
                                </FormGroup>
                                <FormGroup className="mb-4">
                                    <Form.Label>Nueva contraseña</Form.Label>

                                    <Form.Control
                                        ref={passwordRef}
                                        type="password"
                                        name="password"
                                        value={formData.password}
                                        onChange={handleChange}
                                        placeholder="Dejar vacío para mantener la actual"
                                        isInvalid={!!fieldErrors.password}
                                    />

                                    <Form.Control.Feedback type="invalid" className="user-edit-error">
                                        {fieldErrors.password}
                                    </Form.Control.Feedback>
                                </FormGroup>

                                <Row>
                                    <Col md={6} className="mb-2 mb-md-0">
                                        <Button
                                            variant="primary"
                                            type="submit"
                                            className="w-100"
                                            disabled={loading}
                                        >
                                            {loading
                                                ? "Guardando..."
                                                : "Guardar cambios"}
                                        </Button>
                                    </Col>

                                    <Col md={6}>
                                        <Button
                                            variant="secondary"
                                            type="button"
                                            className="w-100"
                                            onClick={() => navigate("/")}
                                            disabled={loading}
                                        >
                                            Cancelar
                                        </Button>
                                    </Col>
                                </Row>

                            </Form>

                        </Card.Body>
                    </Card>
                </Col>
            </Row>
        </Container>
    );
};

export default UserEdit;