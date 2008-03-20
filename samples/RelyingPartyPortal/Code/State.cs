using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using DotNetOpenId.Extensions;

/// <summary>
/// Summary description for State
/// </summary>
public class State
{
    public State()
    {
    }

    public static SimpleRegistrationFieldValues ProfileFields
    {
        get
        {
            if (HttpContext.Current .Session["ProfileFields"] == null)
            {
                HttpContext.Current .Session["ProfileFields"] = new SimpleRegistrationFieldValues();
            }
            return (SimpleRegistrationFieldValues)HttpContext.Current .Session["ProfileFields"];
        }
        set { HttpContext.Current .Session["ProfileFields"] = value; }
    }
    
}
