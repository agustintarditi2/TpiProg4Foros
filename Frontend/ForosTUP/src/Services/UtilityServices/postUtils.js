export const TOAST_SUCCESS = {
  className: "toast-success-custom",
  progressClassName: "toast-progress-custom",
};

export const formatDate = (date) => {
  if (!date) return "Hace un rato";
  const d = new Date(date);
  if (Number.isNaN(d.getTime())) return "Hace un rato";
  return d.toLocaleDateString("es-AR", {
    day: "2-digit",
    month: "short",
    year: "numeric",
  });
};

export const getInitial = (name) => name?.charAt(0)?.toUpperCase() || "U";

export const getRoleLabel = (role) => {
  if (role === "SYSADMIN") return "SYSADMIN";
  if (role === "ADMIN") return "ADMIN";
  return "USER";
};

export const getRoleClass = (role) => {
  if (role === "SYSADMIN") return "role-sysadmin";
  if (role === "ADMIN") return "role-admin";
  return "role-user";
};

export const buildCommentTree = (list) => {
  const map = new Map();
  list.forEach((c) => map.set(c.id, { ...c, children: [] }));
  const roots = [];
  list.forEach((c) => {
    const parentId =
      c.parentCommentId || c.parentId || c.commentParentId || null;
    if (parentId && map.has(parentId)) {
      map.get(parentId).children.push(map.get(c.id));
    } else {
      roots.push(map.get(c.id));
    }
  });
  return roots;
};

export const getNestedCommentCount = (comment) => {
  if (!comment?.children?.length) return 0;
  return comment.children.reduce(
    (total, child) => total + 1 + getNestedCommentCount(child),
    0
  );
};