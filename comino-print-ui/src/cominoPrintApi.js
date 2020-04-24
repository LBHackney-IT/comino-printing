// import { dummyDocuments } from "./dummyDocuments";
import { hackneyToken } from "./lib/Cookie";
const limit = 10;
const allDocs = {};

export const fetchDocuments = (cursor, cb) => {
  const options = {
    method: "GET",
    mode: "cors",
    headers: {
      Authorization: `Bearer ${hackneyToken()}`,
    },
    searchParams: {
      cursor,
      limit,
    },
  };
  let cursorStr = cursor ? `&cursor=${cursor}` : "";

  fetch(
    `${process.env.REACT_APP_API_URL}/documents?limit=10${cursorStr}`,
    options
  ).then(async (response) => {
    const json = await response.json();
    json.documents.forEach((doc) => (allDocs[doc.id] = doc));
    cb(null, json.documents);
  });
};
export const fetchDocument = (id, cb) => {
  cb(null, allDocs[id]);
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
