export class Person {
  constructor (nombre, correo, hashedPassword, rol) {
    this.nombre = nombre;
    this.correo = correo;
    this.hashedPassword = hashedPassword;
    this.rol = rol;
    this.estado = true
  }
}
