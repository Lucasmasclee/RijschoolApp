// Instructeurcode/redirect.php
   <?php
   if (isset($_GET['code'])) {
       $code = $_GET['code'];
       header("Location: https://play.google.com/apps/testing/com.Mascelli.RijlesPlanner?referrer=" . urlencode($code));
       exit();
   } else {
       echo "Geen code gevonden.";
   }
   ?>