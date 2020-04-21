import React from "react";
import Header from "./Components/Header";
import Phase from "./Components/Phase";
import PrivateRoute from "./Components/PrivateRoute";
import { BrowserRouter as Router, Route } from "react-router-dom";
import DocumentsListPage from "./Pages/DocumentsList";
import LoginPage from "./Pages/LoginPage";
import "./css/styles.scss";

function App() {
  return (
    <>
      <Header />
      <Phase />
      <Router>
        <Route path="/login" component={LoginPage} />
        <PrivateRoute path="/" exact component={DocumentsListPage} />
      </Router>
    </>
  );
}

export default App;
