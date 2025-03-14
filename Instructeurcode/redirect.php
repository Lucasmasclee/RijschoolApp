<?php
// Haal de 'code' parameter uit de URL
if (isset($_GET['code'])) {
    $code = $_GET['code'];
    
    // Zet een cookie die 30 dagen geldig blijft
    setcookie("plannerCode", $code, time() + (30 * 24 * 60 * 60), "/");
    
    // Redirect naar de Google Play Store
    header("Location: https://play.google.com/apps/testing/com.Mascelli.RijlesPlanner");
    exit();
} else {
    echo "Geen code gevonden.";
}
?>