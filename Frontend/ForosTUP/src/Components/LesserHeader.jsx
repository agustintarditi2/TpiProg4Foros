
const LesserHeader = ({ headerName }) => {
    const LesserHeaderStyle = {
        backgroundColor: '#7a8',
        borderWidth:'3px',
        borderColor:'#296',
        margin:'0, 0, 0, 0',
        height:'5vw',
        maxHeight:'100px',
        padding:'20px',
        textAlign:'center',
    }
    return <div style={LesserHeaderStyle} title={headerName}>{headerName}
        </div>
}



export default LesserHeader;