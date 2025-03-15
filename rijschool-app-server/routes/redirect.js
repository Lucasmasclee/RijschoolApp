const express = require('express');
const router = express.Router();

router.get('/redirect', (req, res) => {
    const code = req.query.code;

    if (!code) {
        console.log("No code provided.");
        return res.status(400).send("Geen code opgegeven.");
    }

    // Set the cookie
    res.cookie('plannerCode', code, { 
        maxAge: 30 * 24 * 60 * 60 * 1000, 
        httpOnly: false,
        sameSite: 'lax',
        secure: false
    });
    // Validate and set a cookie with the code
    if (typeof code === 'string' && code.length < 100 && !code.includes('<')) {
        res.cookie('plannerCode', code, { 
            maxAge: 30 * 24 * 60 * 60 * 1000, 
            httpOnly: false,
            sameSite: 'lax'
        });
    }

    console.log(`Cookie set: plannerCode = ${code}`);
    
    // Redirect to the Google Play Store
    res.redirect("https://play.google.com/apps/testing/com.Mascelli.RijlesPlanner");
});