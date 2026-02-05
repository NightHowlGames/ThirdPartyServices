#if APPSFLYER
namespace ServiceImplementation.AppsflyerAnalyticTracker
{
    using System.Collections.Generic;
    using AFMiniJSON;
    using AppsFlyerSDK;
    using Core.AnalyticServices.Signal;
    using GameFoundation.Signals;
    using UnityEngine;
    using ILogger = UniT.Logging.ILogger;

    public class AppsflyerMono : MonoBehaviour, IAppsFlyerConversionData, IAppsFlyerPurchaseValidation, IAppsFlyerPurchaseRevenueDataSource, IAppsFlyerPurchaseRevenueDataSourceStoreKit2
    {
        private SignalBus signalBus;
        private ILogger   logger;

        public static AppsflyerMono Create(SignalBus signalBus, ILogger logger)
        {
            var iapGameObject = new GameObject();
            DontDestroyOnLoad(iapGameObject);
            iapGameObject.name = "AppsflyerMono";
            var appsflyerMono = iapGameObject.AddComponent<AppsflyerMono>();
            appsflyerMono.signalBus = signalBus;
            appsflyerMono.logger    = logger;
            return appsflyerMono;
        }

        #region Purchase Revenue Data Sources

        public Dictionary<string, object> PurchaseRevenueAdditionalParametersForProducts(
            HashSet<object> products,
            HashSet<object> transactions
        )
        {
            return new()
            {
                ["implementation_type"] = "separate_repository",
                ["additional_param"]    = "value",
                ["product_count"]       = products.Count,
                ["transaction_count"]   = transactions.Count
            };
        }

        public Dictionary<string, object> PurchaseRevenueAdditionalParametersStoreKit2ForProducts(
            HashSet<object> products,
            HashSet<object> transactions
        )
        {
            // Note: StoreKit 2 support depends on Purchase Connector version
            return new()
            {
                ["implementation_type"] = "separate_repository_sk2",
                ["additional_param"]    = "sk2_value",
                ["product_count"]       = products.Count,
                ["transaction_count"]   = transactions.Count
            };
        }

        #endregion

        #region Purchase Validation Callbacks

        public void didReceivePurchaseRevenueValidationInfo(string validationInfo)
        {
            AppsFlyer.AFLog("didReceivePurchaseRevenueValidationInfo", validationInfo);
            this.logger.Info("Purchase validation success: " + validationInfo);

            // Parse and handle validation info
            var dict = Json.Deserialize(validationInfo) as Dictionary<string, object>;

            #if UNITY_ANDROID
            if (dict.ContainsKey("productPurchase"))
            {
                this.logger.Info("Android in-app purchase validated");
            }
            else if (dict.ContainsKey("subscriptionPurchase"))
            {
                this.logger.Info("Android subscription validated");
            }
            #endif
        }

        public void didReceivePurchaseRevenueError(string error)
        {
            AppsFlyer.AFLog("didReceivePurchaseRevenueError", error);
            this.logger.Error("Purchase validation error: " + error);
        }

        #endregion

        #region Conversion Data Callbacks

        // Handle successful conversion data
        public void onConversionDataSuccess(string conversionData)
        {
            this.logger.Info("Conversion Data Success: " + conversionData);

            // Parse the JSON string into a dictionary
            var dataDictionary = AppsFlyer.CallbackStringToDictionary(conversionData);

            if (dataDictionary != null && dataDictionary.Count > 0)
            {
                this.logger.Info("Parsed Conversion Data:");
                // Log all key-value pairs
                foreach (var entry in dataDictionary)
                {
                    this.logger.Info($"{entry.Key}: {entry.Value}");
                }

                // Analyze key fields
                this.HandleAttributionData(dataDictionary);
            }
            else
            {
                this.logger.Warning("Conversion Data is null or empty.");
            }
            AppsFlyer.AFLog("onConversionDataSuccess", conversionData);
        }

        // Handle conversion data failure
        public void onConversionDataFail(string error)
        {
            AppsFlyer.AFLog("onConversionDataFail", error);
            this.logger.Error("Conversion Data Failure: " + error);
        }

        // Handle app open attribution success
        public void onAppOpenAttribution(string attributionData)
        {
            AppsFlyer.AFLog("onAppOpenAttribution", attributionData);
            this.logger.Info("App Open Attribution Data: " + attributionData);
        }

        // Handle app open attribution failure
        public void onAppOpenAttributionFailure(string error)
        {
            AppsFlyer.AFLog("onAppOpenAttributionFailure", error);
            this.logger.Error("App Open Attribution Failure: " + error);
        }

        // Analyze the conversion data dictionary
        private void HandleAttributionData(Dictionary<string, object> data) => this.signalBus.Fire(new AttributionChangedSignal(data));

        #endregion
    }
}
#endif