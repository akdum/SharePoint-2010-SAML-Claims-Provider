using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web.Script.Serialization;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Administration.Claims;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebControls;

namespace SAML.ClaimsProvider
{
    public class Provider : SPClaimProvider
    {
        #region Constants

        const string SamlSource = "SAMLUserSource";

        #endregion

        #region Constructor

        public Provider(string displayName)
            : base(displayName)
        {
        }

        #endregion

        #region Main properties

        public override string Name
        {
            get { return ProviderInternalName; }
        }

        // The AssociatedTrustedLoginProviderName property is used for 
        // generating the SPClaim object. 
        public string AssociatedTrustedLoginProviderName
        {
            get { return "KO SiteMinder"; }
        }

        public override bool SupportsEntityInformation
        {
            get { return false; }
        }

        public override bool SupportsHierarchy
        {
            get { return false; }
        }

        public override bool SupportsResolve
        {
            get { return true; }
        }

        public override bool SupportsSearch
        {
            get { return true; }
        }

        internal static string ProviderDisplayName
        {
            get { return "SAML Claims Provider"; }
        }


        internal static string ProviderInternalName
        {
            get { return "SAMLClaimsProvider"; }
        }

        #endregion

        #region Methods

        protected override void FillClaimTypes(List<string> claimTypes)
        {
            if (null == claimTypes)
            {
                throw new ArgumentNullException("claimTypes");
            }

            // Add the claim types that will be added by this claims provider.  
            claimTypes.Add(ClaimType.emailAddress);
            claimTypes.Add(ClaimType.UPN);
        }

        protected override void FillClaimValueTypes(List<string> claimValueTypes)
        {
            if (null == claimValueTypes)
            {
                throw new ArgumentNullException("claimValueTypes");
            }

            claimValueTypes.Add
                (Microsoft.IdentityModel.Claims.ClaimValueTypes.String);
            claimValueTypes.Add
                (Microsoft.IdentityModel.Claims.ClaimValueTypes.String);
        }

        protected override void FillSchema(SPProviderSchema schema)
        {
            schema.AddSchemaElement(new SPSchemaElement(PeopleEditorEntityDataKeys.DisplayName, "DisplayName",
                                                        SPSchemaElementType.TableViewOnly));
        }

        protected override void FillEntityTypes(List<string> entityTypes)
        {
            entityTypes.Add(SPClaimEntityTypes.User);
            entityTypes.Add(SPClaimEntityTypes.FormsRole);
        }

        protected override void FillClaimsForEntity(Uri context, SPClaim entity, List<SPClaim> claims)
        {
            throw new NotImplementedException();
        }

        protected override void FillHierarchy(
            Uri context, string[] entityTypes, string hierarchyNodeID, int numberOfLevels,
            SPProviderHierarchyTree hierarchy)
        {
            throw new NotImplementedException();
        }

        protected override void FillResolve(
            Uri context, string[] entityTypes, SPClaim resolveInput, List<PickerEntity> resolved)
        {
            var users = GetUsers(resolveInput.Value);
            resolved.AddRange(users.Select(user => GetPickerEntity(ClaimType.UPN, user.userName, user.email)));
        }

        protected override void FillResolve(
            Uri context, string[] entityTypes, string resolveInput, List<PickerEntity> resolved)
        {
            var users = GetUsers(resolveInput);
            resolved.AddRange(users.Select(user => GetPickerEntity(ClaimType.UPN, user.userName, user.email)));
        }

        protected override void FillSearch(
            Uri context, string[] entityTypes, string searchPattern, string hierarchyNodeID, int maxCount,
            SPProviderHierarchyTree searchTree)
        {
            var users = GetUsers(searchPattern);

            foreach (var user in users)
                searchTree.AddEntity(GetPickerEntity(ClaimType.UPN, user.userName, user.email));
        }

        #endregion

        #region Support Methods

        SPClaim CreateClaimForSTS(string claimtype, string claimValue)
        {
            var result = new SPClaim(claimtype, claimValue, Microsoft.IdentityModel.Claims.ClaimValueTypes.String,
                                     SPOriginalIssuers.Format(SPOriginalIssuerType.TrustedProvider,
                                                              AssociatedTrustedLoginProviderName));

            return result;
        }

        IEnumerable<User> GetUsers(string searchName)
        {
            var userSourceUrl = GetUserSourceUri();
            if (!String.IsNullOrEmpty(userSourceUrl))
            {
                var nameValues = new NameValueCollection();
                nameValues["userName"] = SPEncode.UrlEncode(searchName);
                using (var webClient = new WebClient())
                {
                    var responseFromServer = Encoding.ASCII.GetString(webClient.UploadValues(userSourceUrl, nameValues));

                    var jsonSerializer = new JavaScriptSerializer();
                    var desObj = jsonSerializer.Deserialize<RootObject>(responseFromServer);
                    if (desObj != null && desObj.users != null) return desObj.users;
                }
            }
            return new List<User>();
        }

        PickerEntity GetPickerEntity(string claimType, string claimValue, string email)
        {
            var entity = CreatePickerEntity();
            entity.Claim = CreateClaimForSTS(claimType, claimValue);
            entity.Description = claimValue;
            entity.DisplayText = claimValue;
            entity.EntityData[PeopleEditorEntityDataKeys.DisplayName] = claimValue;
            entity.EntityData[PeopleEditorEntityDataKeys.Email] = email;

            entity.EntityType = String.Compare(claimType, ClaimType.emailAddress, StringComparison.OrdinalIgnoreCase) ==
                                0
                                    ? SPClaimEntityTypes.User
                                    : SPClaimEntityTypes.FormsRole;

            entity.IsResolved = true;

            return entity;
        }

        string GetUserSourceUri()
        {
            var farm = SPFarm.Local;
            if (farm != null && farm.Properties.ContainsKey(SamlSource))
            {
                var sourceVal = farm.Properties[SamlSource];
                return sourceVal != null ? sourceVal.ToString() : null;
            }
            return null;
        }

        #endregion
    }

    public class User
    {
        public string _id { get; set; }
        public string email { get; set; }
        public string lastLogin { get; set; }
        public string userName { get; set; }
    }

    public class RootObject
    {
        public List<User> users { get; set; }
    }
}