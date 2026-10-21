import { API_URL, readJsonResponse } from "./postServices";

export const fetchComments = async (postId) => {
  const response = await fetch(`${API_URL}/posts/${postId}/comments`);
  const data = await readJsonResponse(response);
  if (!response.ok) throw new Error(data.message || "No se pudieron cargar los comentarios");
  return data;
};

export const createComment = async (postId, { text, parentCommentId }, token) => {
  const response = await fetch(`${API_URL}/posts/${postId}/comments`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify({ text, parentCommentId }),
  });
  const data = await readJsonResponse(response);
  if (!response.ok) throw new Error(data.message || "No se pudo crear el comentario");
  return data;
};

export const updateComment = async (commentId, text, token) => {
  const response = await fetch(`${API_URL}/comments/${commentId}`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify({ text }),
  });
  const data = await readJsonResponse(response);
  if (!response.ok) throw new Error(data.message || data.error || "No se pudo editar el comentario");
  return data;
};

export const deleteComment = async (commentId, token) => {
  const response = await fetch(`${API_URL}/comments/${commentId}`, {
    method: "DELETE",
    headers: { Authorization: `Bearer ${token}` },
  });
  const data = await readJsonResponse(response);
  if (!response.ok) throw new Error(data.message || "No se pudo borrar el comentario");
  return data;
};