const express = require("express");
const mongoose = require("mongoose");
const cors = require("cors");
require("dotenv").config();

const app = express();

// Middleware
app.use(express.json());
app.use(cors());

// Verbind met MongoDB
mongoose.connect(process.env.MONGO_URI)
    .then(() => console.log("MongoDB verbonden"))
    .catch(err => console.error("MongoDB fout:", err));

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

// Rijschool Schema
const RijschoolSchema = new mongoose.Schema({
    naam: { type: String, required: true },
    beschrijving: { type: String, required: true },
    wachtwoord: { type: String, required: true },
    leerlingen: [{
        naam: { type: String, required: true },
        frequentie: { type: Number, required: true },
        colorIndex: { type: Number, required: true }
    }],
    rooster: {
        weken: [WeekSchema]
    }
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
        const rijscholen = await Rijschool.find();
        res.json(rijscholen);
    } catch (error) {
        res.status(500).json({ error: "Er is iets misgegaan" });
    }
});

app.put("/api/rijscholen/:naam", async (req, res) => {
    try {
        console.log("Updating rijschool:", req.params.naam);
        console.log("Update data:", req.body);

        const updatedRijschool = await Rijschool.findOneAndUpdate(
            { naam: req.params.naam },
            req.body,
            { 
                new: true,  // Returns the updated document
                runValidators: true  // Ensures the update follows schema validation
            }
        );

        if (!updatedRijschool) {
            return res.status(404).json({ error: "Rijschool niet gevonden" });
        }

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

// Start server
const PORT = process.env.PORT || 5000;
app.listen(PORT, () => console.log(`🚀 Server draait op poort ${PORT}`));