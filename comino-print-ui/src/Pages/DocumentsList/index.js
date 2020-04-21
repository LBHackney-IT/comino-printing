import React, { Component } from "react";
import moment from "moment";
import { Link } from "react-router-dom";
import { fetchDocuments } from "../../cominoPrintApi";

const DoumentRow = (props) => {
  const dateFormat = (date) => moment(date).format("DD/MM/YYYY HH:MM");
  const d = props.document;
  return (
    <tr>
      <td className="govuk-table__cell">{d.id}</td>
      <td className="govuk-table__cell">{dateFormat(d.created)}</td>
      <td className="govuk-table__cell">{dateFormat(d.printed)}</td>
      <td className="govuk-table__cell">{d.status}</td>
      <td className="govuk-table__cell">{dateFormat(d.statusUpdated)}</td>
      <td className="govuk-table__cell">{d.sender}</td>
      <td className="govuk-table__cell">View Letter</td>
      <td className="govuk-table__cell">
        {d.status === "Failed" ? (
          <Link to={`/documents/${d.id}`}>Errors</Link>
        ) : (
          "Cancel"
        )}
      </td>
    </tr>
  );
};

export default class DocumentListPage extends Component {
  state = {
    endId: null,
    documents: [],
  };

  componentDidMount() {
    fetchDocuments(this.state.endId, (err, documents) => {
      this.setState({ documents });
    });
  }

  render() {
    return (
      <div className="lbh-container row details">
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
              <th scope="col" className="govuk-table__header"></th>
              <th scope="col" className="govuk-table__header"></th>
              <th scope="col" className="govuk-table__header"></th>
            </tr>
          </thead>
          <tbody>
            {this.state.documents.map((doc) => (
              <DoumentRow document={doc} />
            ))}
          </tbody>
        </table>
      </div>
    );
  }
}
