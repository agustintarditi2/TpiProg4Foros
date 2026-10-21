export class Post {
  constructor(title, body, userId) {
    this.title = title;
    this.body = body;
    this.userId = userId;
    this.likeCount = 0;
    this.postDate = Date.now()
    this.status = true
  }
}
