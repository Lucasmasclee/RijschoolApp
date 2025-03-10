const express = require('express');
const router = express.Router();

router.get('/redirect', (req, res) => {
    const code = req.query.code;

    if (!code) {
        return res.status(400).send("Geen code opgegeven.");
    }

    // Zet een HTTP cookie met de code (blijft 30 dagen geldig)
    res.cookie('plannerCode', code, { maxAge: 30 * 24 * 60 * 60 * 1000, httpOnly: false });
    
    // Remove the res.send(code) line since we're redirecting
    res.redirect("https://play.google.com/apps/testing/com.Mascelli.RijlesPlanner");
});

module.exports = router;