const express = require('express');
const router = express.Router();

router.get('/redirect', (req, res) => {
    const code = req.query.code;

    if (!code) {
        return res.status(400).send("Geen code opgegeven.");
    }

    // Set the cookie with appropriate attributes
    res.cookie('plannerCode', code, { 
        maxAge: 30 * 24 * 60 * 60 * 1000, 
        httpOnly: false,
        sameSite: 'lax',  // Adjust as needed
        secure: false,     // Set to true if using HTTPS
        domain: 'rijschool.nl'
    });
    
    res.redirect("https://play.google.com/apps/testing/com.Mascelli.RijlesPlanner");
});

module.exports = router;