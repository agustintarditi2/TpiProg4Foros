import "./MainPage.css";
import Forums from "./Forums.jsx";
import { useFetchFromAPI } from "../Services/fetch/UseFetchFromAPI.jsx";

const MainPage = () => {
  const { data: forums, loading, error } = useFetchFromAPI("/Forums", []);

  if (loading) return <p>Cargando foros...</p>;
  if (error) return <p>Error: {error}</p>;

  return <Forums forumsProp={forums} />;
};

export default MainPage;
