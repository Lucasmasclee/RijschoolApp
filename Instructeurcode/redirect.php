<?php
// Haal de 'code' parameter uit de URL (bijv. planner1, planner2, etc.)
if (isset($_GET['code'])) {
    $code = $_GET['code'];
    
    // Zet een cookie die 30 dagen geldig blijft
    setcookie("plannerCode", $code, time() + (30 * 24 * 60 * 60), "/", "", false, false);
    
    // Redirect naar de Google Play Store (vervang met je eigen Play Store link)
    header("Location: https://play.google.com/apps/testing/com.Mascelli.RijlesPlanner");
    exit();
} else {
    echo "Geen code gevonden.";
}
?>
