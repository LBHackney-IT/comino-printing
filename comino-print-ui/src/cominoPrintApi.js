// import { dummyDocuments } from "./dummyDocuments";
import { hackneyToken } from "./lib/Cookie";
let allDocs = null;
const limit = 10;

export const fetchDocuments = (endId, cb) => {
  if (!allDocs) {
    const req = {
      method: "GET",
      mode: "cors",
      headers: {
        Authorization: `Bearer ${hackneyToken()}`,
      },
      searchParams: {
        endId,
        limit,
      },
    };

    fetch(`${process.env.REACT_APP_API_URL}/documents`, req)
      .then(async function (response) {
        const json = await response.json();
        return json;
      })
      .then((docsResult) => {
        allDocs = docsResult.documents;
        cb(null, allDocs.sort((a, b) => a.id < b.id).slice(0, 10));
      });
  } else {
    const docs = endId ? allDocs.filter((d) => d.id < endId) : allDocs;
    docs.sort((a, b) => a.id < b.id);
    cb(null, docs.slice(0, limit));
  }
};
export const fetchDocument = (id, cb) => {
  const doc = allDocs.filter((d) => d.id === id)[0];
  cb(null, doc);
  // const req = {
  //   method: "GET",
  //   mode: "cors",
  //   headers: {
  //     Authorization: `Bearer ${hackneyToken}`,
  //   },
  // };
  // fetch(`${process.env.REACT_APP_HN_API_URL}/documents/${id}`, req)
  //   .then(async function (response) {
  //     const json = await response.json();
  //     return json;
  //   })
  //   .then(function (myJson) {
  //     cb(null, myJson);
  //   });
};
