import { Alert, Button, Form, Modal } from "react-bootstrap";
import { Link } from "react-router-dom";
import { formatBodyText } from "../Services/imgUtils/imgUtils.js";
import {
  formatDate,
  getInitial,
  getRoleLabel,
  getRoleClass,
} from "../Services/UtilityServices/postUtils";

const PostBody = ({
  postState,
  commentCount,
  permissions,
  user,
  requireAuth,
  navigate,
  onMakeAdminClick,
  onDeleted,
}) => {
  const {
    post,
    likedByMe,
    likeLoading,
    toggleLike,
    editing,
    editTitle,
    editBody,
    editError,
    savingEdit,
    setEditTitle,
    setEditBody,
    setEditError,
    startEdit,
    cancelEdit,
    saveEdit,
    deleteModalOpen,
    deleteError,
    deleting,
    openDeleteModal,
    closeDeleteModal,
    confirmDelete,
  } = postState;

  const {
    isBannedUser,
    canBanUser,
    canMakeAdmin,
    canEditPost,
    canDeletePost,
  } = permissions;

  const authorName = post.Person?.name || "Usuario";
  const authorRole = post.Person?.role;
  const forumName = post.Forum?.name || "Foro";
  const forumId = post.Forum?.id || post.forumId;

  const goToBanForm = (target) =>
    navigate(
      `/newban?userId=${target.id}&userName=${encodeURIComponent(target.name || "Usuario")}`
    );

  const handleSaveEdit = (event) =>
    requireAuth(() => saveEdit(event));

  const handleOpenDeleteModal = () =>
    requireAuth(() => {
      if (!canDeletePost(post)) return;
      openDeleteModal();
    });

  const handleConfirmDelete = async () => {
    const ok = await confirmDelete();
    if (ok) onDeleted();
  };

  return (
    <>
      <div className="post-thread-breadcrumb">
        <Link to="/foros">Foros</Link>
        <span>›</span>
        {forumId ? <Link to={`/forum/${forumId}`}>{forumName}</Link> : <span>{forumName}</span>}
        <span>›</span>
        <span>{post.title}</span>
      </div>

      <article className="post-main-block">
        <div className="post-main-meta">
          <div className="post-main-avatar">{getInitial(authorName)}</div>
          <div className="post-main-author-row">
            <span className="post-main-author">{authorName}</span>
            <span className={`user-role-badge ${getRoleClass(authorRole)}`}>
              {getRoleLabel(authorRole)}
            </span>

            {isBannedUser(post.Person) ? (
              <button type="button" className="ban-user-link banned-user-link" disabled>
                Baneado
              </button>
            ) : (
              canBanUser(post.Person) && (
                <button
                  type="button"
                  className="ban-user-link"
                  onClick={() => goToBanForm(post.Person)}
                >
                  Banear
                </button>
              )
            )}

            {canMakeAdmin(post.Person) && (
              <button
                type="button"
                className="make-admin-link"
                onClick={() => onMakeAdminClick(post.Person)}
              >
                Hacer admin
              </button>
            )}

            <span className="post-main-dot">•</span>
            <span className="post-main-date">
              {formatDate(post.postDate || post.createdAt)}
            </span>
          </div>
        </div>

        {editing ? (
          <Form onSubmit={handleSaveEdit} noValidate>
            <Form.Group className="mb-3">
              <Form.Control
                type="text"
                value={editTitle}
                onChange={(e) => {
                  setEditTitle(e.target.value);
                  setEditError("");
                }}
                isInvalid={!!editError}
              />
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Control
                as="textarea"
                rows={4}
                value={editBody}
                onChange={(e) => {
                  setEditBody(e.target.value);
                  setEditError("");
                }}
                isInvalid={!!editError}
              />
            </Form.Group>
            {editError && <Alert variant="danger">{editError}</Alert>}
            <div className="thread-reply-actions">
              <Button type="submit" disabled={savingEdit}>
                {savingEdit ? "Guardando..." : "Guardar"}
              </Button>
              <button type="button" className="thread-reply-cancel" onClick={cancelEdit}>
                Cancelar
              </button>
            </div>
          </Form>
        ) : (
          <>
            <h1 className="post-main-title">{post.title}</h1>
            <p
              className="post-main-description"
              dangerouslySetInnerHTML={{ __html: formatBodyText(post.body) }}
            />
          </>
        )}

        <div className="post-main-actions">
          <button
            type="button"
            onClick={() => requireAuth(toggleLike)}
            disabled={likeLoading}
            className={`post-like-button ${likedByMe ? "post-like-active" : ""}`}
            aria-label={likedByMe ? "Quitar me gusta" : "Dar me gusta"}
          >
            <svg className="post-like-icon" viewBox="0 0 24 24" aria-hidden="true">
              <path d="M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.08C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z" />
            </svg>
            <span>{post.likeCount || 0}</span>
          </button>

          <span className="post-comments-count">{commentCount} comentarios</span>

          {canEditPost(post) && !editing && (
            <Button
              type="button"
              variant="outline-secondary"
              size="sm"
              onClick={() => requireAuth(startEdit)}
            >
              Editar post
            </Button>
          )}

          {canDeletePost(post) && (
            <Button
              type="button"
              variant="outline-danger"
              size="sm"
              onClick={handleOpenDeleteModal}
            >
              Eliminar post
            </Button>
          )}
        </div>
      </article>

      <Modal show={deleteModalOpen} onHide={closeDeleteModal} centered>
        <Modal.Header closeButton={!deleting}>
          <Modal.Title>Borrar post</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <p className="mb-2">¿Estás seguro que deseás borrar este post?</p>
          {deleteError && (
            <Alert variant="danger" className="mt-3 mb-0">{deleteError}</Alert>
          )}
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={closeDeleteModal} disabled={deleting}>
            Cancelar
          </Button>
          <Button variant="danger" onClick={handleConfirmDelete} disabled={deleting}>
            {deleting ? "Borrando..." : "Borrar"}
          </Button>
        </Modal.Footer>
      </Modal>
    </>
  );
};

export default PostBody;