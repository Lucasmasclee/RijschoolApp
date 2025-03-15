const express = require('express');
const router = express.Router();

router.get("/redirect", (req, res) => {
    const { code, verkoper } = req.query;
    
    console.log("Redirect endpoint hit");
    console.log("Query parameters:", req.query);
    
    const userAgent = req.headers['user-agent'].toLowerCase();
    const redirectUrl = userAgent.includes('android')
        ? 'https://play.google.com/store/apps/details?id=com.Mascelli.RijlesPlanner&hl=en-US&ah=MbccWeflwmtbhkBBVOP3guaZc0A'
        : 'https://apps.apple.com/app/jouwapp/id123456789';

    // Before redirecting, set the download headers
    res.setHeader('Content-Disposition', 'attachment; filename=salesCode.txt');
    res.setHeader('Content-Type', 'text/plain');
    
    // Send the code as a downloadable file
    res.send(code || verkoper);
});

router.get("/testRedirect", (req, res) => {
    res.json({
        message: "Test endpoint working",
        query: req.query,
        headers: req.headers
    });
});

module.exports = router;