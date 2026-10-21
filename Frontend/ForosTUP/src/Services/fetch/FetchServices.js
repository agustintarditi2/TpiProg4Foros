//import { useState, useEffect } from "react"

// getPostFromId devuelve el post como objeto JS tras buscarlo de la API
// mediante el método del Backend getPostById. ¡Cuidado con no confundirlos!
/*     export const getPostFromId = async (id) => {
        const post = await fetch(`https://localhost:3000/posts/:${id}`)
            .then( (res) => !res.ok ? res.json() : console.error("Error: No se pudo recuperar la información.") )
            .catch( (err)=> err)
        return post
    }
    export const getPostsByForumId = async (forum) => {
        const postList = await fetch(`https://localhost:3000/posts/${forum}`)
            .then( (res) => !res.ok ? res.json() : console.error("Error: No se pudo recuperar la información.") )
            .catch( (err)=> err)
        return postList
    }
    export const findInPostList = (postSet, postId) => {
        const foundPost = postSet.filter((item)=>{
            return item.id===postId
        })
        return foundPost
    }
    export const getUserById = async (userId) => {
        const user = await fetch(`https://localhost:3000/users/:${id}`)
            .then( (res) => !res.ok ? res.json() : console.error("Error: No se pudo recuperar la información.") )
            .catch( (err)=> err)
        return user
    }
    export const getCommentsByPostId = async (postId) => {
        const comments = await fetch(`https://localhost:3000/comments`)
            .then( (res) => !res.ok ? res.json() : console.error("Error: No se pudo recuperar la información.") )
            .catch( (err)=> err)
        return postList
    }
    export const getForums = async () => {
        const forumList = await fetch(`https://localhost:3000/forums`)
            .then( (res) => !res.ok ? res.json() : console.error("Error: No se pudo recuperar la información.") )
            .catch( (err)=> err)
        return forumList
    }

    export const createPost = () => {
        return
    }

    export const createComment = () => {
        return
    }

    export const createUser = () => {
        return
    }

    export const createForum = () => {
        return
    }

    export const deletePost = () => {
        return
    }

    export const deleteComment = () => {
        return
    }

    export const deleteUser = () => {
        return
    }

    export const deleteForum = () => {
        return
    }
 */





    // reemplacé este código con el custom hook useFetchFromAPI.
    // Por eso está comentado.