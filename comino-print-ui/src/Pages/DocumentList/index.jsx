import React, { Component } from "react";
import moment from "moment";
import { Link } from "react-router-dom";
import { fetchDocuments } from "../../lib/cominoPrintApi";

const DocumentRow = (props) => {
  const dateFormat = (date) => moment(date).format("DD/MM/YYYY HH:mm");
  const d = props.document;
  return (
    <tr>
      <td className="govuk-table__cell">
        <Link to={`/documents/${d.id}`}>{d.docNo}</Link>
      </td>
      <td className="govuk-table__cell">{dateFormat(d.created)}</td>
      <td
        className={`govuk-table__cell ${d.status
          .replace(/ /g, "-")
          .toLowerCase()}`}
      >
        {d.status}
      </td>
      <td className="govuk-table__cell">{d.sender}</td>
      <td className="govuk-table__cell">{d.letterType}</td>
    </tr>
  );
};

export default class DocumentListPage extends Component {
  state = {
    cursor: undefined,
    documents: [],
  };

  fetchDocuments = () => {
    const cursor = new URLSearchParams(this.props.location.search).get(
      "cursor"
    );
    if (cursor !== this.state.cursor) {
      this.setState({ cursor }, () => {
        fetchDocuments(cursor, (err, documents) => {
          this.setState({ documents });
          window.scrollTo(0, 0);
        });
      });
    }
  };

  componentDidUpdate() {
    this.fetchDocuments();
  }

  componentDidMount() {
    this.fetchDocuments();
  }

  prevPage = (e) => {
    this.props.history.goBack();
    e.preventDefault();
    return false;
  };

  nextPage = (e) => {
    const cursor = this.state.documents.map((d) => d.id).sort()[0];
    this.props.history.push(`/?cursor=${cursor}`);
    e.preventDefault();
    return false;
  };

  render() {
    return (
      <div className="DocumentListPage">
        <div className="lbh-container">
          <h2 className="govuk-heading-xl">Letters</h2>
          <table className="govuk-table  lbh-table">
            <caption className="govuk-table__caption">
              View sent letters and check their status (page last updated on {moment().format("DD/MM/YYYY HH:mm")}))
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
                  Status
                </th>
                <th scope="col" className="govuk-table__header">
                  Sent by
                </th>
                <th scope="col" className="govuk-table__header">
                  Letter type
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
            <a onClick={this.prevPage} href="#0">
              Previous 10 documents
            </a>
          ) : null}
          {this.state.cursor && this.state.documents.length === 10 ? (
            <>{" | "}</>
          ) : null}
          {this.state.documents.length === 10 ? (
            <a onClick={this.nextPage} href="#0">
              Next 10 documents
            </a>
          ) : null}
        </div>
      </div>
    );
  }
}
