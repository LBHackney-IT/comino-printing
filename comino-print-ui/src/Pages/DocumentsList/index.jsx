import React, { Component } from "react";
import moment from "moment";
import { Link } from "react-router-dom";
import { fetchDocuments } from "../../cominoPrintApi";

const DoumentRow = (props) => {
  const dateFormat = (date) => moment(date).format("DD/MM/YYYY HH:MM");
  const d = props.document;
  return (
    <tr>
      <td className="govuk-table__cell">
        <Link to={`/documents/${d.id}`}>{d.id}</Link>
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
    endId: null,
    documents: [],
  };

  fetchDocuments = (cb) => {
    fetchDocuments(this.state.endId, (err, documents) => {
      this.setState({ documents });
      cb && cb();
    });
  };

  componentDidMount() {
    this.fetchDocuments();
  }

  nextPage = () => {
    const endId = this.state.documents.map((d) => d.id).sort()[0];
    this.setState({ endId }, () => {
      this.fetchDocuments(() => {
        window.scrollTo(0, 0);
      });
    });
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
                <DoumentRow document={doc} />
              ))}
            </tbody>
          </table>
        </div>
        <div className="lbh-container">
          <a onClick={this.nextPage} href="#0">
            Next 10 documents
          </a>
        </div>
      </div>
    );
  }
}
