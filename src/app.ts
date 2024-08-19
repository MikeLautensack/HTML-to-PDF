import express from "express";
import puppeteer from "puppeteer";

const app = express();
const port = process.env.PORT || 8080;

app.use(express.json());

app.get("/", (req, res) => {
  res.send("Hello World!");
});

app.post("/generate-pdf", async (req, res) => {
  console.log("gen pdf hit...");
  try {
    const { html } = req.body;

    if (!html) {
      return res.status(400).json({ error: "HTML content is required" });
    }

    const browser = await puppeteer.launch();
    const page = await browser.newPage();

    await page.setContent(html);

    const pdf = await page.pdf({ format: "A4", printBackground: true });

    await browser.close();

    res.contentType("application/pdf");
    res.setHeader("Content-Length", pdf.length);
    res.end(pdf, "binary");
  } catch (error: any) {
    console.log("Error generating PDF:", error.message);
    res.status(500).json({ error: "Failed to generate PDF", message: error });
  }
});

app.listen(port, () => {
  console.log(`Server running at ${port}`);
});
