import React, { Component } from "react";
import moment from "moment";
import { Link } from "react-router-dom";
import {
  fetchDocument,
  approveDocument,
  cancelDocument,
} from "../../lib/cominoPrintApi";
// import { createBrowserHistory } from "history";
// const history = createBrowserHistory();

const LogRow = (props) => {
  return (
    <li>
      {moment(props.date).format("DD/MM/YYYY HH:MM")} - {props.message}
    </li>
  );
};

export default class DocumentView extends Component {
  state = {
    updating: false,
    document: null,
  };

  componentDidMount() {
    fetchDocument(this.props.match.params.id, (err, document) => {
      this.setState({ document });
    });
  }

  approve = () => {
    this.setState({ updating: true }, () => {
      approveDocument(this.props.match.params.id, () => {
        fetchDocument(this.props.match.params.id, (err, document) => {
          this.setState({ document, updating: false });
        });
      });
    });
  };

  cancel = () => {
    this.setState({ updating: true }, () => {
      cancelDocument(this.props.match.params.id, () => {
        fetchDocument(this.props.match.params.id, (err, document) => {
          this.setState({ document, updating: false });
        });
      });
    });
  };

  render() {
    const d = this.state.document;
    if (!d)
      return (
        <div className="DocumentViewPage">
          <div className="lbh-container">
            <h2 className="govuk-heading-l">Loading document</h2>
          </div>
        </div>
      );

    return (
      <div className="DocumentViewPage">
        <div className="lbh-container">
          <Link to="/" className="govuk-back-link lbh-back-link">
            Back
          </Link>
        </div>
        <div className="lbh-container">
          <h2 className="govuk-heading-l">
            Document number <a href="./">{d.docNo}</a>
          </h2>
          <dl className="govuk-summary-list">
            <div className="govuk-summary-list__row">
              <dt className="govuk-summary-list__key">Created By</dt>
              <dd className="govuk-summary-list__value">{d.sender}</dd>
            </div>
            <div className="govuk-summary-list__row">
              <dt className="govuk-summary-list__key">Original document</dt>
              <dd className="govuk-summary-list__value">
                <a
                  href={`${process.env.REACT_APP_DOCUMENT_API_URL}/hncomino/documents/${d.docNo}/view`}
                  rel="noopener noreferrer"
                  target="_blank"
                >
                  Download from Comino
                </a>
              </dd>
            </div>
            <div className="govuk-summary-list__row">
              <dt className="govuk-summary-list__key">Converted document</dt>
              <dd className="govuk-summary-list__value">
                <a
                  href={`${process.env.REACT_APP_API_URL}/documents/${d.id}/view`}
                  rel="noopener noreferrer"
                  target="_blank"
                >
                  View PDF
                </a>
              </dd>
            </div>
            <div className="govuk-summary-list__row">
              <dt className="govuk-summary-list__key">Logs</dt>
              <dd className="govuk-summary-list__value">
                <ul className="logs">
                  {d.logs.map((log) => (
                    <LogRow key={log.date} {...log} />
                  ))}
                </ul>
              </dd>
            </div>
          </dl>
        </div>

        <div className="lbh-container buttons">
          {d.status === "Waiting for Approval" ? (
            <button
              onClick={this.approve}
              className="govuk-button  lbh-button"
              disabled={this.state.updating}
            >
              Approve for sending
            </button>
          ) : null}
          {d.status === "Waiting for Approval" ||
          d.status === "Ready for Gov Notify" ? (
            <button
              onClick={this.cancel}
              name="Warning"
              className="govuk-button  lbh-button govuk-button--warning lbh-button--warning"
              disabled={this.state.updating}
            >
              Cancel sending
            </button>
          ) : null}
        </div>
      </div>
    );
  }
}
