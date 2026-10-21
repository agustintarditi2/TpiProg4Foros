import { Alert, Button, Modal, Spinner } from "react-bootstrap";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext.jsx";
import { useRequireAuth } from "../Services/Hooks/useRequireAuth.js";
import Redirecting from "./Redirecting";
import PostBody from "./PostBody";
import CommentSection from "./CommentSection";
import { usePost } from "../Services/Hooks/usePost.js";
import { useComments } from "../Services/Hooks/useComment.js";
import { useMakeAdmin } from "../Services/Hooks/useMakeAdmin.js";
import { createPermissions } from "../Services/UtilityServices/postPermissions.js";
import "./Post.css";

const Post = ({ postId }) => {
  const { token, user, isAdmin, isSysAdmin } = useAuth();
  const { requireAuth, redirecting } = useRequireAuth();
  const navigate = useNavigate();

  const post = usePost(postId, token);
  const comments = useComments(postId, token);
  const admin = useMakeAdmin(token, post.refresh);
  const permissions = createPermissions({ user, isAdmin, isSysAdmin });

  const handleDeleted = () => {
    const forumId = post.post?.Forum?.id || post.post?.forumId;
    navigate(forumId ? `/forum/${forumId}` : "/foros");
  };

  if (redirecting) return <Redirecting />;

  if (post.loading) {
    return (
      <main className="post-thread-page">
        <div className="post-thread-state">
          <Spinner animation="border" />
          <p>Cargando publicación...</p>
        </div>
      </main>
    );
  }

  if (post.error) {
    return (
      <main className="post-thread-page">
        <div className="post-thread-state">
          <Alert variant="danger" className="mb-0">{post.error}</Alert>
        </div>
      </main>
    );
  }

  if (!post.post) {
    return (
      <main className="post-thread-page">
        <div className="post-thread-state">
          <Alert variant="warning" className="mb-0">Publicación no encontrada.</Alert>
        </div>
      </main>
    );
  }

  return (
    <main className="post-thread-page">
      <section className="post-thread-container">
        <PostBody
          postState={post}
          commentCount={comments.comments.length}
          permissions={permissions}
          token={token}
          user={user}
          isAdmin={isAdmin}
          isSysAdmin={isSysAdmin}
          requireAuth={requireAuth}
          navigate={navigate}
          onMakeAdminClick={admin.request}
          onDeleted={handleDeleted}
        />

        <CommentSection
          postId={postId}
          commentsState={comments}
          permissions={permissions}
          token={token}
          user={user}
          isAdmin={isAdmin}
          isSysAdmin={isSysAdmin}
          requireAuth={requireAuth}
          navigate={navigate}
          onMakeAdminClick={admin.request}
        />

        <Modal show={!!admin.targetUser} onHide={admin.close} centered>
          <Modal.Header closeButton={!admin.loading}>
            <Modal.Title>Confirmar cambio de rol</Modal.Title>
          </Modal.Header>
          <Modal.Body>
            <p className="mb-2">
              ¿Estás seguro que querés convertir a{" "}
              <strong>{admin.targetUser?.name || "este usuario"}</strong> en ADMIN?
            </p>
            {admin.error && (
              <Alert variant="danger" className="mt-3 mb-0">{admin.error}</Alert>
            )}
          </Modal.Body>
          <Modal.Footer>
            <Button variant="secondary" onClick={admin.close} disabled={admin.loading}>
              Cancelar
            </Button>
            <Button variant="success" onClick={admin.confirm} disabled={admin.loading}>
              {admin.loading ? "Convirtiendo..." : "Confirmar"}
            </Button>
          </Modal.Footer>
        </Modal>
      </section>
    </main>
  );
};

export default Post;