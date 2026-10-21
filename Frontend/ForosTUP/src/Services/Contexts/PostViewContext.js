import { createContext, useContext } from "react";
const PostViewContext = createContext(null);
export const usePostView = () => useContext(PostViewContext);
export const PostViewProvider = PostViewContext.Provider;