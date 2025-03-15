const express = require('express');
const router = express.Router();
const mongoose = require('mongoose');

const VerkoopCodeSchema = new mongoose.Schema({
    deviceId: { type: String, required: true },
    code: { type: String, required: true },
    createdAt: { type: Date, default: Date.now, expires: 86400 }
});

const VerkoopCode = mongoose.model("VerkoopCode", VerkoopCodeSchema);

router.get("/redirect", async (req, res) => {
    const { code, deviceId } = req.query;
    
    console.log("Redirect endpoint hit");
    console.log("Query parameters:", req.query);
    console.log("Headers:", req.headers);
    
    try {
        const result = await VerkoopCode.findOneAndUpdate(
            { deviceId },
            { code },
            { upsert: true, new: true }
        );
        
        console.log("Database operation result:", result);

        const userAgent = req.headers['user-agent'].toLowerCase();
        const redirectUrl = userAgent.includes('android')
            ? 'https://play.google.com/store/apps/details?id=com.Mascelli.RijlesPlanner&hl=en-US&ah=MbccWeflwmtbhkBBVOP3guaZc0A'
            : 'https://apps.apple.com/app/jouwapp/id123456789';

        res.redirect(redirectUrl);
    } catch (error) {
        console.error('Error in redirect endpoint:', error);
        res.status(500).json({ 
            error: 'Er ging iets mis bij het opslaan van de code',
            details: error.message 
        });
    }
});

router.get("/api/getCode", async (req, res) => {
    const { deviceId } = req.query;

    try {
        const verkoopCode = await VerkoopCode.findOne({ deviceId });
        
        if (!verkoopCode) {
            return res.status(404).json({ error: 'Geen code gevonden voor dit apparaat' });
        }

        await VerkoopCode.findByIdAndDelete(verkoopCode._id);
        
        res.json({ code: verkoopCode.code });
    } catch (error) {
        console.error('Error retrieving sales code:', error);
        res.status(500).json({ error: 'Er ging iets mis bij het ophalen van de code' });
    }
});

router.get("/testRedirect", (req, res) => {
    res.json({
        message: "Test endpoint working",
        query: req.query,
        headers: req.headers
    });
});

module.exports = router;