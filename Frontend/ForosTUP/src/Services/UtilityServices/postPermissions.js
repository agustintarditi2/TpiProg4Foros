export const createPermissions = ({ user, isAdmin, isSysAdmin }) => {
  const isBannedUser = (target) => target?.status === false;

  const canBanUser = (target) => {
    if (!isAdmin) return false;
    if (!target?.id) return false;
    if (target.status === false) return false;
    if (Number(target.id) === Number(user?.id)) return false;
    if (!isSysAdmin && target?.role === "SYSADMIN") return false;
    return true;
  };

  const canMakeAdmin = (target) => {
    if (!isSysAdmin) return false;
    if (!target?.id) return false;
    if (isBannedUser(target)) return false;
    if (Number(target.id) === Number(user?.id)) return false;
    if (target.role && target.role !== "USER") return false;
    return true;
  };

  const canEditPost = (post) => {
    const authorId = post?.userId || post?.Person?.id;
    return Number(authorId) === Number(user?.id);
  };

  const canDeletePost = (post) => {
    if (isSysAdmin) return true;
    if (isAdmin && post?.Person?.role !== "SYSADMIN") return true;
    return false;
  };

  const canEditComment = (comment) => {
    const authorId = comment?.userId || comment?.Person?.id;
    return Number(authorId) === Number(user?.id);
  };

  const canDeleteComments = () => isAdmin || isSysAdmin;

  return {
    isBannedUser,
    canBanUser,
    canMakeAdmin,
    canEditPost,
    canDeletePost,
    canEditComment,
    canDeleteComments,
  };
};