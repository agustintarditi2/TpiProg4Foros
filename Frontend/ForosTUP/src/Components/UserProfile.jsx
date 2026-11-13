
import { useState } from "react";
import {
    Alert,
    Button,
    Card,
    Col,
    Container,
    Form,
    Row
} from "react-bootstrap";
import { toast } from "react-toastify";
import "./UserProfile.css";

const initialData = {
    name: "Juan Pérez",
    nickname: "Juanito23",
    email: "juan@gmail.com",
    birthDate: "15/06/2000",
    DateCreated: "15/06/2010"
};

const UserProfile = () => {
    const [formData, setFormData] = useState(initialData);
    const [savedData, setSavedData] = useState(initialData);

    const [editingFields, setEditingFields] = useState({
        name: false,
        nickname: false,
        email: false
    });

    const [error, setError] = useState("");

    const handleChange = (e) => {
        const { name, value } = e.target;

        setFormData(prev => ({
            ...prev,
            [name]: value
        }));

        setError("");
    };

    const toggleEdit = (field) => {
        setEditingFields(prev => ({
            ...prev,
            [field]: !prev[field]
        }));
    };

    const handleCancel = () => {
        setFormData({ ...savedData });

        setEditingFields({
            name: false,
            nickname: false,
            email: false,
        });

        setError("");
    };

    const handleSubmit = (e) => {
        e.preventDefault();

        if (
            !formData.name.trim() ||
            !formData.nickname.trim() ||
            !formData.email.trim()
        ) {
            setError("Todos los campos son obligatorios.");
            return;
        }

        setSavedData({ ...formData });

        setEditingFields({
            name: false,
            nickname: false,
            email: false
        });

        setError("");

        toast.success("Perfil actualizado correctamente.");
    };

    const renderField = (label, field, type = "text") => (
        <Form.Group className="mb-4">
            <Form.Label className="profile-label">
                {label}
            </Form.Label>

            <Row className="align-items-center">
                <Col>
                    {editingFields[field] ? (
                        <Form.Control
                            type={type}
                            name={field}
                            value={formData[field]}
                            onChange={handleChange}
                            required
                        />
                    ) : (
                        <div className="profile-value">
                            {formData[field]}
                        </div>
                    )}
                </Col>

                <Col xs="auto">
                    <Button
                        type="button"
                        variant="outline-light"
                        size="sm"
                        onClick={() => toggleEdit(field)}
                    >
                        {editingFields[field] ? "Listo" : "Editar"}
                    </Button>
                </Col>
            </Row>
        </Form.Group>
    );

    return (
        <Container className="py-5">
            <Row className="justify-content-center">
                <Col xs={12} md={9} lg={7}>
                    <Card className="profile-card shadow">
                        <Card.Body className="p-4 p-md-5">

                            <h2 className="text-center mb-2">
                                Mi perfil
                            </h2>

                            <p className="text-center profile-subtitle mb-4">
                                Información personal de tu cuenta
                            </p>

                            {error && (
                                <Alert variant="danger">
                                    {error}
                                </Alert>
                            )}

                            <Form onSubmit={handleSubmit}>

                                {renderField("Nombre", "name")}

                                {renderField("Nickname", "nickname")}

                                {renderField(
                                    "Correo electrónico",
                                    "email",
                                    "email"
                                )}

                                <div className="mb-4 birthdate-info">
                                    <span className="birthdate-label">
                                        Fecha de nacimiento
                                    </span>

                                    <p className="birthdate-value">
                                        {formData.birthDate}
                                    </p>

                                    <br />

                                    <span className="birthdate-label">
                                        Fecha de creacion de la cuenta
                                    </span>

                                    <p className="creatdate-value">
                                        {formData.DateCreated}
                                    </p>
                                </div>

                                <hr className="profile-divider" />

                                <Row className="mt-4">
                                    <Col md={6} className="mb-2 mb-md-0">
                                        <Button
                                            type="button"
                                            variant="secondary"
                                            className="w-100"
                                            onClick={handleCancel}
                                        >
                                            Cancelar
                                        </Button>
                                    </Col>

                                    <Col md={6}>
                                        <Button
                                            type="submit"
                                            variant="success"
                                            className="w-100"
                                        >
                                            Guardar cambios
                                        </Button>
                                    </Col>

                                    <Col md= {6} className="">
                                    <Button
                                        variant="danger"
                                    >
                                        Eliminar mi cuenta
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
export default UserProfile;
