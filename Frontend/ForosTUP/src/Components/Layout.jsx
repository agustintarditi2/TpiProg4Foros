import {Outlet} from 'react-router-dom'
import Footer from './Footer.jsx'
import Header from './Header.jsx'
import { ToastContainer } from "react-toastify";

const Layout = () => {
    return <>
        <Header />
            <Outlet />
        <Footer />
        <ToastContainer
        position="top-center"
        autoClose={3000}
        pauseOnHover
        theme="colored"
    />
    </>
}


export default Layout