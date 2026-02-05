namespace ServiceImplementation.AdsServices.ConsentInformation
{
    using UniT.Logging;
    using UnityEngine.Scripting;

    public class DummyConsentInformation : IConsentInformation
    {
        #region Inject

        private readonly ILogger logger;

        #endregion

        [Preserve]
        public DummyConsentInformation(ILoggerManager loggerManager)
        {
            this.logger = loggerManager.GetLogger(this);
        }

        public bool CanRequestAds() => true;

        public void RequestConsent()
        {
            this.logger.Info("Request consent information");
        }

        public bool IsRequestingConsent() => false;
    }
}