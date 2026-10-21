import NavBar from "./NavBar.jsx";

const Header = () => {
  const SpacerStyle = {
    paddingBottom: "72px",
  };

  return (
    <>
      <section className="fixed-top">
        <NavBar />
      </section>

      <section style={SpacerStyle} />
    </>
  );
};

export default Header;