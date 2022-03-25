const express = require("express");
//here we are configuring dist to serve app files
const app = express();
// bind the request to an absolute path or relative to the CWD
app.use(express.static("dist"));

const port = process.env.PORT || 8085;
app.listen(port, () => {});//console.log(`Listening on port ${port}`));