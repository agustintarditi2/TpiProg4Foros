import notFoundImage from "../assets/notFound/img1.jpg";
import { Button, ButtonGroup } from 'react-bootstrap';
import { useNavigate } from "react-router-dom"

const NotFound = () => {
    const navigate = useNavigate();
    const goToLogin = () => {navigate("/login")}
    const goToMain = () => {navigate("/main")}
    return (<div className="text-center mt-3" >
    <img src={notFoundImage} alt="Página no encontrada" width="60%" />
    <h1>¡Mirá dónde te viniste a meter!</h1>
    <h3>Te soy sincero, no tengo idea de como hiciste para terminar acá. <br/> Felicitaciones, che.</h3>
    <ButtonGroup className='my-3 py-5' size='lg'>
    <Button variant="primary" className='mx-2 py-3' onClick={goToMain} >Volver a la página de inicio</Button>
    <Button variant="secondary"className='mx-2 py-3'  onClick={goToLogin}>Ir al inicio de sesión</Button></ButtonGroup>
    </div>
    )
}

export default NotFound;