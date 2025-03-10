const express = require('express');
const router = express.Router();

router.get('/getPlannerCode', (req, res) => {
    const plannerCode = req.cookies.plannerCode;

    if (!plannerCode) {
        return res.send("Geen code gevonden");
    }

    res.send(plannerCode);
});

module.exports = router;
