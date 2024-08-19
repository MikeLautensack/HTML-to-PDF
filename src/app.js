"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
const express_1 = __importDefault(require("express"));
const puppeteer_1 = __importDefault(require("puppeteer"));
const app = (0, express_1.default)();
const port = process.env.PORT || 8080;
app.use(express_1.default.json());
app.get("/", (req, res) => {
    res.send("Hello World!");
});
app.post("/generate-pdf", (req, res) => __awaiter(void 0, void 0, void 0, function* () {
    console.log("gen pdf hit...");
    try {
        const { html } = req.body;
        if (!html) {
            return res.status(400).json({ error: "HTML content is required" });
        }
        const browser = yield puppeteer_1.default.launch();
        const page = yield browser.newPage();
        yield page.setContent(html);
        const pdf = yield page.pdf({ format: "A4", printBackground: true });
        yield browser.close();
        res.contentType("application/pdf");
        res.setHeader("Content-Length", pdf.length);
        res.end(pdf, "binary");
    }
    catch (error) {
        console.log("Error generating PDF:", error);
        res.status(500).json({ error: "Failed to generate PDF" });
    }
}));
app.listen(port, () => {
    console.log(`Server running at ${port}`);
});
