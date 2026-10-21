import { useNavigate } from "react-router-dom"

const Banner = () => {
    const navigate = useNavigate()
    const BannerStyle = {
        backgroundImage: "url('/narrowBanner.png')",
        backgroundRepeat:'no-repeat',
        backgroundSize:'cover',
        margin:'0, 0, 0, 0',
        height:'10vw',
        maxHeight:'100px',
        padding:'0px',
        backgroundPosition: '70% 50%',
        backgroundClip: 'border-box',
    }
    return <div
      style={BannerStyle}
      title="GRAN FORO DE LA TUP: Un sitio de foros para la Tecnicatura Universitaria en Programación de la Universidad Tecnológica Nacional"
      onClick={() => navigate("/main", { replace: true })}
    />
}



export default Banner