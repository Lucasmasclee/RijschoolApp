const express = require('express');
const router = express.Router();

router.get('/getPlannerCode', (req, res) => {
    // Add cache control headers
    res.set('Cache-Control', 'no-cache, no-store, must-revalidate');
    res.set('Pragma', 'no-cache');
    res.set('Expires', '0');
    
    const plannerCode = req.cookies.plannerCode;

    if (!plannerCode) {
        return res.send("Geen code gevonden");
    }

    // Validate that we're not sending HTML content
    if (typeof plannerCode === 'string' && plannerCode.length < 100 && !plannerCode.includes('<')) {
        res.send(plannerCode);
    } else {
        res.send("Ongeldige code gevonden");
    }
});

module.exports = router;
