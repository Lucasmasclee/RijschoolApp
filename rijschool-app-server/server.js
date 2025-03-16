const express = require("express");
const mongoose = require("mongoose");
const cors = require("cors");
require("dotenv").config();
const shortid = require('shortid');
const qr = require('qrcode');


const app = express();

// Middleware
app.use(express.json());
app.use(cors());


// Verbind met MongoDB
// Connect to MongoDB
mongoose.connect(process.env.MONGO_URI)
    .then(() => {
        console.log("MongoDB successfully connected");
        console.log("Connection string:", process.env.MONGO_URI.replace(/:[^:]*@/, ':****@')); // Hide password in logs
        console.log("Connection string:", process.env.MONGO_URI.replace(/:[^:]*@/, ':****@'));
    })
    .catch(err => {
        console.error("MongoDB connection error:", err);
        console.error("Connection string used:", process.env.MONGO_URI.replace(/:[^:]*@/, ':****@')); // Hide password in logs
        console.error("Connection string used:", process.env.MONGO_URI.replace(/:[^:]*@/, ':****@'));
    });

// Les Schema
const LesSchema = new mongoose.Schema({
    begintijd: { type: String, required: true },
    eindtijd: { type: String, required: true },
    notities: { type: String, default: "" },
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
    woonPlaats: { type: String },
    adres: { type: String },  // New field
    wachtwoord: { type: String },
    woonPlaats: { type: String },  // New field
    wachtwoord: { type: String }   // New field
});

// Rijschool Schema
const RijschoolSchema = new mongoose.Schema({
    naam: { type: String, required: true },
    beschrijving: { type: String },
    wachtwoord: { type: String, required: true },
    woonPlaats: { type: String },
    LLzienLessen: { type: Boolean, default: false },  // New field
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

// Schema voor het opslaan van codes
const CodeSchema = new mongoose.Schema({
    code: { type: String, required: true, unique: true },
    createdAt: { type: Date, default: Date.now, expires: 86400 } // Verloopt na 24 uur
});

const Code = mongoose.model('Code', CodeSchema);

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

// Endpoint om een nieuwe code en QR code te genereren
app.all('/api/generate-qr', async (req, res) => {
    try {
        const uniqueCode = shortid.generate();
        const newCode = new Code({ code: uniqueCode });
        await newCode.save();

        // Maak een deeplink URL met de code
        const deeplinkUrl = `rijlesplanner://code/${uniqueCode}`; // Deeplink met de unieke code
        const fallbackUrl = `https://play.google.com/store/apps/details?id=com.Mascelli.RijlesPlanner&hl=en-US&ah=MbccWeflwmtbhkBBVOP3guaZc0A`;
        const redirectUrl = `${process.env.SERVER_URL}/redirect/${uniqueCode}`;

        // Genereer QR code
        const qrCode = await qr.toDataURL(redirectUrl);

        res.json({
            code: uniqueCode,
            qrCode: qrCode,
            deeplinkUrl: deeplinkUrl // Stuur de deeplink terug in de response
        });
    } catch (error) {
        console.error('QR Generation error:', error);
        res.status(500).json({ error: 'Er is iets misgegaan bij het genereren van de QR code' });
    }
});

// Redirect endpoint voor wanneer de QR code gescand wordt
app.get('/redirect/:code', async (req, res) => {
    try {
        const code = await Code.findOne({ code: req.params.code });
        if (!code) {
            return res.status(404).send('Code niet gevonden of verlopen');
        }

        // Detecteer het besturingssysteem
        const userAgent = req.headers['user-agent'].toLowerCase();
        const isIOS = /iphone|ipad|ipod/.test(userAgent);
        const isAndroid = /android/.test(userAgent);

        // Vervang deze URLs met je eigen app URLs
        const iosAppStoreUrl = 'https://apps.apple.com/your-app';
        const androidPlayStoreUrl = 'https://play.google.com/store/apps/details?id=com.Mascelli.RijlesPlanner&hl=en-US&ah=MbccWeflwmtbhkBBVOP3guaZc0A';
        const deeplinkUrl = `your-app-scheme://code/${req.params.code}`;

        // HTML voor de redirect pagina
        const html = `
            <!DOCTYPE html>
            <html>
            <head>
                <meta name="viewport" content="width=device-width, initial-scale=1">
                <title>Redirecting to app...</title>
                <script>
                    function redirect() {
                        // Probeer eerst de app te openen
                        window.location.href = "${deeplinkUrl}";
                        
                        // Als de app niet geïnstalleerd is, redirect naar de store na 1 seconde
                        setTimeout(function() {
                            window.location.href = "${isIOS ? iosAppStoreUrl : (isAndroid ? androidPlayStoreUrl : iosAppStoreUrl)}";
                        }, 1000);
                    }
                </script>
            </head>
            <body onload="redirect()">
                <h2>Je wordt doorgestuurd naar de app...</h2>
            </body>
            </html>
        `;

        res.send(html);
    } catch (error) {
        console.error('Redirect error:', error);
        res.status(500).send('Er is iets misgegaan');
    }
});

// Add a basic root route
app.get("/", (req, res) => {
    res.json({ message: "Rijschool API is running" });
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

// Add these logging middleware before your routes
app.use((req, res, next) => {
    console.log('\n=== Incoming Request ===');
    console.log('URL:', req.url);
    console.log('Method:', req.method);
    console.log('Headers:', JSON.stringify(req.headers, null, 2));
    console.log('Cookies:', JSON.stringify(req.cookies, null, 2));
    next();
});

// Start server
const PORT = process.env.PORT || 5000;
app.listen(PORT, () => {
    console.log(`🚀 Server running on port ${PORT}`);
    console.log(`Environment: ${process.env.NODE_ENV}`);
    console.log(`MongoDB URI set: ${process.env.MONGO_URI ? 'Yes' : 'No'}`);
});