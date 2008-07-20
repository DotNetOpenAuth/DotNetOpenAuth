using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Data.SqlClient;

namespace ConsumerPortal {
	public partial class logs : System.Web.UI.Page {
		protected void Page_Load(object sender, EventArgs e) {
			SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["godaddy"].ConnectionString);
			conn.Open();
			try {
				var cmd = conn.CreateCommand();
				cmd.Parameters.Add(new SqlParameter("@log_date", "1/1/2008"));
				cmd.Parameters.Add(new SqlParameter("@thread", "3"));
				cmd.Parameters.Add(new SqlParameter("@log_level", "INFO"));
				cmd.Parameters.Add(new SqlParameter("@logger", "dnoi"));
				cmd.Parameters.Add(new SqlParameter("@message", "some message"));
				cmd.Parameters.Add(new SqlParameter("@exception", DBNull.Value));
				cmd.CommandText = "INSERT INTO Log ([Date],[Thread],[Level],[Logger],[Message],[Exception]) VALUES (@log_date, @thread, @log_level, @logger, @message, @exception)";
				cmd.CommandType = CommandType.Text;
				int result = cmd.ExecuteNonQuery();
				Response.Write("<P>result: " + result.ToString() + "</p>");
			} finally {
				conn.Close();
			}
		}
	}
}
