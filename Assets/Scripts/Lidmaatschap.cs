using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Purchasing;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Runtime.InteropServices;

public class Lidmaatschap : MonoBehaviour, IStoreListener
{
    private IStoreController storeController;
    private IExtensionProvider storeExtensionProvider;

    private string plannerCodeCookie = "";

    [Header("Subscription Products")]
    private string monthlySubID = "monthly_subscription";
    private string yearlySubID = "yearly_subscription";

    [Header("UI Elements")]
    [SerializeField] private GameObject subscriptionPopup;
    [SerializeField] private GameObject mainContent;
    [SerializeField] private TextMeshProUGUI monthlyPriceText;
    [SerializeField] private TextMeshProUGUI yearlyPriceText;
    [SerializeField] private Button monthlySubButton;
    [SerializeField] private Button yearlySubButton;
    [SerializeField] private Button restorePurchaseButton;
    [SerializeField] private GameObject loadingSpinner;
    [SerializeField] private GameObject verifyLeraar;
    [SerializeField] private TMP_InputField password;
    [SerializeField] private GameObject correctPassword;
    [SerializeField] private GameObject incorrectPassword;
    [SerializeField] private GameObject leraarButton;

    private DateTime trialEndDate;
    private bool isSubscribed = false;
    private List<string> validPasswords = new List<string>() { "planner2", "planner3", "planner4", "planner5", "planner6", "planner7", "planner8", "planner9", "planner10", "planner11", "planner12", "planner13", "planner14", "planner15", "planner16", "planner17", "planner18", "planner19", "planner20", }; // You should populate this with valid passwords






//    [DllImport("__Internal")]
//    private static extern string GetStoredCode();
//    private string rijschoolCode;
//    void Start()
//    {
//        ReadStoredCode();
//    }
//    private void ReadStoredCode()
//    {
//#if UNITY_WEBGL && !UNITY_EDITOR
//        // Voor WebGL builds
//        rijschoolCode = GetStoredCode();
//#elif UNITY_ANDROID
//        // Voor Android
//        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
//        using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
//        using (AndroidJavaObject context = currentActivity.Call<AndroidJavaObject>("getApplicationContext"))
//        {
//            AndroidJavaClass pluginClass = new AndroidJavaClass("com.yourcompany.plugin.StorageHelper");
//            rijschoolCode = pluginClass.CallStatic<string>("getStoredCode", context);
//        }
//#elif UNITY_IOS
//        // Voor iOS
//        rijschoolCode = _GetStoredCodeIOS();
//#endif

//        Debug.Log($"Retrieved code: {rijschoolCode}");
//        UnityAnalyticsManager.Instance.InstructeurcodeQRCode( rijschoolCode );
//        // Hier kun je de code gebruiken voor je app logica
//    }





    #region
    private void InitializePurchasing()
    {
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.AddProduct(monthlySubID, ProductType.Subscription);
        builder.AddProduct(yearlySubID, ProductType.Subscription);
        UnityPurchasing.Initialize(this, builder);
    }

    private void CheckSubscriptionStatus()
    {
        // Check if it's first time opening the app
        if (!PlayerPrefs.HasKey("TrialStartDate"))
        {
            // Set trial start date silently
            PlayerPrefs.SetString("TrialStartDate", DateTime.Now.ToString());
            PlayerPrefs.Save();
        }

        // Get trial end date
        DateTime trialStartDate = DateTime.Parse(PlayerPrefs.GetString("TrialStartDate"));
        trialEndDate = trialStartDate.AddDays(7);

        // Only show subscription popup if trial has expired and user is not subscribed
        if (DateTime.Now > trialEndDate && !isSubscribed)
        {
            ShowSubscriptionPopup();
        }
        else
        {
            mainContent.SetActive(true);
            subscriptionPopup.SetActive(false);
        }
    }

    private void ShowSubscriptionPopup()
    {
        mainContent.SetActive(false);
        subscriptionPopup.SetActive(true);
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        storeController = controller;
        storeExtensionProvider = extensions;

        // Update UI with actual prices
        UpdateProductPrices();
    }

    private void UpdateProductPrices()
    {
        if (storeController != null)
        {
            Product monthlyProduct = storeController.products.WithID(monthlySubID);
            Product yearlyProduct = storeController.products.WithID(yearlySubID);

            monthlyPriceText.text = monthlyProduct.metadata.localizedPriceString + "/maand";
            yearlyPriceText.text = yearlyProduct.metadata.localizedPriceString + "/jaar";
        }
    }

    public void PurchaseMonthlySubscription()
    {
        loadingSpinner.SetActive(true);
        storeController.InitiatePurchase(monthlySubID);
    }

    public void PurchaseYearlySubscription()
    {
        loadingSpinner.SetActive(true);
        storeController.InitiatePurchase(yearlySubID);
    }

    public void RestorePurchases()
    {
        loadingSpinner.SetActive(true);
        storeExtensionProvider.GetExtension<IAppleExtensions>().RestoreTransactions(OnRestoreTransactionsFinished);
    }

    private void OnRestoreTransactionsFinished(bool success)
    {
        loadingSpinner.SetActive(false);
        // Handle restore result
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        loadingSpinner.SetActive(false);

        if (args.purchasedProduct.definition.id == monthlySubID ||
            args.purchasedProduct.definition.id == yearlySubID)
        {
            isSubscribed = true;
            UnityAnalyticsManager.Instance.TrackSubscription(
                args.purchasedProduct.definition.id,
                args.purchasedProduct.metadata.localizedPrice
            );
            subscriptionPopup.SetActive(false);
            mainContent.SetActive(true);
        }

        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        loadingSpinner.SetActive(false);
        UnityAnalyticsManager.Instance.TrackSubscriptionFailure(
            product.definition.id,
            failureReason.ToString()
        );
        Debug.LogError($"Purchase failed: {product.definition.id}, reason: {failureReason}");
        // Show error message to user
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError($"Purchasing initialization failed: {error}");
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogError($"Purchasing initialization failed: {error}, message: {message}");
    }

    #endregion


    public void CheckPassword()
    {
        // Playerprefs set in UnityAnalyticsManager, where the password is sent to the unity analytics dashboard
        if (validPasswords.Contains(password.text.ToLower()))
        {
            correctPassword.SetActive(true);
            incorrectPassword.SetActive(false);
        }
        else
        {
            correctPassword.SetActive(false);
            incorrectPassword.SetActive(true);
        }
    }
}