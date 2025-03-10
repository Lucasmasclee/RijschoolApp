<?php
// Controleer of de cookie bestaat
if (isset($_COOKIE['plannerCode'])) {
    echo $_COOKIE['plannerCode']; // Stuur de waarde terug naar Unity
} else {
    echo "Geen code gevonden";
}
?>
