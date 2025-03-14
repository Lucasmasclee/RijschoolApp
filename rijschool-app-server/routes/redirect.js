const express = require('express');
const router = express.Router();

// router.get('/redirect', (req, res) => {
//     const code = req.query.code;

//     if (!code) {
//         return res.status(400).send("Geen code opgegeven.");
//     }

//     // Validate and set a cookie with the code
//     if (typeof code === 'string' && code.length < 100 && !code.includes('<')) {
//         res.cookie('plannerCode', code, { 
//             maxAge: 30 * 24 * 60 * 60 * 1000, 
//             httpOnly: false,
//             sameSite: 'lax'
//         });
//     }

//     // Redirect to the Google Play Store
//     res.redirect("https://play.google.com/apps/testing/com.Mascelli.RijlesPlanner");
// });

// const express = require('express');
// const router = express.Router();
const uuid = require('uuid'); // Use UUID for unique identifiers

// In-memory storage for demonstration purposes
const plannerCodeStore = {};

router.get('/redirect', (req, res) => {
    const code = req.query.code;

    if (!code) {
        return res.status(400).send("Geen code opgegeven.");
    }

    // Generate a unique identifier
    const uniqueId = uuid.v4();

    // Store the planner code with the unique identifier
    plannerCodeStore[uniqueId] = code;

    // Redirect to the Play Store with the unique identifier as a referral parameter
    res.redirect(`https://play.google.com/apps/testing/com.Mascelli.RijlesPlanner?referrer=${uniqueId}`);
});

module.exports = router;
