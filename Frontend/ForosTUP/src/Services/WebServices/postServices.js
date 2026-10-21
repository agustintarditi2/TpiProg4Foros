export const API_URL = "http://localhost:3000";

export const readJsonResponse = async (response) => {
  const text = await response.text();
  try {
    return text ? JSON.parse(text) : null;
  } catch {
    throw new Error(
      `El servidor no devolvió JSON (${response.status}). Respuesta: ${text.slice(0, 100)}`
    );
  }
};

export const fetchPost = async (postId) => {
  const response = await fetch(`${API_URL}/posts/${postId}`);
  const data = await readJsonResponse(response);
  if (!response.ok) throw new Error(data.message || "No se pudo cargar el post");
  return data;
};

export const fetchMyLike = async (postId, token) => {
  const response = await fetch(`${API_URL}/posts/${postId}/my-like`, {
    headers: { Authorization: `Bearer ${token}` },
  });
  const data = await readJsonResponse(response);
  if (!response.ok) throw new Error(data.message || "No se pudo cargar el like");
  return data;
};

export const likePost = async (postId, token) => {
  const response = await fetch(`${API_URL}/posts/${postId}/like`, {
    method: "PATCH",
    headers: { Authorization: `Bearer ${token}` },
  });
  const data = await readJsonResponse(response);
  if (!response.ok) throw new Error(data.message || "No se pudo actualizar el like");
  return data;
};

export const updatePost = async (postId, { title, body }, token) => {
  const response = await fetch(`${API_URL}/posts/${postId}`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify({ title, body }),
  });
  const data = await readJsonResponse(response);
  if (!response.ok) throw new Error(data.message || data.error || "No se pudo editar el post");
  return data;
};

export const deletePost = async (postId, token) => {
  const response = await fetch(`${API_URL}/posts/${postId}`, {
    method: "DELETE",
    headers: { Authorization: `Bearer ${token}` },
  });
  const data = await readJsonResponse(response);
  if (!response.ok) throw new Error(data.message || "No se pudo borrar el post");
  return data;
};

export const makeUserAdmin = async (userId, token) => {
  const response = await fetch(`${API_URL}/persons/${userId}/make-admin`, {
    method: "PATCH",
    headers: { Authorization: `Bearer ${token}` },
  });
  const data = await readJsonResponse(response);
  if (!response.ok) throw new Error(data.message || "No se pudo convertir el usuario en ADMIN");
  return data;
};