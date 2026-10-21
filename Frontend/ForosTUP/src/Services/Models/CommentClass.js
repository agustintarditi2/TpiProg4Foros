export class Comment {
  constructor(text, userId){
    this.text = text
    this.userId = userId
    this.postDate = Date.now()
    this.likeCount = 0
  }
}
