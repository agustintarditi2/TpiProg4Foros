import { useCallback, useEffect, useState } from "react";
import { toast } from "react-toastify";
import { TOAST_SUCCESS } from "../UtilityServices/postUtils";
import {
  fetchPost,
  fetchMyLike,
  likePost,
  updatePost,
  deletePost,
} from "../WebServices/postServices";

export const usePost = (postId, token) => {
  const [post, setPost] = useState(null);
  const [likedByMe, setLikedByMe] = useState(false);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [likeLoading, setLikeLoading] = useState(false);

  const [editing, setEditing] = useState(false);
  const [editTitle, setEditTitle] = useState("");
  const [editBody, setEditBody] = useState("");
  const [editError, setEditError] = useState("");
  const [savingEdit, setSavingEdit] = useState(false);

  const [deleteModalOpen, setDeleteModalOpen] = useState(false);
  const [deleteError, setDeleteError] = useState("");
  const [deleting, setDeleting] = useState(false);

  const refresh = useCallback(async () => {
    try {
      setLoading(true);
      setError("");
      const data = await fetchPost(postId);
      setPost(data);
      if (token) {
        const likeData = await fetchMyLike(postId, token);
        setLikedByMe(!!likeData?.likedByMe);
      } else {
        setLikedByMe(false);
      }
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }, [postId, token]);

  useEffect(() => {
    refresh();
  }, [refresh]);

  const toggleLike = async () => {
    if (likeLoading) return;
    try {
      setLikeLoading(true);
      const data = await likePost(postId, token);
      const next = !!data?.likedByMe;
      setLikedByMe(next);
      setPost((prev) =>
        prev
          ? {
              ...prev,
              likeCount:
                data?.likeCount ??
                Math.max((prev.likeCount || 0) + (next ? 1 : -1), 0),
            }
          : prev
      );
    } catch (err) {
      setError(err.message);
    } finally {
      setLikeLoading(false);
    }
  };

  const startEdit = () => {
    setEditing(true);
    setEditTitle(post?.title || "");
    setEditBody(post?.body || "");
    setEditError("");
  };

  const cancelEdit = () => {
    setEditing(false);
    setEditTitle("");
    setEditBody("");
    setEditError("");
  };

  const saveEdit = async (event) => {
    event.preventDefault();
    if (!editTitle.trim()) return setEditError("El título no puede estar vacío");
    if (!editBody.trim()) return setEditError("El contenido no puede estar vacío");
    try {
      setSavingEdit(true);
      setEditError("");
      await updatePost(postId, { title: editTitle.trim(), body: editBody.trim() }, token);
      toast.success("Post actualizado correctamente", TOAST_SUCCESS);
      setEditing(false);
      await refresh();
    } catch (err) {
      setEditError(err.message);
    } finally {
      setSavingEdit(false);
    }
  };

  const openDeleteModal = () => {
    setDeleteModalOpen(true);
    setDeleteError("");
  };

  const closeDeleteModal = () => {
    if (deleting) return;
    setDeleteModalOpen(false);
    setDeleteError("");
  };

  const confirmDelete = async () => {
    try {
      setDeleting(true);
      setDeleteError("");
      await deletePost(postId, token);
      toast.success("El post ha sido eliminado correctamente", TOAST_SUCCESS);
      setDeleteModalOpen(false);
      return true;
    } catch (err) {
      setDeleteError(err.message);
      return false;
    } finally {
      setDeleting(false);
    }
  };

  return {
    post,
    likedByMe,
    loading,
    error,
    likeLoading,
    toggleLike,
    refresh,
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
  };
};