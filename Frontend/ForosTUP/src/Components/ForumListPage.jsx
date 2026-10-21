import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Container, Alert, Spinner, Form } from "react-bootstrap";
import Forums from "./Forums.jsx";
import { useFetchFromAPI } from "../Services/fetch/UseFetchFromAPI.jsx";
import { useAuth } from "../context/AuthContext.jsx";
import "./ForumListPage.css";

const ForumListPage = () => {
  const navigate = useNavigate();
  const { isAdmin } = useAuth();

  const { data: forums, loading, error } = useFetchFromAPI("/forums", []);
  const [search, setSearch] = useState("");

  const filteredForums = (forums || []).filter((forum) => {
    const text = `${forum.name || ""} ${forum.desc || ""}`.toLowerCase();
    return text.includes(search.toLowerCase());
  });

  if (loading) {
    return (
      <main className="forums-page">
        <Container>
          <div className="forums-page-loading">
            <Spinner animation="border" />
            <p>Cargando foros...</p>
          </div>
        </Container>
      </main>
    );
  }

  if (error) {
    return (
      <main className="forums-page">
        <Container>
          <Alert variant="danger">
            Error al cargar los foros: {error}
          </Alert>
        </Container>
      </main>
    );
  }

  return (
    <main className="forums-page">
      <Container>
        <section className="forums-page-header">
          <h1 className="forums-page-title">Foros de la comunidad</h1>

          <p className="forums-page-description">
            Elegí un foro para ver sus publicaciones, leer comentarios y
            participar en las conversaciones de la comunidad TUP.
          </p>

          <div className="forums-page-search-wrapper">
            <Form.Control
              type="text"
              placeholder="Buscar foros..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="forums-page-search"
            />
          </div>

          {isAdmin && (
            <button
              className="forums-page-create-button"
              onClick={() => navigate("/newforum")}
            >
              + Crear foro
            </button>
          )}
        </section>

        <Forums forumsProp={filteredForums} />
      </Container>
    </main>
  );
};

export default ForumListPage;