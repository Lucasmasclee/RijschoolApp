const express = require('express');
const router = express.Router();

router.get('/redirect', (req, res) => {
    const code = req.query.code;

    if (!code) {
        return res.status(400).send("Geen code opgegeven.");
    }

    // Only store the actual code value, not the entire HTML response
    // Add validation to ensure code is a simple string
    if (typeof code === 'string' && code.length < 100 && !code.includes('<')) {
        res.cookie('plannerCode', code, { 
            maxAge: 30 * 24 * 60 * 60 * 1000, 
            httpOnly: false,
            sameSite: 'lax'  // Add sameSite attribute
        });
    }
    
    res.redirect("https://play.google.com/apps/testing/com.Mascelli.RijlesPlanner");
});

module.exports = router;