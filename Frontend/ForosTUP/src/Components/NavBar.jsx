import {
  Alert,
  Navbar,
  Container,
  Nav,
  Button,
  Dropdown,
  DropdownButton,
  Modal,
  ModalFooter,
  ButtonGroup,
  DropdownMenu,
  DropdownHeader,
} from "react-bootstrap";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext.jsx";
import "./NavBar.css";
import { useState } from "react";

const NavBar = () => {
  const navigate = useNavigate();

  const API_URL = "http://localhost:3000";

  const [deleteModal, setDeleteModal] = useState(false);
  const [deleteAccountLoading, setDeleteAccountLoading] = useState(false);
  const [deleteAccountError, setDeleteAccountError] = useState("");

  const { user, isAuthenticated, logout, isAdmin, isSysAdmin, token } = useAuth();

  const userColor = isSysAdmin ? "warning" : (isAdmin ? "light" : "info");

  const handleLogout = () => {
    logout();
    navigate("/login");
  };

  const openDeleteModal = () => {
    setDeleteModal(true);
  };

  const closeDeleteModal = () => {
    setDeleteModal(false);
  };

  const handleDeleteAccount = async () => {
    setDeleteAccountLoading(true);
    setDeleteAccountError("");

    const idUserToRemove = user.id;

    try {
      console.log(idUserToRemove);

      const response = await fetch(`${API_URL}/persons/${idUserToRemove}`, {
        method: "DELETE",
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      const data = await response.json();

      if (!response.ok) {
        throw new Error(data.message || "No se pudo eliminar el usuario");
      } else {
        closeDeleteModal();
        logout();
        navigate("/register");
      }
    } catch (err) {
      setDeleteAccountError(err);
    } finally {
      setDeleteAccountLoading(false);
    }
  };

  return (
    <Navbar expand="lg" className="app-navbar">
      <Container fluid className="app-navbar-container">
        <Navbar.Brand as={Link} to="/main" className="app-navbar-brand">
          GRAN FORO TUP
        </Navbar.Brand>

        <Navbar.Toggle
          aria-controls="basic-navbar-nav"
          className="app-navbar-toggle"
        />

        <Navbar.Collapse id="basic-navbar-nav">
          <Nav className="app-navbar-links">
            <Nav.Link as={Link} to="/foros" className="app-navbar-link">
              Foros
            </Nav.Link>

            {isAuthenticated && isAdmin && (
              <Nav.Link as={Link} to="/bans" className="app-navbar-link">
                Baneos
              </Nav.Link>
            )}
          </Nav>

          <Nav className="app-navbar-user ms-auto">
            {isAuthenticated ? (
              <>
              <ButtonGroup size="sm ms-auto">
                <Button disabled variant={userColor}>{user.role}</Button>
                <DropdownButton
                  as={ButtonGroup}
                  popperConfig={{ strategy: 'fixed' }}
                  align="end"
                  drop="down"
                  variant="success"
                  className="app-navbar-username app-navbar-dropdown bg-nested-dropdown"
                  title={user?.name || "Usuario"}
                >
                    <DropdownHeader>Opciones de usuario</DropdownHeader>
                  <Dropdown.Item as={Link} to="/user/edit">
                    Editar mi informacion
                  </Dropdown.Item>

                  <Dropdown.Item onClick={handleLogout}>
                    Cerrar sesión
                  </Dropdown.Item>

                  <Dropdown.Item
                    className="app-dropdown-danger"
                    onClick={openDeleteModal}
                  >
                    Eliminar cuenta
                  </Dropdown.Item>
                </DropdownButton></ButtonGroup>

                <Modal show={deleteModal}>
                  <Modal.Header>
                    <Modal.Title>¿Eliminar cuenta?</Modal.Title>
                  </Modal.Header>

                  {deleteAccountError && (
                    <Alert variant="danger">
                      {deleteAccountError.message || deleteAccountError}
                    </Alert>
                  )}

                  <Modal.Body>
                    <img
                      style={{
                        display: "block",
                        marginLeft: "auto",
                        marginRight: "auto",
                      }}
                      src="https://external-content.duckduckgo.com/iu/?u=https%3A%2F%2Fwallpapercave.com%2Fwp%2Fwp7683467.jpg&f=1&nofb=1&ipt=944c6bd243f846daed0b91dfc007ca0c5130b28f7de5184c0b65edc68989aece"
                      width="250px"
                      alt="Por favor, ¡reconsidere!"
                    />

                    <br />

                    <p>
                      Si acepta, su cuenta será eliminada y ya <b>no</b> podrá
                      entrar al foro con ella. ¿Está seguro que eso es lo que
                      quiere?
                    </p>
                  </Modal.Body>

                  <ModalFooter>
                    <Button
                      variant="secondary"
                      onClick={closeDeleteModal}
                      disabled={deleteAccountLoading}
                    >
                      Cancelar
                    </Button>

                    <Button
                      variant="danger"
                      onClick={handleDeleteAccount}
                      disabled={deleteAccountLoading}
                    >
                      Eliminar mi cuenta
                    </Button>
                  </ModalFooter>
                </Modal>
              </>
            ) : (
              <>
                <Nav.Link as={Link} to="/login" className="app-navbar-link">
                  Iniciar sesión
                </Nav.Link>

                <Nav.Link as={Link} to="/register" className="app-navbar-link">
                  Registrarse
                </Nav.Link>
              </>
            )}
          </Nav>
        </Navbar.Collapse>
      </Container>
    </Navbar>
  );
};

export default NavBar;