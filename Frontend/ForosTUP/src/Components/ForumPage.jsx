import { useParams, Link, useNavigate } from "react-router-dom";
import { Form, Row, Col, Card, Spinner, Alert, Button } from "react-bootstrap";
import { useFetchFromAPI } from "../Services/fetch/UseFetchFromAPI.jsx";
import { useRequireAuth } from "../Services/Hooks/useRequireAuth.js";
import "./ForumPage.css";
import { useState } from "react";
import { useAuth } from "../context/AuthContext.jsx";
import { toast } from "react-toastify";
import Redirecting from "./Redirecting.jsx";

const ForumPage = () => {
  // obtiene el id del foro desde la url
  const { forumId } = useParams();


  const navigate = useNavigate();

  // activación de los campos de edición
  const {isAdmin, isSysAdmin, user, token} = useAuth()
  const [editForumName, setEditForumName] = useState(false);
  const [editForumRules, setEditForumRules] = useState(false);
  const [editForumDesc, setEditForumDesc] = useState(false);
  const [editedRules, setEditedRules] = useState("");
  const [editedName, setEditedName] = useState("");
  const [editedDesc, setEditedDesc] = useState("");
  const [editRulesError, setEditRulesError] = useState("");
  const [editNameError, setEditNameError] = useState("");
  const [editDescError, setEditDescError] = useState("");
  const [editLoading, setEditLoading] = useState(false)
  
  // verifica que el usuario este loggeado
  const { requireAuth, redirecting } = useRequireAuth();

  // trae los datos del foro desde el backend
  const {
    data: forum,
    loading: loadingForum,
    error: errorForum,
    refetch: fetchForums,
  } = useFetchFromAPI(`/forums/${forumId}`, null);

  // trae los posts del foro desde el backend
  const {
    data: posts,
    loading: loadingPosts,
    error: errorPosts,
  } = useFetchFromAPI(`/forums/${forumId}/posts`, []);

  // asegura que sea un array
  const forumPosts = Array.isArray(posts) ? posts : [];
  // arma la lista de reglas, si no tiene reglas usa reglas por defecto
  const rules = forum?.rules
    ? forum?.rules
        .split(".")
        .map((rule) => rule.trim())
        .filter((rule) => rule.length > 0)
    : [
        "Respeta a los demás usuarios",
        "No publicar spam",
        "No contenido ofensivo",
      ];

  // calcula la cantidad de comentarios
  const totalComments = forumPosts.reduce(
    (total, post) => total + (post.Comments?.length || 0),
    0
  );

  // maneja el "crear publicacion", si esta loggeado lo lleva al form
  const handleNewPost = () => {
    requireAuth(() => navigate(`/forums/${forumId}/posts/new`));
  };

  // maneja el click sobre un post
  const handleGoToPost = (postId) => {
    navigate(`/post/${postId}`);
  };

  // un booleano que es True si el usuario puede editar el foro
    const canEditForum =
    isSysAdmin ||
    (isAdmin && Number(user?.id) === Number(forum?.founderId));
     // funciones que abren o cierran los editores.
    const toggleEditForumName = () => {
      setEditedName(forum?.name || "")
      setEditForumDesc(false);
      setEditForumRules(false)
      setEditForumName(!editForumName);}

    const toggleEditForumRules = () => {
      setEditedRules(forum?.rules || "")
      setEditForumDesc(false);
      setEditForumName(false);
      setEditForumRules(!editForumRules);}

    const toggleEditForumDesc = () => {
      setEditedDesc(forum?.desc || "");
      setEditForumRules(false);
      setEditForumName(false);
      setEditForumDesc(!editForumDesc);
    }

  //Función que maneja el editor de reglas:
  const handleUpdateForum = async () => {
    
    if (!canEditForum) {
      toast.error("No tenés permisos para editar este foro.");
      return;
    }

    setEditLoading(true)
    setEditNameError("")
    setEditRulesError("")
    setEditDescError("")
   try {
     let newForum = {
      name: forum?.name,
      desc: forum?.desc,
      rules: forum?.rules,
      status: forum?.status
    }

    if (editedName !== "") newForum.name = editedName || "";
    if (editedRules !== "") newForum.rules = editedRules || "";
    if (editedDesc !== "") newForum.desc = editedDesc || ""
    const res = await fetch(`http://localhost:3000/forums/${forumId}`, {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
        Authorization:`Bearer ${token}`
      },
      body: JSON.stringify(newForum)
    })
    const data = await res.json()
    if (!res.ok) {
      throw new Error(
        data.message || data.error || "No se pudo editar el post");};
    setEditForumRules(false);
    setEditForumName(false)
    setEditForumDesc(false)
    setEditedRules("");
    setEditedName("");
    setEditLoading(false)
    fetchForums()
   } catch (error) {
    toast.error(error.message || error || "Hubo un error")
   } finally {
    setEditLoading(false);
   }

  }


  
  if (redirecting) {
    return <Redirecting />
  }

  if (loadingForum || loadingPosts) {
    return (
      <main className="forum-detail-page">
        <div className="forum-detail-loading">
          <Spinner animation="border" />
          <p>Cargando foro...</p>
        </div>
      </main>
    );
  }

  if (errorForum || errorPosts) {
    return (
      <main className="forum-detail-page">
        <Alert variant="danger">
          Error al cargar el foro: {errorForum || errorPosts}
        </Alert>
      </main>
    );
  }

  return (
    <main className="forum-detail-page">
      <section className="forum-detail-card">
        <div className="forum-detail-breadcrumb">
          <Link to="/foros">Foros</Link>
          <span>{">"}</span>
          <span>{forum?.name || "Foro"}</span>
        </div>

        <div className="forum-detail-header">
          <div className="forum-detail-header-content">
            {!!editForumName ? 
                  <Form>
                    <h6>Editar el nombre del foro</h6>
                    <Form.Group>
                      <Form.Control
                        type="text"
                        value={editedName}
                        onChange={(event) => {
                          setEditedName(event.target.value);
                          setEditNameError("");
                        }}
                        isInvalid={!!editNameError}/>
                      </Form.Group>
                    <Button variant="secondary" disabled={editLoading} onClick={toggleEditForumName}>Cancelar</Button>&emsp;
                    <Button variant="success" disabled={editLoading} onClick={handleUpdateForum}>Guardar</Button>
                  </Form> : <h1>{forum?.name || "Foro"} &emsp;<Button
                    variant="outline-success"
                    size="sm"
                    className="edit-button"
                    hidden={!canEditForum || !!editForumName}
                    onClick={toggleEditForumName}
                    >Editar</Button></h1>
                    }            
            {!!editForumDesc ? 
                  <Form>
                    <h6>Editar la descripción del foro</h6>
                    <Form.Group>
                      <Form.Control
                        as="textarea"
                        rows={4}
                        value={editedDesc}
                        onChange={(event) => {
                          setEditedDesc(event.target.value);
                          setEditDescError("");
                        }}
                        isInvalid={!!editDescError}/>
                      </Form.Group>
                    <Button variant="secondary" disabled={editLoading} onClick={toggleEditForumDesc}>Cancelar</Button>&emsp;
                    <Button variant="success" disabled={editLoading} onClick={handleUpdateForum}>Guardar</Button>
                  </Form> : <p> {forum?.desc || "Espacio general para debatir temas de la comunidad."} &emsp;<Button
                    variant="outline-success"
                    size="sm" className="edit-button"
                    hidden={!canEditForum || !!editForumDesc}
                    onClick={toggleEditForumDesc}
                    >Editar</Button> </p>}


            <div className="forum-detail-meta">
              <span>Fundador: {forum?.Person?.name || "Admin"}</span>
              <span>{forumPosts.length} Publicaciones</span>
            </div>
          </div>

          <Button
            variant="success"
            className="forum-detail-new-post"
            onClick={handleNewPost}
          >
            + Nueva publicación
          </Button>
        </div>

        <Row className="forum-detail-body">
          <Col lg={8}>
            <section className="forum-detail-posts">
              <h2>Publicaciones recientes</h2>

              {forumPosts.length === 0 ? (
                <Card className="forum-detail-empty-card">
                  <Card.Body>
                    <Card.Title>No hay publicaciones todavía</Card.Title>
                    <Card.Text>
                      Este foro todavía no tiene publicaciones cargadas.
                    </Card.Text>
                  </Card.Body>
                </Card>
              ) : (
                <div className="forum-detail-post-list">
                  {forumPosts.map((post) => (
                    <article
                      key={post.id}
                      className="forum-detail-post-card"
                      onClick={() => handleGoToPost(post.id)}
                    >
                      <div className="forum-detail-post-avatar">
                        {post.Person?.name?.charAt(0).toUpperCase() || "U"}
                      </div>

                      <div className="forum-detail-post-content">
                        <h3>{post.title}</h3>

                        <p>{post.body}</p>

                        <div className="forum-detail-post-footer">
                          <span>
                            {post.Person?.name || "Usuario"} · Hace unas horas
                          </span>

                          <div className="forum-detail-post-stats">
                            <span className="forum-detail-comments-count">
                              comentarios {post.Comments?.length || 0}
                            </span>

                            <span className="forum-detail-like-count">
                              <svg
                                className="forum-detail-like-icon"
                                viewBox="0 0 24 24"
                                aria-hidden="true"
                              >
                                <path d="M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.08C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z" />
                              </svg>

                              {post.likeCount || 0}
                            </span>
                          </div>
                        </div>
                      </div>
                    </article>
                  ))}
                </div>
              )}
            </section>
          </Col>

          <Col lg={4}>
            <aside className="forum-detail-sidebar">
              <Card className="forum-detail-side-card">
                <Card.Body>
                  <Card.Title>Sobre este foro</Card.Title>
                  {!!editForumRules ? 
                  <Form>
                    <h6>Editar reglas del foro</h6>
                    <p style={{color:'red'}}>Recuerde separar las reglas con un punto (.).</p>
                    <Form.Group>
                      <Form.Control
                        as="textarea"
                        rows={4}
                        value={editedRules}
                        onChange={(event) => {
                          setEditedRules(event.target.value);
                          setEditRulesError("");
                        }}
                        isInvalid={!!editRulesError}/>
                      </Form.Group>
                    <Button variant="secondary" disabled={editLoading} onClick={toggleEditForumRules}>Cancelar</Button>
                    &emsp;
                    <Button variant="success" disabled={editLoading} onClick={handleUpdateForum}>Guardar</Button>
                  </Form> : <div><h6>Reglas del foro</h6>
                  <ul className="forum-detail-rules">
                    {rules.map((rule, index) => (
                      <li key={index}>{rule}.</li>
                    ))}
                  </ul></div>}
                  <Button
                    variant="outline-success"
                    size="sm"
                    hidden={!canEditForum || !!editForumRules}
                    onClick={toggleEditForumRules}
                    >Editar</Button>
                </Card.Body>
                
              </Card>

              <Card className="forum-detail-side-card">
                <Card.Body>
                  <Card.Title>Estadísticas</Card.Title>

                  <div className="forum-detail-stat">
                    <span>Publicaciones:</span>
                    <strong>{forumPosts.length}</strong>
                  </div>

                  <div className="forum-detail-stat">
                    <span>Comentarios:</span>
                    <strong>{totalComments}</strong>
                  </div>

                  <div className="forum-detail-stat">
                    <span>Estado:</span>
                    <strong>Activo</strong>
                  </div>
                </Card.Body>
              </Card>
            </aside>
          </Col>
        </Row>
      </section>
    </main>
  );
};

export default ForumPage;