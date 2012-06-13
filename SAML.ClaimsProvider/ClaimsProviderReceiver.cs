using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration.Claims;

namespace SAML.ClaimsProvider
{
    public class ClaimsProviderReceiver : SPClaimProviderFeatureReceiver
    {
        public override string ClaimProviderAssembly
        {
            get { return typeof(Provider).Assembly.FullName; }
        }

        public override string ClaimProviderType
        {
            get { return typeof(Provider).FullName; }
        }

        public override string ClaimProviderDisplayName
        {
            get { return Provider.ProviderDisplayName; }
        }

        public override string ClaimProviderDescription
        {
            get { return "Claims provider for truster login provider"; }
        }

        public override bool ClaimProviderUsedByDefault
        {
            get { return true; }
        }

        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            ExecBaseFeatureActivated(properties);
        }

        void ExecBaseFeatureActivated(SPFeatureReceiverProperties properties)
        {
            base.FeatureActivated(properties);
        }
    }
}