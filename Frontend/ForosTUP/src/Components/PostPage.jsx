import { useParams } from "react-router-dom";
import Post from "./Post.jsx";

const PostPage = () => {
  const { postId } = useParams();

  return <Post postId={postId} />;
};

export default PostPage;