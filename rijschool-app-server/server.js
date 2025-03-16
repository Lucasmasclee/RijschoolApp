const express = require("express");
const mongoose = require("mongoose");
const cors = require("cors");
require("dotenv").config();
const cookieParser = require('cookie-parser');
const QRCode = require('qrcode');


const app = express();
app.use(cookieParser());

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

// Add response logging middleware
app.use((req, res, next) => {
    // Store the original res.send
    const originalSend = res.send;
    
    // Override res.send to log the response
    res.send = function(body) {
        console.log('\n=== Outgoing Response ===');
        console.log('Status:', res.statusCode);
        console.log('Response Headers:', JSON.stringify(res.getHeaders(), null, 2));
        console.log('Response Body:', body);
        
        // Call the original res.send
        return originalSend.call(this, body);
    };
    
    next();
});

// Voeg deze route toe voor de QR code landing page
app.get('/qr/:code', (req, res) => {
    const code = req.params.code.trim();
    console.log('Received code:', code);

    const html = `
    <!DOCTYPE html>
    <html>
    <head>
        <title>Rijschool App Setup</title>
        <meta name="viewport" content="width=device-width, initial-scale=1">
    </head>
    <body>
        <div id="content" style="text-align: center; padding: 20px;">
            <h1>Welkom bij de Rijschool App</h1>
            <p>Even geduld, we stellen je apparaat in...</p>
            <p id="status"></p>
            <p id="debug"></p>
        </div>
        <script>
            const statusElement = document.getElementById('status');
            const debugElement = document.getElementById('debug');
            
            try {
                // Store the code directly without any URL parameters
                const code = "${code}";
                localStorage.setItem('rijschoolAppCode', code);
                
                // Debug output
                statusElement.textContent = 'Code succesvol opgeslagen!';
                
                // Redirect to custom scheme URL or Play Store with code parameter
                setTimeout(() => {
                    const userAgent = navigator.userAgent || navigator.vendor || window.opera;
                    if (/android/i.test(userAgent)) {
                        // Try to open app first with deep link - send raw code
                        window.location.href = 'rijschoolapp://code/' + code;
                        
                        // After a short delay, redirect to Play Store if app isn't installed
                        setTimeout(() => {
                            window.location.href = 'https://play.google.com/store/apps/details?id=com.Mascelli.RijlesPlanner';
                        }, 1000);
                    } else if (/iPad|iPhone|iPod/.test(userAgent)) {
                        window.location.href = 'rijschoolapp://code/' + code;
                        setTimeout(() => {
                            window.location.href = 'https://apps.apple.com/app/id[jouw_app_id]';
                        }, 1000);
                    } else {
                        statusElement.textContent = 'Download de app op je mobiele telefoon';
                    }
                }, 1000);
            } catch (error) {
                statusElement.textContent = 'Er ging iets mis: ' + error.message;
                debugElement.textContent = 'Error details: ' + error;
            }
        </script>
    </body>
    </html>
    `;
    
    res.send(html);
});

// Add a debug endpoint
app.get('/debug-storage', (req, res) => {
    const html = `
    <!DOCTYPE html>
    <html>
    <head>
        <title>Debug Storage</title>
    </head>
    <body>
        <h1>Storage Debug Info:</h1>
        <div id="debug"></div>
        <script>
            const debugDiv = document.getElementById('debug');
            try {
                const code = localStorage.getItem('rijschoolAppCode');
                const cookies = document.cookie;
                debugDiv.innerHTML = '<p>localStorage code: ' + code + '</p>' +
                    '<p>Cookies: ' + cookies + '</p>';
            } catch (error) {
                debugDiv.textContent = 'Error: ' + error;
            }
        </script>
    </body>
    </html>
    `;
    res.send(html);
});

// Route om QR code te genereren
app.get('/generate-qr/:code', async (req, res) => {
    try {
        const url = `https://rijschoolapp.onrender.com/qr/${req.params.code}`;
        const qrImage = await QRCode.toDataURL(url);
        
        const html = `
        <!DOCTYPE html>
        <html>
        <head>
            <title>QR Code</title>
        </head>
        <body style="text-align: center; padding: 20px;">
            <h1>Scan deze QR code</h1>
            <img src="${qrImage}" alt="QR Code">
        </body>
        </html>
        `;
        
        res.send(html);
    } catch (error) {
        res.status(500).send('Error generating QR code');
    }
});

// Start server
const PORT = process.env.PORT || 5000;
app.listen(PORT, () => {
    console.log(`🚀 Server running on port ${PORT}`);
    console.log(`Environment: ${process.env.NODE_ENV}`);
    console.log(`MongoDB URI set: ${process.env.MONGO_URI ? 'Yes' : 'No'}`);
});