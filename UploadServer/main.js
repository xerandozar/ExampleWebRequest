const express = require("express");
const multer = require("multer");

const app = express();
const port = 8080;

const basicAuth = (request, response, next) => {
  const auth = request.headers["authorization"];
  if (!auth) {
    return response
      .status(401)
      .json({ success: false, message: "Unauthorized" });
  }

  const credentials = Buffer.from(auth.split(" ")[1], "base64")
    .toString("utf8")
    .split(":");

  const [username, password] = credentials;

  if (username === "admin" && password === "password") {
    return next();
  }

  response.status(401).json({ success: false, message: "Unauthorized" });
};

const storage = multer.memoryStorage();
const upload = multer({
  storage,
  fileFilter: (_, file, cb) => {
    const allowedTypes = ["image/jpeg", "image/png", "image/gif"];
    if (!allowedTypes.includes(file.mimetype)) {
      return cb(new Error("Only images are allowed"), false);
    }

    cb(null, true);
  },
});

app.post("/upload", basicAuth, upload.single("image"), (request, response) => {
  if (!request.file) {
    return response
      .status(400)
      .json({ success: false, message: "No file uploaded" });
  }
  console.log(`Uploaded file size: ${request.file.size} bytes`);
  response.json({
    success: true,
    message: "File received successfully",
    fileSize: request.file.size,
  });
});

app.listen(port, () => {
  console.log(`Server running on port ${port}`);
});
