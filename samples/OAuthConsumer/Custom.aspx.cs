using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using System.Xml.XPath;
using DotNetOpenAuth.ApplicationBlock;
using DotNetOpenAuth.OAuth;

public partial class Custom : System.Web.UI.Page
{
    private string AccessToken
    {
        get { return (string)Session["sampleAccessToken"]; }
        set { Session["sampleAccessToken"] = value; }
    }

    private InMemoryTokenManager TokenManager
    {
        get
        {
            var tokenManager = (InMemoryTokenManager)Application["sampleTokenManager"];
            if (tokenManager == null)
            {
                string consumerKey = ConfigurationManager.AppSettings["sampleConsumerKey"];
                string consumerSecret = ConfigurationManager.AppSettings["sampleConsumerSecret"];
                if (!string.IsNullOrEmpty(consumerKey))
                {
                    tokenManager = new InMemoryTokenManager(consumerKey, consumerSecret);
                    Application["sampleTokenManager"] = tokenManager;
                }
            }

            return tokenManager;
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (this.TokenManager != null)
        {
            MultiView1.ActiveViewIndex = 1;

            if (!IsPostBack)
            {
                var custom = new WebConsumer(CustomConsumer.ServiceDescription, this.TokenManager);

                // Is Twitter calling back with authorization?
                var accessTokenResponse = custom.ProcessUserAuthorization();
                if (accessTokenResponse != null)
                {
                    this.AccessToken = accessTokenResponse.AccessToken;
                }
                else if (this.AccessToken == null)
                {
                    

                    // If we don't yet have access, immediately request it.
                    custom.Channel.Send(custom.PrepareRequestUserAuthorization(HttpContext.Current.Request.Url, new Dictionary<string, string> {
                    { "scope", "GetName" }}, null));
                }
            }
        }
    }

    protected void getName_Click(object sender, EventArgs e)
    {
        var custom = new WebConsumer(CustomConsumer.ServiceDescription, this.TokenManager);
        XDocument doc = CustomConsumer.GetName(custom, this.AccessToken);
        resultsPlaceholder.Controls.Add(new Literal { Text = doc.ToString() });
    }

    protected void getAge_Click(object sender, EventArgs e)
    {
        var custom = new WebConsumer(CustomConsumer.ServiceDescription, this.TokenManager);
        XDocument doc = CustomConsumer.GetAge(custom, this.AccessToken);
        resultsPlaceholder.Controls.Add(new Literal { Text = doc.ToString() });
    }

    protected void getFavoriteSites_Click(object sender, EventArgs e)
    {
        var custom = new WebConsumer(CustomConsumer.ServiceDescription, this.TokenManager);
        XDocument doc = CustomConsumer.GetFavoriteSites(custom, this.AccessToken);
        resultsPlaceholder.Controls.Add(new Literal { Text = doc.ToString() });
    }

    protected void getNameAsJson_Click(object sender, EventArgs e)
    {
        var custom = new WebConsumer(CustomConsumer.ServiceDescription, this.TokenManager);
        resultsAsJsonPlaceholder.Value = CustomConsumer.GetNameAsJson(custom, this.AccessToken);
    }

    protected void getAgeAsJson_Click(object sender, EventArgs e)
    {
        var custom = new WebConsumer(CustomConsumer.ServiceDescription, this.TokenManager);
        resultsAsJsonPlaceholder.Value = CustomConsumer.GetAgeAsJson(custom, this.AccessToken);
    }

    protected void getFavoriteSitesAsJson_Click(object sender, EventArgs e)
    {
        var custom = new WebConsumer(CustomConsumer.ServiceDescription, this.TokenManager);
        resultsAsJsonPlaceholder.Value = CustomConsumer.GetFavoriteSitesAsJson(custom, this.AccessToken);
    }
}