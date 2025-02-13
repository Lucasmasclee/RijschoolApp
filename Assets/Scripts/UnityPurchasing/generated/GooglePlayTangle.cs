// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("FA6/RhFeAbdIQSA/pLQS+7VfESLiOvFFHQEAkbG+ry1A9ZAEGakz3pOem5aiKmck5hjnk2ApabkwCdthhNUIufR2ps6dcQVLedwvSSY6ABiBh6QE6rBE2S6rIRUn6nIZ4xZegIAysZKAvba5mjb4Nke9sbGxtbCzuJpEvX62xF8ZkWXkF6Ohv52Z30kIX4YcLGvIUF6+1evS+iRH3GG+WmQChVWqovRUKf5RqDHLPkAsaV48ZgkPUdFptGn1VfpuL5B7vuYxpwxiFg/9h3SXNiQHnkTgh+LkhC9bPjKxv7CAMrG6sjKxsbA+NjuviSsBETBu6Bmxs5LcmDHr6OYIuzWIezV2EFCgCWg8xa/odRK0C2FRLNtT+VYAc/8acE5S/7KzsbCx");
        private static int[] order = new int[] { 5,8,3,3,6,5,12,11,12,11,13,12,13,13,14 };
        private static int key = 176;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
