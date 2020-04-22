import React, { Component } from "react";
import moment from "moment";
import { fetchDocument } from "../../cominoPrintApi";

const LogRow = (props) => {
  return (
    <li>
      {moment(props.date).format("DD/MM/YYYY HH:MM")} - {props.message}
    </li>
  );
};

export default class DocumentView extends Component {
  state = {
    document: null,
  };

  componentDidMount() {
    fetchDocument(Number(this.props.match.params.id), (err, document) => {
      this.setState({ document });
    });
  }

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
          <h2 className="govuk-heading-l">
            Document number <a href="./">{d.id}</a>
          </h2>
          <dl class="govuk-summary-list">
            <div class="govuk-summary-list__row">
              <dt class="govuk-summary-list__key">Created By</dt>
              <dd class="govuk-summary-list__value">{d.sender}</dd>
            </div>
            <div class="govuk-summary-list__row">
              <dt class="govuk-summary-list__key">Original document</dt>
              <dd class="govuk-summary-list__value">
                <a href="#0">Download from Comino</a>
              </dd>
            </div>
            <div class="govuk-summary-list__row">
              <dt class="govuk-summary-list__key">Converted document</dt>
              <dd class="govuk-summary-list__value">
                <a href="#0">View PDF</a>
              </dd>
            </div>
            <div class="govuk-summary-list__row">
              <dt class="govuk-summary-list__key">Logs</dt>
              <dd class="govuk-summary-list__value">
                <ul className="logs">
                  {d.logs.map((log) => (
                    <LogRow {...log} />
                  ))}
                </ul>
              </dd>
            </div>
          </dl>
        </div>

        <div className="lbh-container">
          <button className="govuk-button  lbh-button">
            Approve for sending
          </button>
        </div>
      </div>
    );
  }
}
