import { useCallback, useEffect, useState } from "react";
import { toast } from "react-toastify";
import { TOAST_SUCCESS } from "../UtilityServices/postUtils";
import {
  fetchComments,
  createComment,
  updateComment,
  deleteComment,
} from "../WebServices/commentServices";

export const useComments = (postId, token) => {
  const [comments, setComments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const [newCommentText, setNewCommentText] = useState("");
  const [creating, setCreating] = useState(false);
  const [createError, setCreateError] = useState("");

  const [editingId, setEditingId] = useState(null);
  const [editText, setEditText] = useState("");
  const [savingEdit, setSavingEdit] = useState(false);
  const [editError, setEditError] = useState("");

  const [replyingId, setReplyingId] = useState(null);
  const [replyText, setReplyText] = useState("");
  const [replying, setReplying] = useState(false);
  const [replyError, setReplyError] = useState("");

  const [commentToDelete, setCommentToDelete] = useState(null);
  const [deletingComment, setDeletingComment] = useState(false);
  const [deleteError, setDeleteError] = useState("");

  const refresh = useCallback(async () => {
    try {
      setLoading(true);
      setError("");
      const data = await fetchComments(postId);
      setComments(data);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }, [postId]);

  useEffect(() => {
    refresh();
  }, [refresh]);

  const submitComment = async (event) => {
    event.preventDefault();
    if (!newCommentText.trim()) return setCreateError("El comentario no puede estar vacío");
    try {
      setCreating(true);
      setCreateError("");
      await createComment(postId, { text: newCommentText.trim() }, token);
      toast.success("Comentario publicado", TOAST_SUCCESS);
      setNewCommentText("");
      await refresh();
    } catch (err) {
      setCreateError(err.message);
    } finally {
      setCreating(false);
    }
  };

  const startEdit = (comment) => {
    setEditingId(comment.id);
    setEditText(comment.text);
    setEditError("");
    setReplyingId(null);
  };

  const cancelEdit = () => {
    setEditingId(null);
    setEditText("");
    setEditError("");
  };

  const saveEdit = async (event) => {
    event.preventDefault();
    if (!editText.trim()) return setEditError("El comentario no puede estar vacío");
    try {
      setSavingEdit(true);
      setEditError("");
      await updateComment(editingId, editText.trim(), token);
      toast.success("Comentario actualizado", TOAST_SUCCESS);
      setEditingId(null);
      await refresh();
    } catch (err) {
      setEditError(err.message);
    } finally {
      setSavingEdit(false);
    }
  };

  const startReply = (comment) => {
    setReplyingId(comment.id);
    setReplyText("");
    setReplyError("");
    setEditingId(null);
  };

  const cancelReply = () => {
    setReplyingId(null);
    setReplyText("");
    setReplyError("");
  };

  const submitReply = async (event, parentCommentId) => {
    event.preventDefault();
    if (!replyText.trim()) return setReplyError("La respuesta no puede estar vacía");
    try {
      setReplying(true);
      setReplyError("");
      await createComment(postId, { text: replyText.trim(), parentCommentId }, token);
      toast.success("Respuesta añadida", TOAST_SUCCESS);
      setReplyText("");
      setReplyingId(null);
      await refresh();
    } catch (err) {
      setReplyError(err.message);
    } finally {
      setReplying(false);
    }
  };

  const openDeleteModal = (comment) => {
    setCommentToDelete(comment);
    setDeleteError("");
  };

  const closeDeleteModal = () => {
    if (deletingComment) return;
    setCommentToDelete(null);
    setDeleteError("");
  };

  const confirmDelete = async () => {
    if (!commentToDelete) return;
    try {
      setDeletingComment(true);
      setDeleteError("");
      await deleteComment(commentToDelete.id, token);
      toast.success("El comentario ha sido borrado", TOAST_SUCCESS);
      setCommentToDelete(null);
      setReplyingId(null);
      setEditingId(null);
      await refresh();
    } catch (err) {
      setDeleteError(err.message);
    } finally {
      setDeletingComment(false);
    }
  };

  return {
    comments,
    loading,
    error,
    refresh,
    newCommentText,
    setNewCommentText,
    creating,
    createError,
    submitComment,
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
    commentToDelete,
    deletingComment,
    deleteError,
    openDeleteModal,
    closeDeleteModal,
    confirmDelete,
  };
};