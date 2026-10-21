import { Card, Button, Modal, Alert } from "react-bootstrap";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext.jsx";
import "./forumDisplay.css";
import { useState } from "react";

//Recibe por prop un foro mapeado por forum(padre) y lo renderiza visualmente
const ForumDisplay = ({ forum }) => {
  const navigate = useNavigate();

  const { token, isSysAdmin, isAuthenticated } = useAuth();

  const { id, name, desc, status } = forum;

  const [deletionModal, setDeletionModal] = useState(false);
  const [deleteForumLoading, setDeleteForumLoading] = useState(false);
  const [deleteForumError, setDeleteForumError] = useState("");
  const [isForumVisible, setIsForumVisible] = useState(status);

  const posts = Array.isArray(forum.Posts) ? forum.Posts : [];
  const totalPosts = posts.length;
  const postsLabel = totalPosts === 1 ? "post" : "posts";

  // al tocar el boton "ir al foro" arma una url con el id del foro clickeado y usa navigate para ir a esa ruta.
  const handleGoToForum = () => {
    navigate(`/forum/${id}`);
  };

  // Verificación de que el usuario tenga los permisos necesarios.
  const canDeleteForum = isSysAdmin && isAuthenticated;

  // Esta función abre el modal para eliminar foros...
  const handleOpenDeleteForumModal = () => {
    if (!canDeleteForum) {
      setDeleteForumError("No tenés permisos para borrar foros");
      return;
    }

    setDeletionModal(true);
    setDeleteForumError("");
  };

  // ...y esta lo cierra.
  const handleCloseDeleteForumModal = () => {
    if (deleteForumLoading) {
      return;
    }

    setDeletionModal(false);
    setDeleteForumError("");
  };

  // Esta función da la baja lógica del foro, que no puede darse de baja físicamente fuera del servidor. Los posts seguirán accesibles, pero solo por link directo.
  const handleDeleteForum = async () => {
    try {
      setDeleteForumLoading(true);
      setDeleteForumError("");

      const response = await fetch(`http://localhost:3000/forums/${id}`, {
        method: "DELETE",
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      const data = await response.json();

      if (!response.ok) {
        throw new Error(data.message || "No se pudo borrar el foro");
      }

      setDeletionModal(false);
      setIsForumVisible(false);
    } catch (err) {
      setDeleteForumError(err.message);
    } finally {
      setDeleteForumLoading(false);
    }
  };

  // Si el foro se dió de baja, no se renderiza.
  if (!isForumVisible) return null;

  // Return exitoso del foro.
  return (
    <Card className="single-forum-card">
      <Card.Body className="single-forum-card-body">
        <div className="single-forum-card-content">
          <Card.Title className="single-forum-card-title">
            {name}
          </Card.Title>

          <Card.Text className="single-forum-card-description">
            {desc || "Este foro no tiene descripción cargada."}
          </Card.Text>

          <div className="single-forum-card-stats">
            <span>
              {totalPosts} {postsLabel}
            </span>
          </div>
        </div>

        <Button
          variant="outline-success"
          className="single-forum-card-action"
          onClick={handleGoToForum}
        >
          Ir al foro
        </Button>

        {/* Componente del botón de borrar foros. Solo se renderiza si el usuario tiene la autorización correcta. */}
        {canDeleteForum && (
          <Button variant="outline-danger" onClick={handleOpenDeleteForumModal}>
            Eliminar foro
          </Button>
        )}
      </Card.Body>

      <Modal show={deletionModal} onHide={handleCloseDeleteForumModal} centered>
        <Modal.Header closeButton={!deleteForumLoading}>
          <Modal.Title>Borrar foro</Modal.Title>
        </Modal.Header>

        <Modal.Body>
          <p className="mb-2">
            ¿Estás seguro que deseás borrar este foro?
          </p>

          {deleteForumError && (
            <Alert variant="danger" className="mt-3 mb-0">
              {deleteForumError}
            </Alert>
          )}
        </Modal.Body>

        <Modal.Footer>
          <Button
            variant="secondary"
            onClick={handleCloseDeleteForumModal}
            disabled={deleteForumLoading}
          >
            Cancelar
          </Button>

          <Button
            variant="danger"
            onClick={handleDeleteForum}
            disabled={deleteForumLoading}
          >
            {deleteForumLoading ? "Borrando..." : "Borrar"}
          </Button>
        </Modal.Footer>
      </Modal>
    </Card>
  );
};

export default ForumDisplay;