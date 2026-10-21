import { Alert, Button, Form, Modal } from "react-bootstrap";
import Comment from "./Comment";
import { buildCommentTree, getInitial, getNestedCommentCount } from "../Services/UtilityServices/postUtils";

const CommentSection = ({
  postId,
  commentsState,
  permissions,
  token,
  user,
  isAdmin,
  isSysAdmin,
  requireAuth,
  navigate,
  onMakeAdminClick,
}) => {
  const {
    comments,
    newCommentText,
    setNewCommentText,
    creating,
    createError,
    submitComment,
    commentToDelete,
    deletingComment,
    deleteError,
    openDeleteModal,
    closeDeleteModal,
    confirmDelete,
  } = commentsState;

  const commentTree = buildCommentTree(comments);

  const commentProps = {
    postId,
    commentsState,
    permissions,
    token,
    user,
    isAdmin,
    isSysAdmin,
    requireAuth,
    navigate,
    onMakeAdminClick,
    onDeleteClick: (comment) => requireAuth(() => openDeleteModal(comment)),
  };

  const nestedCount = commentToDelete ? getNestedCommentCount(commentToDelete) : 0;

  return (
    <>
      <section className="post-comment-entry">
        <Form onSubmit={(e) => requireAuth(() => submitComment(e))} noValidate>
          <div className="post-comment-input-row">
            <div className="post-comment-user-avatar">{getInitial(user?.name || "U")}</div>
            <Form.Control
              type="text"
              className="post-comment-input"
              placeholder="Unite a la conversación"
              value={newCommentText}
              onChange={(e) => setNewCommentText(e.target.value)}
              isInvalid={!!createError}
            />
          </div>
          <Form.Control.Feedback type="invalid" className="post-comment-error d-block">
            {createError}
          </Form.Control.Feedback>
          <Button type="submit" className="post-comment-submit" disabled={creating}>
            {creating ? "Comentando..." : "Comentar"}
          </Button>
        </Form>
      </section>

      <section className="post-comments-thread">
        {commentTree.length === 0 ? (
          <div className="post-comments-empty">
            <h3>No hay comentarios todavía</h3>
            <p>Sé el primero en unirte a la conversación.</p>
          </div>
        ) : (
          commentTree.map((comment) => (
            <Comment key={comment.id} comment={comment} level={0} {...commentProps} />
          ))
        )}
      </section>

      <Modal show={!!commentToDelete} onHide={closeDeleteModal} centered>
        <Modal.Header closeButton={!deletingComment}>
          <Modal.Title>Borrar comentario</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <p className="mb-2">¿Estás seguro que deseás borrar este comentario?</p>
          {nestedCount > 0 && (
            <p className="delete-comment-warning mb-0">
              También se borrarán {nestedCount}
              {nestedCount === 1 ? " respuesta anidada." : " respuestas anidadas."}
            </p>
          )}
          {deleteError && (
            <Alert variant="danger" className="mt-3 mb-0">{deleteError}</Alert>
          )}
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={closeDeleteModal} disabled={deletingComment}>
            Cancelar
          </Button>
          <Button variant="danger" onClick={confirmDelete} disabled={deletingComment}>
            {deletingComment ? "Borrando..." : "Borrar"}
          </Button>
        </Modal.Footer>
      </Modal>
    </>
  );
};

export default CommentSection;