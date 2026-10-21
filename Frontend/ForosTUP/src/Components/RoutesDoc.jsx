import { BrowserRouter, Route, Routes } from "react-router-dom";

import "react-toastify/dist/ReactToastify.css";
import "../styles/toast.css";
import Protected from "./Protected.jsx";
import PublicOnly from "./PublicOnly.jsx";

import MainPage from "./MainPage.jsx";
import LogIn from "./LogIn.jsx";
import Register from "./Register.jsx";
import NotFound from "./NotFound.jsx";
import Layout from "./Layout.jsx"
import ForumListPage from "./ForumListPage.jsx";
import ForumPage from "./ForumPage.jsx";
import PostPage from "./PostPage.jsx";

import NewForum from "./NewForum.jsx";
import NewPost from "./NewPost.jsx";

import Bans from "./Bans.jsx";
import NewBans from "./NewBans.jsx";
import UserEdit from "./UserEdit.jsx";
import ProtectedOnlyAdmin from "./ProtectedOnlyAdmin.jsx";

const RoutesDoc = () => {
  return (
    <BrowserRouter>
      

      <Routes>
        {/* Rutas públicas */}
        <Route element={<Layout />}>
          <Route path="/" element={<MainPage />} />
          <Route path="/main" element={<MainPage />} />
          <Route path="/home" element={<MainPage />} />

          <Route element={<PublicOnly />}>
            <Route path="/login" element={<LogIn />} />
            <Route path="/register" element={<Register />} />
          </Route>

          <Route path="/foros" element={<ForumListPage />} />
          <Route path="/forum/:forumId" element={<ForumPage />} />
          <Route path="/post/:postId" element={<PostPage />} />

          {/* Rutas protegidas: solo usuarios logueados pueden entrar */}
          <Route element={<Protected />}>
            <Route path="/user/edit" element={<UserEdit />} />
            <Route path="/forums/:forumId/posts/new" element={<NewPost />} />
            
            {/* Rutas de Admin: solo ADMIN y SYSADMIN pueden acceder. */}
            <Route element={<ProtectedOnlyAdmin />}>
              <Route path="/bans" element={<Bans />} />
              <Route path="/newban" element={<NewBans />} />
              {/* /users queda pendiente: UserList no existe en el proyecto original. */}
              <Route path="/newforum" element={<NewForum />} />
            </Route>
        </Route>
        </Route>
        {/* Ruta 404 */}
        <Route path="*" element={<NotFound />} />
      </Routes>


     
    </BrowserRouter>
  );
};

export default RoutesDoc;
