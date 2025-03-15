const express = require("express");
const mongoose = require("mongoose");
const cors = require("cors");
require("dotenv").config();
const cookieParser = require('cookie-parser');

const app = express();
app.use(cookieParser());

// Middleware
app.use(express.json());
app.use(cors());

const redirectRoute = require('./routes/redirect');
const getPlannerCodeRoute = require('./routes/getPlannerCode');

app.use(redirectRoute);
app.use(getPlannerCodeRoute);

// Connect to MongoDB
mongoose.connect(process.env.MONGO_URI)
    .then(() => {
        console.log("MongoDB successfully connected");
        console.log("Connection string:", process.env.MONGO_URI.replace(/:[^:]*@/, ':****@'));
    })
    .catch(err => {
        console.error("MongoDB connection error:", err);
        console.error("Connection string used:", process.env.MONGO_URI.replace(/:[^:]*@/, ':****@'));
    });

// Les Schema
const LesSchema = new mongoose.Schema({
    begintijd: { type: String, required: true },
    eindtijd: { type: String, required: true },
    notities: { type: String },
    datum: { type: String, required: true },  // Format: "dd-MM-yyyy"
    weekNummer: { type: Number, required: true },
    leerlingId: { type: String },  // Reference to student
    leerlingNaam: { type: String }, // Store student name for easier access
    gereserveerdDoorLeerling: [{
        naam: { type: String, required: true },
        frequentie: { type: Number, required: true },
        colorIndex: { type: Number, required: true }
    }]
});

// Update the schemas to include availability
const TimeSlotSchema = new mongoose.Schema({
    startTijd: { type: String, required: true },
    eindTijd: { type: String, required: true }
});

const BeschikbaarheidSchema = new mongoose.Schema({
    dag: { type: String, required: true },
    weekNummer: { type: Number, required: true },
    jaar: { type: Number, required: true },
    tijdslots: [TimeSlotSchema]
});

// Week Schema
const WeekSchema = new mongoose.Schema({
    weekNummer: { type: Number, required: true },
    jaar: { type: Number, required: true },
    lessen: [LesSchema]  // Array of lessons for this week
});

// Rooster Schema
const RoosterSchema = new mongoose.Schema({
    weken: [WeekSchema]
});

// Leerling Schema
const LeerlingSchema = new mongoose.Schema({
    naam: { type: String, required: true },
    frequentie: { type: Number, required: true },
    colorIndex: { type: Number, required: true },
    minutesPerLes: { type: Number, default: 60 },
    beschikbaarheid: [BeschikbaarheidSchema],
    woonPlaats: { type: String },  // New field
    wachtwoord: { type: String }   // New field
});

// Rijschool Schema
const RijschoolSchema = new mongoose.Schema({
    naam: { type: String, required: true },
    beschrijving: { type: String },
    wachtwoord: { type: String, required: true },
    woonPlaats: { type: String },  // New field
    leerlingen: [LeerlingSchema],
    rooster: {
        weken: [{
            lessen: [LesSchema],
            weekNummer: { type: Number, required: true },
            jaar: { type: Number, required: true }
        }]
    },
    instructeurBeschikbaarheid: [BeschikbaarheidSchema]
});

const Rijschool = mongoose.model("Rijschool", RijschoolSchema);

// API-routes
app.post("/api/rijscholen", async (req, res) => {
    try {
        console.log("Received data:", req.body);  // Debug log
        const nieuweRijschool = new Rijschool(req.body);
        await nieuweRijschool.save();
        res.status(201).json(nieuweRijschool);
    } catch (error) {
        console.error("Post error:", error);
        res.status(500).json({ error: "Er is iets misgegaan" });
    }
});

app.get("/api/rijscholen", async (req, res) => {
    try {
        console.log("Attempting to fetch rijscholen...");
        const rijscholen = await Rijschool.find();
        console.log(`Found ${rijscholen.length} rijscholen`);
        res.json(rijscholen);
    } catch (error) {
        console.error("Error in GET /api/rijscholen:", error);
        res.status(500).json({ 
            error: "Er is iets misgegaan", 
            details: error.message,
            stack: error.stack 
        });
    }
});

app.put("/api/rijscholen/:naam", async (req, res) => {
    try {
        console.log("Updating rijschool:", req.params.naam);
        console.log("Update data:", req.body);
        console.log("Instructor availability:", req.body.instructeurBeschikbaarheid);

        const updatedRijschool = await Rijschool.findOneAndUpdate(
            { naam: req.params.naam },
            req.body,
            { 
                new: true,
                runValidators: true
            }
        );

        if (!updatedRijschool) {
            return res.status(404).json({ error: "Rijschool niet gevonden" });
        }

        console.log("Updated rijschool:", updatedRijschool);
        res.json(updatedRijschool);
    } catch (error) {
        console.error("Update error:", error);
        res.status(500).json({ error: "Er is iets misgegaan bij het updaten" });
    }
});

