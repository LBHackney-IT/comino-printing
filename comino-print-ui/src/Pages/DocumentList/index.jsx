import React, { Component } from "react";
import moment from "moment";
import { Link } from "react-router-dom";
import { fetchDocuments } from "../../lib/cominoPrintApi";
import { createBrowserHistory } from "history";
const history = createBrowserHistory();

const DocumentRow = (props) => {
  const dateFormat = (date) => moment(date).format("DD/MM/YYYY HH:MM");
  const d = props.document;
  return (
    <tr>
      <td className="govuk-table__cell">
        <Link to={`/documents/${d.id}`}>{d.docNo}</Link>
      </td>
      <td className="govuk-table__cell">{dateFormat(d.created)}</td>
      <td className="govuk-table__cell">
        {d.printed ? dateFormat(d.printed) : null}
      </td>
      <td
        className={`govuk-table__cell ${d.status
          .replace(/ /g, "-")
          .toLowerCase()}`}
      >
        {d.status}
      </td>
      <td className="govuk-table__cell">{dateFormat(d.statusUpdated)}</td>
      <td className="govuk-table__cell">{d.sender}</td>
    </tr>
  );
};

export default class DocumentListPage extends Component {
  state = {
    cursor: null,
    documents: [],
  };

  fetchDocuments = () => {
    fetchDocuments(this.state.cursor, (err, documents) => {
      this.setState({ documents });
    });
  };

  componentDidMount() {
    const params = new URLSearchParams(this.props.location.search);

    this.setState({ cursor: params.cursor }, () => {
      this.fetchDocuments();
    });
  }

  prevPage = () => {
    history.goBack();
  };

  nextPage = () => {
    const cursor = this.state.documents.map((d) => d.id).sort()[0];
    this.setState({ cursor }, () => {
      history.push(`/?cursor=${cursor}`);
      this.fetchDocuments();
    });
    window.scrollTo(0, 0);
  };

  render() {
    return (
      <div className="DocumentListPage">
        <div className="lbh-container">
          <h2 className="govuk-heading-xl">Letters</h2>
          <table className="govuk-table  lbh-table">
            <caption className="govuk-table__caption">
              View sent letters and check their status
            </caption>
            <thead className="govuk-table__head">
              <tr className="govuk-table__row">
                <th scope="col" className="govuk-table__header">
                  Document No
                </th>
                <th scope="col" className="govuk-table__header">
                  Created
                </th>
                <th scope="col" className="govuk-table__header">
                  Printed
                </th>
                <th scope="col" className="govuk-table__header">
                  Status
                </th>
                <th scope="col" className="govuk-table__header">
                  Status updated
                </th>
                <th scope="col" className="govuk-table__header">
                  Sent by
                </th>
              </tr>
            </thead>
            <tbody>
              {this.state.documents.map((doc) => (
                <DocumentRow key={doc.id} document={doc} />
              ))}
            </tbody>
          </table>
        </div>
        <div className="lbh-container">
          {this.state.cursor ? (
            <>
              <a onClick={this.prevPage} href="#0">
                Previous 10 documents
              </a>
              {" | "}
            </>
          ) : null}
          <a onClick={this.nextPage} href="#0">
            Next 10 documents
          </a>
        </div>
      </div>
    );
  }
}
