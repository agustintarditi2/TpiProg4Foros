import { Alert, Button, Form } from "react-bootstrap";
import { formatBodyText } from "../Services/imgUtils/imgUtils.js";
import {
  formatDate,
  getInitial,
  getRoleLabel,
  getRoleClass,
} from "../Services/UtilityServices/postUtils";

const Comment = ({
  comment,
  level,
  commentsState,
  permissions,
  token,
  user,
  requireAuth,
  navigate,
  onMakeAdminClick,
  onDeleteClick,
  ...rest
}) => {
  const {
    editingId,
    editText,
    setEditText,
    savingEdit,
    editError,
    startEdit,
    cancelEdit,
    saveEdit,
    replyingId,
    replyText,
    setReplyText,
    replying,
    replyError,
    startReply,
    cancelReply,
    submitReply,
  } = commentsState;

  const { isBannedUser, canBanUser, canMakeAdmin, canEditComment, canDeleteComments } = permissions;

  const author = comment.Person?.name || "Usuario";
  const role = comment.Person?.role;
  const children = comment.children || [];
  const isReplying = replyingId === comment.id;
  const isEditing = editingId === comment.id;

  const goToBanForm = (target) =>
    navigate(
      `/newban?userId=${target.id}&userName=${encodeURIComponent(target.name || "Usuario")}`
    );

  const childProps = {
    commentsState,
    permissions,
    token,
    user,
    requireAuth,
    navigate,
    onMakeAdminClick,
    onDeleteClick,
    ...rest,
  };

  return (
    <div className={`thread-comment level-${Math.min(level, 4)}`}>
      <div className="thread-comment-line" />

      <div className="thread-comment-main">
        <div className="thread-comment-avatar">{getInitial(author)}</div>

        <div className="thread-comment-body">
          <div className="thread-comment-meta">
            <span className="thread-comment-author">{author}</span>
            <span className={`user-role-badge ${getRoleClass(role)}`}>
              {getRoleLabel(role)}
            </span>

            {isBannedUser(comment.Person) ? (
              <button type="button" className="ban-user-link banned-user-link" disabled>
                Baneado
              </button>
            ) : (
              canBanUser(comment.Person) && (
                <button
                  type="button"
                  className="ban-user-link"
                  onClick={() => goToBanForm(comment.Person)}
                >
                  Banear
                </button>
              )
            )}

            {canMakeAdmin(comment.Person) && (
              <button
                type="button"
                className="make-admin-link"
                onClick={() => onMakeAdminClick(comment.Person)}
              >
                Hacer admin
              </button>
            )}

            <span className="thread-comment-dot">•</span>
            <span className="thread-comment-date">
              {formatDate(comment.postDate || comment.createdAt)}
            </span>
          </div>

          {isEditing ? (
            <Form
              onSubmit={(e) => requireAuth(() => saveEdit(e))}
              className="thread-reply-form"
              noValidate
            >
              <Form.Control
                as="textarea"
                rows={2}
                className="thread-reply-input"
                value={editText}
                onChange={(e) => setEditText(e.target.value)}
                isInvalid={!!editError}
              />
              {editError && <div className="thread-reply-error">{editError}</div>}
              <div className="thread-reply-actions">
                <Button type="submit" className="thread-reply-submit" disabled={savingEdit}>
                  {savingEdit ? "Guardando..." : "Guardar"}
                </Button>
                <button type="button" className="thread-reply-cancel" onClick={cancelEdit}>
                  Cancelar
                </button>
              </div>
            </Form>
          ) : (
            <p
              className="thread-comment-text"
              dangerouslySetInnerHTML={{ __html: formatBodyText(comment.text) }}
            />
          )}

          <div className="thread-comment-actions">
            <button type="button" hidden>↑ {comment.likeCount || 0}</button>
            <button type="button" onClick={() => requireAuth(() => startReply(comment))}>
              Responder
            </button>
            {canEditComment(comment) && !isEditing && (
              <button type="button" onClick={() => requireAuth(() => startEdit(comment))}>
                Editar
              </button>
            )}
            {canDeleteComments() && (
              <button
                type="button"
                className="delete-comment-text-button"
                onClick={() => onDeleteClick(comment)}
              >
                Borrar
              </button>
            )}
          </div>

          {isReplying && (
            <Form
              onSubmit={(e) => requireAuth(() => submitReply(e, comment.id))}
              className="thread-reply-form"
              noValidate
            >
              <Form.Control
                type="text"
                className="thread-reply-input"
                placeholder={`Responder a ${author}`}
                value={replyText}
                onChange={(e) => setReplyText(e.target.value)}
                isInvalid={!!replyError}
              />
              {replyError && <div className="thread-reply-error">{replyError}</div>}
              <div className="thread-reply-actions">
                <Button type="submit" className="thread-reply-submit" disabled={replying}>
                  {replying ? "Respondiendo..." : "Responder"}
                </Button>
                <button type="button" className="thread-reply-cancel" onClick={cancelReply}>
                  Cancelar
                </button>
              </div>
            </Form>
          )}

          {children.length > 0 && (
            <div className="thread-children">
              {children.map((child) => (
                <Comment
                  key={child.id}
                  comment={child}
                  level={level + 1}
                  {...childProps}
                />
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default Comment;