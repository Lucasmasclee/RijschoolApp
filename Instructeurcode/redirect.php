<?php
// Haal de 'code' parameter uit de URL (bijv. planner1, planner2, etc.)
if (isset($_GET['code'])) {
    $code = $_GET['code'];
    
    // Set the cookie
    $cookieSet = setcookie("plannerCode", $code, time() + (30 * 24 * 60 * 60), "/", "yourdomain.com", false, false);
    
    if ($cookieSet) {
        echo "Cookie set successfully: plannerCode = $code";
    } else {
        echo "Failed to set cookie.";
    }
    
    // Redirect naar de Google Play Store (vervang met je eigen Play Store link)
    header("Location: https://play.google.com/apps/testing/com.Mascelli.RijlesPlanner");
    exit();
} else {
    echo "Geen code gevonden.";
}
?>
