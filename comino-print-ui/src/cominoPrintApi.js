// import { hackneyToken } from "./lib/Cookie";
import { dummyDocuments } from "./dummyDocuments";

export const fetchDocuments = (endId, cb) => {
  const limit = 10;
  const docs = endId
    ? dummyDocuments.filter((d) => d.id < endId)
    : dummyDocuments;
  docs.sort((a, b) => a.id < b.id);
  cb(null, docs.slice(0, limit));
  // const req = {
  //   method: "GET",
  //   mode: "cors",
  //   headers: {
  //     Authorization: `Bearer ${hackneyToken}`,
  //   },
  //   searchParams: {
  //     endId,
  //   },
  // };

  // fetch(`${process.env.REACT_APP_HN_API_URL}/documents`, req)
  //   .then(async function (response) {
  //     const json = await response.json();
  //     return json;
  //   })
  //   .then(function (myJson) {
  //     cb(null, myJson);
  //   });
};
export const fetchDocument = (id, cb) => {
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
