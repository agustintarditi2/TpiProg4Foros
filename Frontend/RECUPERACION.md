# Paso 1: recuperación del frontend original

Origen: https://github.com/IMN2400/tpi-prog3-2tup2-2026

Commit de origen: `1345d5f2a91c792e8f7a1ee9463ab15001adde63`.

Se recuperaron 33 archivos ausentes: pantallas, estilos, contexto de autenticación, hooks, servicios auxiliares e imágenes. Los archivos se copiaron sin modificar su contenido y se verificaron byte por byte. Los componentes se ubicaron en la carpeta `Components` existente y los hooks en `Services/Hooks`. No se sobrescribió ningún archivo existente ni se incorporó el backend Node o su base de datos.

## Pendientes para el paso 2

- Ajustar imports a las ubicaciones actuales, incluyendo mayúsculas y minúsculas.
- Revisar dependencias de frontend; no copiar las dependencias de servidor incluidas en el package.json original.
- Conectar rutas y AuthProvider al arranque y recuperar los estilos globales necesarios.
- Revisar las dos implementaciones de contexto de autenticación recuperadas y conservar una sola en la reorganización.
- Resolver la referencia a `usersList/UserList`: ese archivo tampoco está presente en el commit original examinado.
- Comprobar compilación y navegación una vez conectadas las pantallas. Un build de la plantilla Vite actual no valida estos archivos.

## Alcance

Esta recuperación no constituye una integración funcional. Los archivos conservan las rutas HTTP y contratos del backend Node original. La adaptación a .NET y la validación funcional se realizarán en los pasos siguientes.

## Paso 2: arranque y referencias

Se corrigieron los imports para la estructura actual, se conectaron App y las rutas con AuthProvider, y se agregaron las dependencias de interfaz necesarias con su lockfile. El login actualiza el contexto de sesión. Se conserva un único proveedor de autenticación: `src/context/AuthContext.jsx`; se eliminaron los dos archivos de la implementación alternativa incompleta y sin consumidores en `Services/auth`.

Se restauró el fondo global del frontend original, se retiraron los estilos de la plantilla Vite que interferían con las pantallas, se conectaron los CSS de login, registro e inicio y se corrigió la imagen del 404. También se corrigieron el export de LesserHeader y la declaración de ForumClass.

La ruta `/users` se retiró porque su componente no existe. No se implementó un listado alternativo. Las rutas HTTP y contratos siguen siendo los de Node en localhost:3000. No hay integración con .NET en este paso.

Validación: `npm run build` completado; `npm run lint` sin errores bloqueantes, con advertencias heredadas. La compilación advierte sobre el tamaño del bundle; queda para una optimización posterior.

Para ejecutar, desde `Frontend/Foros TUP`: `npm ci` y `npm run dev`. Se requiere una versión de Node compatible con Vite 8 (20.19+ o 22.12+; se verificó con Node 24). Sin el backend antiguo, las páginas de datos muestran errores de conexión; login y registro pueden visualizarse, pero no autenticar ni guardar.

No se completó la prueba automatizada en navegador: el entorno no tenía Chromium y su descarga falló. Queda por verificar visualmente la navegación y las pantallas en el equipo local.
