import ForumDisplay from "./ForumDisplay.jsx";

//recibe los foros por props
const Forums = ({ forumsProp = [] }) => {
  // si llega algo que no es un array usa un array vacio
  const forums = Array.isArray(forumsProp) ? forumsProp : [];

  if (forums.length === 0) {
    return (
      <div className="forums-page-empty">
        <p>No se encontraron foros.</p>
      </div>
    );
  }

  return (
    <section className="forums-page-grid">
      {forums.map((forum) => (
        <ForumDisplay key={forum.id} forum={forum} />
      ))}
    </section>
  );
};

export default Forums;