// Delete endpoint (optional but recommended)
app.delete("/api/rijscholen/:naam", async (req, res) => {
    try {
        const deletedRijschool = await Rijschool.findOneAndDelete({ naam: req.params.naam });
        
        if (!deletedRijschool) {
            return res.status(404).json({ error: "Rijschool niet gevonden" });
        }

        res.json({ message: "Rijschool succesvol verwijderd" });
    } catch (error) {
        console.error("Delete error:", error);
        res.status(500).json({ error: "Er is iets misgegaan bij het verwijderen" });
    }
});

// Get single rijschool endpoint (optional but useful for debugging)
app.get("/api/rijscholen/:naam", async (req, res) => {
    try {
        const rijschool = await Rijschool.findOne({ naam: req.params.naam });
        
        if (!rijschool) {
            return res.status(404).json({ error: "Rijschool niet gevonden" });
        }

        res.json(rijschool);
    } catch (error) {
        res.status(500).json({ error: "Er is iets misgegaan" });
    }
});

// Add a basic root route
app.get("/", (req, res) => {
    res.json({ message: "Rijschool API is running" });
});

app.get("/clearPlannerCode", (req, res) => {
    res.clearCookie("plannerCode");
    res.json({ message: "Planner code cleared" });
});

// Schema voor verkoperscodes
const VerkoopCodeSchema = new mongoose.Schema({
    deviceId: { type: String, required: true },
    code: { type: String, required: true },
    createdAt: { type: Date, default: Date.now, expires: 86400 } // Verloopt na 24 uur
});

const VerkoopCode = mongoose.model("VerkoopCode", VerkoopCodeSchema);

// Route voor het opslaan van de verkoopcode
app.get("/redirect", async (req, res) => {
    const { code, deviceId } = req.query;
    
    console.log("Redirect endpoint hit");
    console.log("Query parameters:", req.query);
    console.log("Headers:", req.headers);
    
    try {
        // Log vóór database operatie
        console.log("Attempting to save code to database...");
        
        const result = await VerkoopCode.findOneAndUpdate(
            { deviceId },
            { code },
            { upsert: true, new: true }
        );
        
        // Log na database operatie
        console.log("Database operation result:", result);

        const userAgent = req.headers['user-agent'].toLowerCase();
        const redirectUrl = userAgent.includes('android')
            ? 'https://play.google.com/store/apps/details?id=com.Mascelli.RijlesPlanner&hl=en-US&ah=MbccWeflwmtbhkBBVOP3guaZc0A'
            : 'https://apps.apple.com/app/jouwapp/id123456789';

        console.log("Redirecting to:", redirectUrl);
        res.redirect(redirectUrl);
    } catch (error) {
        console.error('Error in redirect endpoint:', error);
        res.status(500).json({ 
            error: 'Er ging iets mis bij het opslaan van de code',
            details: error.message 
        });
    }
});

// Route voor Unity om de code op te halen
app.get("/api/getCode", async (req, res) => {
    const { deviceId } = req.query;

    try {
        const verkoopCode = await VerkoopCode.findOne({ deviceId });
        
        if (!verkoopCode) {
            return res.status(404).json({ error: 'Geen code gevonden voor dit apparaat' });
        }

        // Verwijder de code na succesvol ophalen
        await VerkoopCode.findByIdAndDelete(verkoopCode._id);
        
        res.json({ code: verkoopCode.code });
    } catch (error) {
        console.error('Error retrieving sales code:', error);
        res.status(500).json({ error: 'Er ging iets mis bij het ophalen van de code' });
    }
});

// Test endpoint
app.get("/testcode", async (req, res) => {
    const { code, deviceId } = req.query;
    
    try {
        const result = await VerkoopCode.findOneAndUpdate(
            { deviceId },
            { code },
            { upsert: true, new: true }
        );
        
        res.json({ 
            message: "Code saved successfully", 
            savedCode: result 
        });
    } catch (error) {
        console.error('Test endpoint error:', error);
        res.status(500).json({ 
            error: 'Error saving code',
            details: error.message 
        });
    }
});

// Add error handling middleware
app.use((err, req, res, next) => {
    console.error("Global error handler:", err);
    res.status(500).json({
        error: "Er is iets misgegaan",
        details: err.message,
        stack: err.stack
    });
});

// Start server
const PORT = process.env.PORT || 5000;
app.listen(PORT, () => {
    console.log(`🚀 Server running on port ${PORT}`);
    console.log(`Environment: ${process.env.NODE_ENV}`);
    console.log(`MongoDB URI set: ${process.env.MONGO_URI ? 'Yes' : 'No'}`);
});