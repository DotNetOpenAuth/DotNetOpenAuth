<%@ Control Language="C#" AutoEventWireup="true" Inherits="OpenIdProviderWebForms.ProfileFields" CodeBehind="ProfileFields.ascx.cs" %>
This consumer has requested the following fields from you<br />
<table>
	<tr>
		<td>
			&nbsp;
		</td>
		<td>
			<asp:HyperLink ID="privacyLink" runat="server" Text="Privacy Policy"
			Target="_blank" />
		</td>
	</tr>
	<tr runat="server" id="nicknameRow">
		<td>
			Nickname
			<asp:Label ID="nicknameRequiredLabel" runat="server" Text="*" Visible="False"></asp:Label>
		</td>
		<td>
			<asp:TextBox ID="nicknameTextBox" runat="server"></asp:TextBox>
		</td>
	</tr>
	<tr runat="server" id="emailRow">
		<td>
			Email
			<asp:Label ID="emailRequiredLabel" runat="server" Text="*" Visible="False"></asp:Label>
		</td>
		<td>
			<asp:TextBox ID="emailTextBox" runat="server"></asp:TextBox>
		</td>
	</tr>
	<tr runat="server" id="fullnameRow">
		<td>
			FullName
			<asp:Label ID="fullnameRequiredLabel" runat="server" Text="*" Visible="False"></asp:Label>
		</td>
		<td>
			<asp:TextBox ID="fullnameTextBox" runat="server"></asp:TextBox>
		</td>
	</tr>
	<tr runat="server" id="dateOfBirthRow">
		<td>
			Date of Birth
			<asp:Label ID="dobRequiredLabel" runat="server" Text="*" Visible="False"></asp:Label>
		</td>
		<td>
			<asp:DropDownList ID="dobDayDropdownlist" runat="server">
				<asp:ListItem></asp:ListItem>
				<asp:ListItem>1</asp:ListItem>
				<asp:ListItem>2</asp:ListItem>
				<asp:ListItem>3</asp:ListItem>
				<asp:ListItem>4</asp:ListItem>
				<asp:ListItem>5</asp:ListItem>
				<asp:ListItem>6</asp:ListItem>
				<asp:ListItem>7</asp:ListItem>
				<asp:ListItem>8</asp:ListItem>
				<asp:ListItem>9</asp:ListItem>
				<asp:ListItem>10</asp:ListItem>
				<asp:ListItem>11</asp:ListItem>
				<asp:ListItem>12</asp:ListItem>
				<asp:ListItem>13</asp:ListItem>
				<asp:ListItem>14</asp:ListItem>
				<asp:ListItem>15</asp:ListItem>
				<asp:ListItem>16</asp:ListItem>
				<asp:ListItem>17</asp:ListItem>
				<asp:ListItem>18</asp:ListItem>
				<asp:ListItem>19</asp:ListItem>
				<asp:ListItem>20</asp:ListItem>
				<asp:ListItem>21</asp:ListItem>
				<asp:ListItem>22</asp:ListItem>
				<asp:ListItem>23</asp:ListItem>
				<asp:ListItem>24</asp:ListItem>
				<asp:ListItem>25</asp:ListItem>
				<asp:ListItem>26</asp:ListItem>
				<asp:ListItem>27</asp:ListItem>
				<asp:ListItem>28</asp:ListItem>
				<asp:ListItem>29</asp:ListItem>
				<asp:ListItem>30</asp:ListItem>
				<asp:ListItem>31</asp:ListItem>
			</asp:DropDownList>
			&nbsp;<asp:DropDownList ID="dobMonthDropdownlist" runat="server">
				<asp:ListItem></asp:ListItem>
				<asp:ListItem Value="1">January</asp:ListItem>
				<asp:ListItem Value="2">February</asp:ListItem>
				<asp:ListItem Value="3">March</asp:ListItem>
				<asp:ListItem Value="4">April</asp:ListItem>
				<asp:ListItem Value="5">May</asp:ListItem>
				<asp:ListItem Value="6">June</asp:ListItem>
				<asp:ListItem Value="7">July</asp:ListItem>
				<asp:ListItem Value="8">August</asp:ListItem>
				<asp:ListItem Value="9">September</asp:ListItem>
				<asp:ListItem Value="10">October</asp:ListItem>
				<asp:ListItem Value="11">November</asp:ListItem>
				<asp:ListItem Value="12">December</asp:ListItem>
			</asp:DropDownList>
			&nbsp;
			<asp:DropDownList ID="dobYearDropdownlist" runat="server">
				<asp:ListItem></asp:ListItem>
				<asp:ListItem>2009</asp:ListItem>
				<asp:ListItem>2008</asp:ListItem>
				<asp:ListItem>2007</asp:ListItem>
				<asp:ListItem>2006</asp:ListItem>
				<asp:ListItem>2005</asp:ListItem>
				<asp:ListItem>2004</asp:ListItem>
				<asp:ListItem>2003</asp:ListItem>
				<asp:ListItem>2002</asp:ListItem>
				<asp:ListItem>2001</asp:ListItem>
				<asp:ListItem>2000</asp:ListItem>
				<asp:ListItem>1999</asp:ListItem>
				<asp:ListItem>1998</asp:ListItem>
				<asp:ListItem>1997</asp:ListItem>
				<asp:ListItem>1996</asp:ListItem>
				<asp:ListItem>1995</asp:ListItem>
				<asp:ListItem>1994</asp:ListItem>
				<asp:ListItem>1993</asp:ListItem>
				<asp:ListItem>1992</asp:ListItem>
				<asp:ListItem>1991</asp:ListItem>
				<asp:ListItem>1990</asp:ListItem>
				<asp:ListItem>1989</asp:ListItem>
				<asp:ListItem>1988</asp:ListItem>
				<asp:ListItem>1987</asp:ListItem>
				<asp:ListItem>1986</asp:ListItem>
				<asp:ListItem>1985</asp:ListItem>
				<asp:ListItem>1984</asp:ListItem>
				<asp:ListItem>1983</asp:ListItem>
				<asp:ListItem>1982</asp:ListItem>
				<asp:ListItem>1981</asp:ListItem>
				<asp:ListItem>1980</asp:ListItem>
				<asp:ListItem>1979</asp:ListItem>
				<asp:ListItem>1978</asp:ListItem>
				<asp:ListItem>1977</asp:ListItem>
				<asp:ListItem>1976</asp:ListItem>
				<asp:ListItem>1975</asp:ListItem>
				<asp:ListItem>1974</asp:ListItem>
				<asp:ListItem>1973</asp:ListItem>
				<asp:ListItem>1972</asp:ListItem>
				<asp:ListItem>1971</asp:ListItem>
				<asp:ListItem>1970</asp:ListItem>
				<asp:ListItem>1969</asp:ListItem>
				<asp:ListItem>1968</asp:ListItem>
				<asp:ListItem>1967</asp:ListItem>
				<asp:ListItem>1966</asp:ListItem>
				<asp:ListItem>1965</asp:ListItem>
				<asp:ListItem>1964</asp:ListItem>
				<asp:ListItem>1963</asp:ListItem>
				<asp:ListItem>1962</asp:ListItem>
				<asp:ListItem>1961</asp:ListItem>
				<asp:ListItem>1960</asp:ListItem>
				<asp:ListItem>1959</asp:ListItem>
				<asp:ListItem>1958</asp:ListItem>
				<asp:ListItem>1957</asp:ListItem>
				<asp:ListItem>1956</asp:ListItem>
				<asp:ListItem>1955</asp:ListItem>
				<asp:ListItem>1954</asp:ListItem>
				<asp:ListItem>1953</asp:ListItem>
				<asp:ListItem>1952</asp:ListItem>
				<asp:ListItem>1951</asp:ListItem>
				<asp:ListItem>1950</asp:ListItem>
				<asp:ListItem>1949</asp:ListItem>
				<asp:ListItem>1948</asp:ListItem>
				<asp:ListItem>1947</asp:ListItem>
				<asp:ListItem>1946</asp:ListItem>
				<asp:ListItem>1945</asp:ListItem>
				<asp:ListItem>1944</asp:ListItem>
				<asp:ListItem>1943</asp:ListItem>
				<asp:ListItem>1942</asp:ListItem>
				<asp:ListItem>1941</asp:ListItem>
				<asp:ListItem>1940</asp:ListItem>
				<asp:ListItem>1939</asp:ListItem>
				<asp:ListItem>1938</asp:ListItem>
				<asp:ListItem>1937</asp:ListItem>
				<asp:ListItem>1936</asp:ListItem>
				<asp:ListItem>1935</asp:ListItem>
				<asp:ListItem>1934</asp:ListItem>
				<asp:ListItem>1933</asp:ListItem>
				<asp:ListItem>1932</asp:ListItem>
				<asp:ListItem>1931</asp:ListItem>
				<asp:ListItem>1930</asp:ListItem>
				<asp:ListItem>1929</asp:ListItem>
				<asp:ListItem>1928</asp:ListItem>
				<asp:ListItem>1927</asp:ListItem>
				<asp:ListItem>1926</asp:ListItem>
				<asp:ListItem>1925</asp:ListItem>
				<asp:ListItem>1924</asp:ListItem>
				<asp:ListItem>1923</asp:ListItem>
				<asp:ListItem>1922</asp:ListItem>
				<asp:ListItem>1921</asp:ListItem>
				<asp:ListItem>1920</asp:ListItem>
			</asp:DropDownList>
		</td>
	</tr>
	<tr runat="server" id="genderRow">
		<td>
			Gender
			<asp:Label ID="genderRequiredLabel" runat="server" Text="*" Visible="False"></asp:Label>
		</td>
		<td>
			<asp:DropDownList ID="genderDropdownList" runat="server">
				<asp:ListItem Selected="True"></asp:ListItem>
				<asp:ListItem>Male</asp:ListItem>
				<asp:ListItem>Female</asp:ListItem>
			</asp:DropDownList>
		</td>
	</tr>
	<tr runat="server" id="postcodeRow">
		<td>
			Post Code
			<asp:Label ID="postcodeRequiredLabel" runat="server" Text="*" Visible="False"></asp:Label>
		</td>
		<td>
			<asp:TextBox ID="postcodeTextBox" runat="server"></asp:TextBox>
		</td>
	</tr>
	<tr runat="server" id="countryRow">
		<td>
			Country
			<asp:Label ID="countryRequiredLabel" runat="server" Text="*" Visible="False"></asp:Label>
		</td>
		<td>
			<asp:DropDownList ID="countryDropdownList" runat="server">
				<asp:ListItem Value=""> </asp:ListItem>
				<asp:ListItem Value="AF">AFGHANISTAN </asp:ListItem>
				<asp:ListItem Value="AX">ÅLAND ISLANDS</asp:ListItem>
				<asp:ListItem Value="AL">ALBANIA</asp:ListItem>
				<asp:ListItem Value="DZ">ALGERIA</asp:ListItem>
				<asp:ListItem Value="AS">AMERICAN SAMOA</asp:ListItem>
				<asp:ListItem Value="AD">ANDORRA</asp:ListItem>
				<asp:ListItem Value="AO">ANGOLA</asp:ListItem>
				<asp:ListItem Value="AI">ANGUILLA</asp:ListItem>
				<asp:ListItem Value="AQ">ANTARCTICA</asp:ListItem>
				<asp:ListItem Value="AG">ANTIGUA AND BARBUDA</asp:ListItem>
				<asp:ListItem Value="AR">ARGENTINA</asp:ListItem>
				<asp:ListItem Value="AM">ARMENIA</asp:ListItem>
				<asp:ListItem Value="AW">ARUBA</asp:ListItem>
				<asp:ListItem Value="AU">AUSTRALIA</asp:ListItem>
				<asp:ListItem Value="AT">AUSTRIA</asp:ListItem>
				<asp:ListItem Value="AZ">AZERBAIJAN</asp:ListItem>
				<asp:ListItem Value="BS">BAHAMAS</asp:ListItem>
				<asp:ListItem Value="BH">BAHRAIN</asp:ListItem>
				<asp:ListItem Value="BD">BANGLADESH</asp:ListItem>
				<asp:ListItem Value="BB">BARBADOS</asp:ListItem>
				<asp:ListItem Value="BY">BELARUS</asp:ListItem>
				<asp:ListItem Value="BE">BELGIUM</asp:ListItem>
				<asp:ListItem Value="BZ">BELIZE</asp:ListItem>
				<asp:ListItem Value="BJ">BENIN</asp:ListItem>
				<asp:ListItem Value="BM">BERMUDA</asp:ListItem>
				<asp:ListItem Value="BT">BHUTAN</asp:ListItem>
				<asp:ListItem Value="BO">BOLIVIA</asp:ListItem>
				<asp:ListItem Value="BA">BOSNIA AND HERZEGOVINA</asp:ListItem>
				<asp:ListItem Value="BW">BOTSWANA</asp:ListItem>
				<asp:ListItem Value="BV">BOUVET ISLAND</asp:ListItem>
				<asp:ListItem Value="BR">BRAZIL</asp:ListItem>
				<asp:ListItem Value="IO">BRITISH INDIAN OCEAN TERRITORY</asp:ListItem>
				<asp:ListItem Value="BN">BRUNEI DARUSSALAM</asp:ListItem>
				<asp:ListItem Value="BG">BULGARIA</asp:ListItem>
				<asp:ListItem Value="BF">BURKINA FASO</asp:ListItem>
				<asp:ListItem Value="BI">BURUNDI</asp:ListItem>
				<asp:ListItem Value="KH">CAMBODIA</asp:ListItem>
				<asp:ListItem Value="CM">CAMEROON</asp:ListItem>
				<asp:ListItem Value="CA">CANADA</asp:ListItem>
				<asp:ListItem Value="CV">CAPE VERDE</asp:ListItem>
				<asp:ListItem Value="KY">CAYMAN ISLANDS</asp:ListItem>
				<asp:ListItem Value="CF">CENTRAL AFRICAN REPUBLIC</asp:ListItem>
				<asp:ListItem Value="TD">CHAD</asp:ListItem>
				<asp:ListItem Value="CL">CHILE</asp:ListItem>
				<asp:ListItem Value="CN">CHINA</asp:ListItem>
				<asp:ListItem Value="CX">CHRISTMAS ISLAND</asp:ListItem>
				<asp:ListItem Value="CC">COCOS (KEELING) ISLANDS</asp:ListItem>
				<asp:ListItem Value="CO">COLOMBIA</asp:ListItem>
				<asp:ListItem Value="KM">COMOROS</asp:ListItem>
				<asp:ListItem Value="CG">CONGO</asp:ListItem>
				<asp:ListItem Value="CD">CONGO, THE DEMOCRATIC REPUBLIC OF THE</asp:ListItem>
				<asp:ListItem Value="CK">COOK ISLANDS</asp:ListItem>
				<asp:ListItem Value="CR">COSTA RICA</asp:ListItem>
				<asp:ListItem Value="CI">CÔTE D'IVOIRE</asp:ListItem>
				<asp:ListItem Value="HR">CROATIA</asp:ListItem>
				<asp:ListItem Value="CU">CUBA</asp:ListItem>
				<asp:ListItem Value="CY">CYPRUS</asp:ListItem>
				<asp:ListItem Value="CZ">CZECH REPUBLIC</asp:ListItem>
				<asp:ListItem Value="DK">DENMARK</asp:ListItem>
				<asp:ListItem Value="DJ">DJIBOUTI</asp:ListItem>
				<asp:ListItem Value="DM">DOMINICA</asp:ListItem>
				<asp:ListItem Value="DO">DOMINICAN REPUBLIC</asp:ListItem>
				<asp:ListItem Value="EC">ECUADOR</asp:ListItem>
				<asp:ListItem Value="EG">EGYPT</asp:ListItem>
				<asp:ListItem Value="SV">EL SALVADOR</asp:ListItem>
				<asp:ListItem Value="GQ">EQUATORIAL GUINEA</asp:ListItem>
				<asp:ListItem Value="ER">ERITREA</asp:ListItem>
				<asp:ListItem Value="EE">ESTONIA</asp:ListItem>
				<asp:ListItem Value="ET">ETHIOPIA</asp:ListItem>
				<asp:ListItem Value="FK">FALKLAND ISLANDS (MALVINAS)</asp:ListItem>
				<asp:ListItem Value="FO">FAROE ISLANDS</asp:ListItem>
				<asp:ListItem Value="FJ">FIJI</asp:ListItem>
				<asp:ListItem Value="FI">FINLAND</asp:ListItem>
				<asp:ListItem Value="FR">FRANCE</asp:ListItem>
				<asp:ListItem Value="GF">FRENCH GUIANA</asp:ListItem>
				<asp:ListItem Value="PF">FRENCH POLYNESIA</asp:ListItem>
				<asp:ListItem Value="TF">FRENCH SOUTHERN TERRITORIES</asp:ListItem>
				<asp:ListItem Value="GA">GABON </asp:ListItem>
				<asp:ListItem Value="GM">GAMBIA</asp:ListItem>
				<asp:ListItem Value="GE">GEORGIA</asp:ListItem>
				<asp:ListItem Value="DE">GERMANY</asp:ListItem>
				<asp:ListItem Value="GH">GHANA</asp:ListItem>
				<asp:ListItem Value="GI">GIBRALTAR</asp:ListItem>
				<asp:ListItem Value="GR">GREECE</asp:ListItem>
				<asp:ListItem Value="GL">GREENLAND</asp:ListItem>
				<asp:ListItem Value="GD">GRENADA</asp:ListItem>
				<asp:ListItem Value="GP">GUADELOUPE</asp:ListItem>
				<asp:ListItem Value="GU">GUAM </asp:ListItem>
				<asp:ListItem Value="GT">GUATEMALA</asp:ListItem>
				<asp:ListItem Value="GG">GUERNSEY</asp:ListItem>
				<asp:ListItem Value="GN">GUINEA</asp:ListItem>
				<asp:ListItem Value="GW">GUINEA-BISSAU</asp:ListItem>
				<asp:ListItem Value="GY">GUYANA</asp:ListItem>
				<asp:ListItem Value="HT">HAITI</asp:ListItem>
				<asp:ListItem Value="HM">HEARD ISLAND AND MCDONALD ISLANDS</asp:ListItem>
				<asp:ListItem Value="VA">HOLY SEE (VATICAN CITY STATE)</asp:ListItem>
				<asp:ListItem Value="HN">HONDURAS</asp:ListItem>
				<asp:ListItem Value="HK">HONG KONG</asp:ListItem>
				<asp:ListItem Value="HU">HUNGARY</asp:ListItem>
				<asp:ListItem Value="IS">ICELAND</asp:ListItem>
				<asp:ListItem Value="IN">INDIA</asp:ListItem>
				<asp:ListItem Value="ID">INDONESIA</asp:ListItem>
				<asp:ListItem Value="IR">IRAN, ISLAMIC REPUBLIC OF</asp:ListItem>
				<asp:ListItem Value="IQ">IRAQ</asp:ListItem>
				<asp:ListItem Value="IE">IRELAND</asp:ListItem>
				<asp:ListItem Value="IM">ISLE OF MAN</asp:ListItem>
				<asp:ListItem Value="IL">ISRAEL</asp:ListItem>
				<asp:ListItem Value="IT">ITALY</asp:ListItem>
				<asp:ListItem Value="JM">JAMAICA</asp:ListItem>
				<asp:ListItem Value="JP">JAPAN</asp:ListItem>
				<asp:ListItem Value="JE">JERSEY</asp:ListItem>
				<asp:ListItem Value="JO">JORDAN</asp:ListItem>
				<asp:ListItem Value="KZ">KAZAKHSTAN</asp:ListItem>
				<asp:ListItem Value="KE">KENYA</asp:ListItem>
				<asp:ListItem Value="KI">KIRIBATI</asp:ListItem>
				<asp:ListItem Value="KP">KOREA, DEMOCRATIC PEOPLE'S REPUBLIC OF</asp:ListItem>
				<asp:ListItem Value="KR">KOREA, REPUBLIC OF</asp:ListItem>
				<asp:ListItem Value="KW">KUWAIT</asp:ListItem>
				<asp:ListItem Value="KG">KYRGYZSTAN</asp:ListItem>
				<asp:ListItem Value="LA">LAO PEOPLE'S DEMOCRATIC REPUBLIC </asp:ListItem>
				<asp:ListItem Value="LV">LATVIA</asp:ListItem>
				<asp:ListItem Value="LB">LEBANON</asp:ListItem>
				<asp:ListItem Value="LS">LESOTHO</asp:ListItem>
				<asp:ListItem Value="LR">LIBERIA</asp:ListItem>
				<asp:ListItem Value="LY">LIBYAN ARAB JAMAHIRIYA</asp:ListItem>
				<asp:ListItem Value="LI">LIECHTENSTEIN</asp:ListItem>
				<asp:ListItem Value="LT">LITHUANIA</asp:ListItem>
				<asp:ListItem Value="LU">LUXEMBOURG</asp:ListItem>
				<asp:ListItem Value="MO">MACAO</asp:ListItem>
				<asp:ListItem Value="MK">MACEDONIA, THE FORMER YUGOSLAV REPUBLIC OF</asp:ListItem>
				<asp:ListItem Value="MG">MADAGASCAR</asp:ListItem>
				<asp:ListItem Value="MW">MALAWI</asp:ListItem>
				<asp:ListItem Value="MY">MALAYSIA</asp:ListItem>
				<asp:ListItem Value="MV">MALDIVES</asp:ListItem>
				<asp:ListItem Value="ML">MALI</asp:ListItem>
				<asp:ListItem Value="MT">MALTA</asp:ListItem>
				<asp:ListItem Value="MH">MARSHALL ISLANDS</asp:ListItem>
				<asp:ListItem Value="MQ">MARTINIQUE</asp:ListItem>
				<asp:ListItem Value="MR">MAURITANIA</asp:ListItem>
				<asp:ListItem Value="MU">MAURITIUS</asp:ListItem>
				<asp:ListItem Value="YT">MAYOTTE</asp:ListItem>
				<asp:ListItem Value="MX">MEXICO</asp:ListItem>
				<asp:ListItem Value="FM">MICRONESIA, FEDERATED STATES OF</asp:ListItem>
				<asp:ListItem Value="MD">MOLDOVA, REPUBLIC OF</asp:ListItem>
				<asp:ListItem Value="MC">MONACO</asp:ListItem>
				<asp:ListItem Value="MN">MONGOLIA</asp:ListItem>
				<asp:ListItem Value="ME">MONTENEGRO</asp:ListItem>
				<asp:ListItem Value="MS">MONTSERRAT</asp:ListItem>
				<asp:ListItem Value="MA">MOROCCO</asp:ListItem>
				<asp:ListItem Value="MZ">MOZAMBIQUE</asp:ListItem>
				<asp:ListItem Value="MM">MYANMAR</asp:ListItem>
				<asp:ListItem Value="NA">NAMIBIA</asp:ListItem>
				<asp:ListItem Value="NR">NAURU</asp:ListItem>
				<asp:ListItem Value="NP">NEPAL</asp:ListItem>
				<asp:ListItem Value="NL">NETHERLANDS</asp:ListItem>
				<asp:ListItem Value="AN">NETHERLANDS ANTILLES</asp:ListItem>
				<asp:ListItem Value="NC">NEW CALEDONIA</asp:ListItem>
				<asp:ListItem Value="NZ">NEW ZEALAND</asp:ListItem>
				<asp:ListItem Value="NI">NICARAGUA</asp:ListItem>
				<asp:ListItem Value="NE">NIGER</asp:ListItem>
				<asp:ListItem Value="NG">NIGERIA</asp:ListItem>
				<asp:ListItem Value="NU">NIUE</asp:ListItem>
				<asp:ListItem Value="NF">NORFOLK ISLAND</asp:ListItem>
				<asp:ListItem Value="MP">NORTHERN MARIANA ISLANDS</asp:ListItem>
				<asp:ListItem Value="NO">NORWAY</asp:ListItem>
				<asp:ListItem Value="OM">OMAN</asp:ListItem>
				<asp:ListItem Value="PK">PAKISTAN</asp:ListItem>
				<asp:ListItem Value="PW">PALAU</asp:ListItem>
				<asp:ListItem Value="PS">PALESTINIAN TERRITORY, OCCUPIED</asp:ListItem>
				<asp:ListItem Value="PA">PANAMA</asp:ListItem>
				<asp:ListItem Value="PG">PAPUA NEW GUINEA</asp:ListItem>
				<asp:ListItem Value="PY">PARAGUAY</asp:ListItem>
				<asp:ListItem Value="PE">PERU</asp:ListItem>
				<asp:ListItem Value="PH">PHILIPPINES</asp:ListItem>
				<asp:ListItem Value="PN">PITCAIRN</asp:ListItem>
				<asp:ListItem Value="PL">POLAND</asp:ListItem>
				<asp:ListItem Value="PT">PORTUGAL</asp:ListItem>
				<asp:ListItem Value="PR">PUERTO RICO</asp:ListItem>
				<asp:ListItem Value="QA">QATAR</asp:ListItem>
				<asp:ListItem Value="RE">RÉUNION</asp:ListItem>
				<asp:ListItem Value="RO">ROMANIA</asp:ListItem>
				<asp:ListItem Value="RU">RUSSIAN FEDERATION</asp:ListItem>
				<asp:ListItem Value="RW">RWANDA</asp:ListItem>
				<asp:ListItem Value="SH">SAINT HELENA </asp:ListItem>
				<asp:ListItem Value="KN">SAINT KITTS AND NEVIS</asp:ListItem>
				<asp:ListItem Value="LC">SAINT LUCIA</asp:ListItem>
				<asp:ListItem Value="PM">SAINT PIERRE AND MIQUELON</asp:ListItem>
				<asp:ListItem Value="VC">SAINT VINCENT AND THE GRENADINES</asp:ListItem>
				<asp:ListItem Value="WS">SAMOA</asp:ListItem>
				<asp:ListItem Value="SM">SAN MARINO</asp:ListItem>
				<asp:ListItem Value="ST">SAO TOME AND PRINCIPE</asp:ListItem>
				<asp:ListItem Value="SA">SAUDI ARABIA</asp:ListItem>
				<asp:ListItem Value="SN">SENEGAL</asp:ListItem>
				<asp:ListItem Value="RS">SERBIA</asp:ListItem>
				<asp:ListItem Value="SC">SEYCHELLES</asp:ListItem>
				<asp:ListItem Value="SL">SIERRA LEONE</asp:ListItem>
				<asp:ListItem Value="SG">SINGAPORE</asp:ListItem>
				<asp:ListItem Value="SK">SLOVAKIA</asp:ListItem>
				<asp:ListItem Value="SI">SLOVENIA</asp:ListItem>
				<asp:ListItem Value="SB">SOLOMON ISLANDS</asp:ListItem>
				<asp:ListItem Value="SO">SOMALIA</asp:ListItem>
				<asp:ListItem Value="ZA">SOUTH AFRICA</asp:ListItem>
				<asp:ListItem Value="GS">SOUTH GEORGIA AND THE SOUTH SANDWICH ISLANDS</asp:ListItem>
				<asp:ListItem Value="ES">SPAIN</asp:ListItem>
				<asp:ListItem Value="LK">SRI LANKA</asp:ListItem>
				<asp:ListItem Value="SD">SUDAN</asp:ListItem>
				<asp:ListItem Value="SR">SURINAME</asp:ListItem>
				<asp:ListItem Value="SJ">SVALBARD AND JAN MAYEN</asp:ListItem>
				<asp:ListItem Value="SZ">SWAZILAND</asp:ListItem>
				<asp:ListItem Value="SE">SWEDEN</asp:ListItem>
				<asp:ListItem Value="CH">SWITZERLAND</asp:ListItem>
				<asp:ListItem Value="SY">SYRIAN ARAB REPUBLIC</asp:ListItem>
				<asp:ListItem Value="TW">TAIWAN, PROVINCE OF CHINA</asp:ListItem>
				<asp:ListItem Value="TJ">TAJIKISTAN</asp:ListItem>
				<asp:ListItem Value="TZ">TANZANIA, UNITED REPUBLIC OF</asp:ListItem>
				<asp:ListItem Value="TH">THAILAND</asp:ListItem>
				<asp:ListItem Value="TL">TIMOR-LESTE</asp:ListItem>
				<asp:ListItem Value="TG">TOGO</asp:ListItem>
				<asp:ListItem Value="TK">TOKELAU</asp:ListItem>
				<asp:ListItem Value="TO">TONGA</asp:ListItem>
				<asp:ListItem Value="TT">TRINIDAD AND TOBAGO</asp:ListItem>
				<asp:ListItem Value="TN">TUNISIA</asp:ListItem>
				<asp:ListItem Value="TR">TURKEY</asp:ListItem>
				<asp:ListItem Value="TM">TURKMENISTAN</asp:ListItem>
				<asp:ListItem Value="TC">TURKS AND CAICOS ISLANDS</asp:ListItem>
				<asp:ListItem Value="TV">TUVALU</asp:ListItem>
				<asp:ListItem Value="UG">UGANDA</asp:ListItem>
				<asp:ListItem Value="UA">UKRAINE</asp:ListItem>
				<asp:ListItem Value="AE">UNITED ARAB EMIRATES</asp:ListItem>
				<asp:ListItem Value="GB">UNITED KINGDOM</asp:ListItem>
				<asp:ListItem Value="US">UNITED STATES</asp:ListItem>
				<asp:ListItem Value="UM">UNITED STATES MINOR OUTLYING ISLANDS</asp:ListItem>
				<asp:ListItem Value="UY">URUGUAY</asp:ListItem>
				<asp:ListItem Value="UZ">UZBEKISTAN</asp:ListItem>
				<asp:ListItem Value="VU">VANUATU</asp:ListItem>
				<asp:ListItem Value="VE">VENEZUELA</asp:ListItem>
				<asp:ListItem Value="VN">VIET NAM</asp:ListItem>
				<asp:ListItem Value="VG">VIRGIN ISLANDS, BRITISH</asp:ListItem>
				<asp:ListItem Value="VI">VIRGIN ISLANDS, U.S.</asp:ListItem>
				<asp:ListItem Value="WF">WALLIS AND FUTUNA</asp:ListItem>
				<asp:ListItem Value="EH">WESTERN SAHARA</asp:ListItem>
				<asp:ListItem Value="YE">YEMEN</asp:ListItem>
				<asp:ListItem Value="ZM">ZAMBIA</asp:ListItem>
				<asp:ListItem Value="ZW">ZIMBABWE</asp:ListItem>
			</asp:DropDownList>
		</td>
	</tr>
	<tr runat="server" id="languageRow">
		<td>
			Language
			<asp:Label ID="languageRequiredLabel" runat="server" Text="*" Visible="False"></asp:Label>
		</td>
		<td>
			<asp:DropDownList ID="languageDropdownList" runat="server">
				<asp:ListItem Value=""></asp:ListItem>
				<asp:ListItem Value="EN">English</asp:ListItem>
				<asp:ListItem Value="AB">Abkhazian</asp:ListItem>
				<asp:ListItem Value="AA">Afar</asp:ListItem>
				<asp:ListItem Value="AF">Afrikaans</asp:ListItem>
				<asp:ListItem Value="SQ">Albanian</asp:ListItem>
				<asp:ListItem Value="AM">Amharic</asp:ListItem>
				<asp:ListItem Value="AR">Arabic</asp:ListItem>
				<asp:ListItem Value="HY">Armenian</asp:ListItem>
				<asp:ListItem Value="AS">Assamese</asp:ListItem>
				<asp:ListItem Value="AY">Aymara</asp:ListItem>
				<asp:ListItem Value="AZ">Azerbaijani</asp:ListItem>
				<asp:ListItem Value="BA">Bashkir</asp:ListItem>
				<asp:ListItem Value="EU">Basque</asp:ListItem>
				<asp:ListItem Value="BN">Bengali</asp:ListItem>
				<asp:ListItem Value="DZ">Bhutani</asp:ListItem>
				<asp:ListItem Value="BH">Bihari</asp:ListItem>
				<asp:ListItem Value="BI">Bislama</asp:ListItem>
				<asp:ListItem Value="BR">Breton</asp:ListItem>
				<asp:ListItem Value="BG">Bulgarian</asp:ListItem>
				<asp:ListItem Value="MY">Burmese</asp:ListItem>
				<asp:ListItem Value="BE">Byelorussian</asp:ListItem>
				<asp:ListItem Value="KM">Cambodian</asp:ListItem>
				<asp:ListItem Value="CA">Catalan</asp:ListItem>
				<asp:ListItem Value="ZH">Chinese</asp:ListItem>
				<asp:ListItem Value="CO">Corsican</asp:ListItem>
				<asp:ListItem Value="HR">Croatian</asp:ListItem>
				<asp:ListItem Value="CS">Czech</asp:ListItem>
				<asp:ListItem Value="DA">Danish</asp:ListItem>
				<asp:ListItem Value="NL">Dutch</asp:ListItem>
				<asp:ListItem Value="EO">Esperanto</asp:ListItem>
				<asp:ListItem Value="ET">Estonian</asp:ListItem>
				<asp:ListItem Value="FO">Faeroese</asp:ListItem>
				<asp:ListItem Value="FJ">Fiji</asp:ListItem>
				<asp:ListItem Value="FI">Finnish</asp:ListItem>
				<asp:ListItem Value="FR">French</asp:ListItem>
				<asp:ListItem Value="FY">Frisian</asp:ListItem>
				<asp:ListItem Value="GD">Gaelic</asp:ListItem>
				<asp:ListItem Value="GL">Galician</asp:ListItem>
				<asp:ListItem Value="KA">Georgian</asp:ListItem>
				<asp:ListItem Value="DE">German</asp:ListItem>
				<asp:ListItem Value="EL">Greek</asp:ListItem>
				<asp:ListItem Value="KL">Greenlandic</asp:ListItem>
				<asp:ListItem Value="GN">Guarani</asp:ListItem>
				<asp:ListItem Value="GU">Gujarati</asp:ListItem>
				<asp:ListItem Value="HA">Hausa</asp:ListItem>
				<asp:ListItem Value="IW">Hebrew</asp:ListItem>
				<asp:ListItem Value="HI">Hindi</asp:ListItem>
				<asp:ListItem Value="HU">Hungarian</asp:ListItem>
				<asp:ListItem Value="IS">Icelandic</asp:ListItem>
				<asp:ListItem Value="IN">Indonesian</asp:ListItem>
				<asp:ListItem Value="IA">Interlingua</asp:ListItem>
				<asp:ListItem Value="IE">Interlingue</asp:ListItem>
				<asp:ListItem Value="IK">Inupiak</asp:ListItem>
				<asp:ListItem Value="GA">Irish</asp:ListItem>
				<asp:ListItem Value="IT">Italian</asp:ListItem>
				<asp:ListItem Value="JA">Japanese</asp:ListItem>
				<asp:ListItem Value="JW">Javanese</asp:ListItem>
				<asp:ListItem Value="KN">Kannada</asp:ListItem>
				<asp:ListItem Value="KS">Kashmiri</asp:ListItem>
				<asp:ListItem Value="KK">Kazakh</asp:ListItem>
				<asp:ListItem Value="RW">Kinyarwanda</asp:ListItem>
				<asp:ListItem Value="KY">Kirghiz</asp:ListItem>
				<asp:ListItem Value="RN">Kirundi</asp:ListItem>
				<asp:ListItem Value="KO">Korean</asp:ListItem>
				<asp:ListItem Value="KU">Kurdish</asp:ListItem>
				<asp:ListItem Value="LO">Laothian</asp:ListItem>
				<asp:ListItem Value="LA">Latin</asp:ListItem>
				<asp:ListItem Value="LV">Latvian</asp:ListItem>
				<asp:ListItem Value="LN">Lingala</asp:ListItem>
				<asp:ListItem Value="LT">Lithuanian</asp:ListItem>
				<asp:ListItem Value="MK">Macedonian</asp:ListItem>
				<asp:ListItem Value="MG">Malagasy</asp:ListItem>
				<asp:ListItem Value="MS">Malay</asp:ListItem>
				<asp:ListItem Value="ML">Malayalam</asp:ListItem>
				<asp:ListItem Value="MT">Maltese</asp:ListItem>
				<asp:ListItem Value="MI">Maori</asp:ListItem>
				<asp:ListItem Value="MR">Marathi</asp:ListItem>
				<asp:ListItem Value="MO">Moldavian</asp:ListItem>
				<asp:ListItem Value="MN">Mongolian</asp:ListItem>
				<asp:ListItem Value="NA">Nauru</asp:ListItem>
				<asp:ListItem Value="NE">Nepali</asp:ListItem>
				<asp:ListItem Value="NO">Norwegian</asp:ListItem>
				<asp:ListItem Value="OC">Occitan</asp:ListItem>
				<asp:ListItem Value="OR">Oriya</asp:ListItem>
				<asp:ListItem Value="OM">Oromo</asp:ListItem>
				<asp:ListItem Value="PS">Pashto</asp:ListItem>
				<asp:ListItem Value="FA">Persian</asp:ListItem>
				<asp:ListItem Value="PL">Polish</asp:ListItem>
				<asp:ListItem Value="PT">Portuguese</asp:ListItem>
				<asp:ListItem Value="PA">Punjabi</asp:ListItem>
				<asp:ListItem Value="QU">Quechua</asp:ListItem>
				<asp:ListItem Value="RM">Rhaeto-Romance</asp:ListItem>
				<asp:ListItem Value="RO">Romanian</asp:ListItem>
				<asp:ListItem Value="RU">Russian</asp:ListItem>
				<asp:ListItem Value="SM">Samoan</asp:ListItem>
				<asp:ListItem Value="SG">Sangro</asp:ListItem>
				<asp:ListItem Value="SA">Sanskrit</asp:ListItem>
				<asp:ListItem Value="SR">Serbian</asp:ListItem>
				<asp:ListItem Value="SH">Serbo-Croatian</asp:ListItem>
				<asp:ListItem Value="ST">Sesotho</asp:ListItem>
				<asp:ListItem Value="TN">Setswana</asp:ListItem>
				<asp:ListItem Value="SN">Shona</asp:ListItem>
				<asp:ListItem Value="SD">Sindhi</asp:ListItem>
				<asp:ListItem Value="SI">Singhalese</asp:ListItem>
				<asp:ListItem Value="SS">Siswati</asp:ListItem>
				<asp:ListItem Value="SK">Slovak</asp:ListItem>
				<asp:ListItem Value="SL">Slovenian</asp:ListItem>
				<asp:ListItem Value="SO">Somali</asp:ListItem>
				<asp:ListItem Value="ES">Spanish</asp:ListItem>
				<asp:ListItem Value="SU">Sudanese</asp:ListItem>
				<asp:ListItem Value="SW">Swahili</asp:ListItem>
				<asp:ListItem Value="SV">Swedish</asp:ListItem>
				<asp:ListItem Value="TL">Tagalog</asp:ListItem>
				<asp:ListItem Value="TG">Tajik</asp:ListItem>
				<asp:ListItem Value="TA">Tamil</asp:ListItem>
				<asp:ListItem Value="TT">Tatar</asp:ListItem>
				<asp:ListItem Value="TE">Telugu</asp:ListItem>
				<asp:ListItem Value="TH">Thai</asp:ListItem>
				<asp:ListItem Value="BO">Tibetan</asp:ListItem>
				<asp:ListItem Value="TI">Tigrinya</asp:ListItem>
				<asp:ListItem Value="TO">Tonga</asp:ListItem>
				<asp:ListItem Value="TS">Tsonga</asp:ListItem>
				<asp:ListItem Value="TR">Turkish</asp:ListItem>
				<asp:ListItem Value="TK">Turkmen</asp:ListItem>
				<asp:ListItem Value="TW">Twi</asp:ListItem>
				<asp:ListItem Value="UK">Ukrainian</asp:ListItem>
				<asp:ListItem Value="UR">Urdu</asp:ListItem>
				<asp:ListItem Value="UZ">Uzbek</asp:ListItem>
				<asp:ListItem Value="VI">Vietnamese</asp:ListItem>
				<asp:ListItem Value="VO">Volapuk</asp:ListItem>
				<asp:ListItem Value="CY">Welsh</asp:ListItem>
				<asp:ListItem Value="WO">Wolof</asp:ListItem>
				<asp:ListItem Value="XH">Xhosa</asp:ListItem>
				<asp:ListItem Value="JI">Yiddish</asp:ListItem>
				<asp:ListItem Value="YO">Yoruba</asp:ListItem>
				<asp:ListItem Value="ZU">Zulu</asp:ListItem>
			</asp:DropDownList>
		</td>
	</tr>
	<tr runat="server" id="timezoneRow">
		<td>
			Timezone
			<asp:Label ID="timezoneRequiredLabel" runat="server" Text="*" Visible="False"></asp:Label>
		</td>
		<td>
			<asp:DropDownList runat="server" ID="timezoneDropdownList">
				<asp:ListItem Value=""></asp:ListItem>
				<asp:ListItem Value="Europe/London">Europe/London</asp:ListItem>
				<asp:ListItem Value="Africa/Abidjan">Africa/Abidjan</asp:ListItem>
				<asp:ListItem Value="Africa/Accra">Africa/Accra</asp:ListItem>
				<asp:ListItem Value="Africa/Addis_Ababa">Africa/Addis_Ababa</asp:ListItem>
				<asp:ListItem Value="Africa/Algiers">Africa/Algiers</asp:ListItem>
				<asp:ListItem Value="Africa/Asmera">Africa/Asmera</asp:ListItem>
				<asp:ListItem Value="Africa/Bamako">Africa/Bamako</asp:ListItem>
				<asp:ListItem Value="Africa/Bangui">Africa/Bangui</asp:ListItem>
				<asp:ListItem Value="Africa/Banjul">Africa/Banjul</asp:ListItem>
				<asp:ListItem Value="Africa/Bissau">Africa/Bissau</asp:ListItem>
				<asp:ListItem Value="Africa/Blantyre">Africa/Blantyre</asp:ListItem>
				<asp:ListItem Value="Africa/Brazzaville">Africa/Brazzaville</asp:ListItem>
				<asp:ListItem Value="Africa/Bujumbura">Africa/Bujumbura</asp:ListItem>
				<asp:ListItem Value="Africa/Cairo">Africa/Cairo</asp:ListItem>
				<asp:ListItem Value="Africa/Casablanca">Africa/Casablanca</asp:ListItem>
				<asp:ListItem Value="Africa/Ceuta">Africa/Ceuta</asp:ListItem>
				<asp:ListItem Value="Africa/Conakry">Africa/Conakry</asp:ListItem>
				<asp:ListItem Value="Africa/Dakar">Africa/Dakar</asp:ListItem>
				<asp:ListItem Value="Africa/Dar_es_Salaam">Africa/Dar_es_Salaam</asp:ListItem>
				<asp:ListItem Value="Africa/Djibouti">Africa/Djibouti</asp:ListItem>
				<asp:ListItem Value="Africa/Douala">Africa/Douala</asp:ListItem>
				<asp:ListItem Value="Africa/El_Aaiun">Africa/El_Aaiun</asp:ListItem>
				<asp:ListItem Value="Africa/Freetown">Africa/Freetown</asp:ListItem>
				<asp:ListItem Value="Africa/Gaborone">Africa/Gaborone</asp:ListItem>
				<asp:ListItem Value="Africa/Harare">Africa/Harare</asp:ListItem>
				<asp:ListItem Value="Africa/Johannesburg">Africa/Johannesburg</asp:ListItem>
				<asp:ListItem Value="Africa/Kampala">Africa/Kampala</asp:ListItem>
				<asp:ListItem Value="Africa/Khartoum">Africa/Khartoum</asp:ListItem>
				<asp:ListItem Value="Africa/Kigali">Africa/Kigali</asp:ListItem>
				<asp:ListItem Value="Africa/Kinshasa">Africa/Kinshasa</asp:ListItem>
				<asp:ListItem Value="Africa/Lagos">Africa/Lagos</asp:ListItem>
				<asp:ListItem Value="Africa/Libreville">Africa/Libreville</asp:ListItem>
				<asp:ListItem Value="Africa/Lome">Africa/Lome</asp:ListItem>
				<asp:ListItem Value="Africa/Luanda">Africa/Luanda</asp:ListItem>
				<asp:ListItem Value="Africa/Lubumbashi">Africa/Lubumbashi</asp:ListItem>
				<asp:ListItem Value="Africa/Lusaka">Africa/Lusaka</asp:ListItem>
				<asp:ListItem Value="Africa/Malabo">Africa/Malabo</asp:ListItem>
				<asp:ListItem Value="Africa/Maputo">Africa/Maputo</asp:ListItem>
				<asp:ListItem Value="Africa/Maseru">Africa/Maseru</asp:ListItem>
				<asp:ListItem Value="Africa/Mbabane">Africa/Mbabane</asp:ListItem>
				<asp:ListItem Value="Africa/Mogadishu">Africa/Mogadishu</asp:ListItem>
				<asp:ListItem Value="Africa/Monrovia">Africa/Monrovia</asp:ListItem>
				<asp:ListItem Value="Africa/Nairobi">Africa/Nairobi</asp:ListItem>
				<asp:ListItem Value="Africa/Ndjamena">Africa/Ndjamena</asp:ListItem>
				<asp:ListItem Value="Africa/Niamey">Africa/Niamey</asp:ListItem>
				<asp:ListItem Value="Africa/Nouakchott">Africa/Nouakchott</asp:ListItem>
				<asp:ListItem Value="Africa/Ouagadougou">Africa/Ouagadougou</asp:ListItem>
				<asp:ListItem Value="Africa/Porto">Africa/Porto</asp:ListItem>
				<asp:ListItem Value="Africa/Sao_Tome">Africa/Sao_Tome</asp:ListItem>
				<asp:ListItem Value="Africa/Tripoli">Africa/Tripoli</asp:ListItem>
				<asp:ListItem Value="Africa/Tunis">Africa/Tunis</asp:ListItem>
				<asp:ListItem Value="Africa/Windhoek">Africa/Windhoek</asp:ListItem>
				<asp:ListItem Value="America/Adak">America/Adak</asp:ListItem>
				<asp:ListItem Value="America/Anchorage">America/Anchorage</asp:ListItem>
				<asp:ListItem Value="America/Anguilla">America/Anguilla</asp:ListItem>
				<asp:ListItem Value="America/Antigua">America/Antigua</asp:ListItem>
				<asp:ListItem Value="America/Araguaina">America/Araguaina</asp:ListItem>
				<asp:ListItem Value="America/Argentina/Buenos_Aires">America/Argentina/Buenos_Aires</asp:ListItem>
				<asp:ListItem Value="America/Argentina/Catamarca">America/Argentina/Catamarca</asp:ListItem>
				<asp:ListItem Value="America/Argentina/Cordoba">America/Argentina/Cordoba</asp:ListItem>
				<asp:ListItem Value="America/Argentina/Jujuy">America/Argentina/Jujuy</asp:ListItem>
				<asp:ListItem Value="America/Argentina/La_Rioja">America/Argentina/La_Rioja</asp:ListItem>
				<asp:ListItem Value="America/Argentina/Mendoza">America/Argentina/Mendoza</asp:ListItem>
				<asp:ListItem Value="America/Argentina/Rio_Gallegos">America/Argentina/Rio_Gallegos</asp:ListItem>
				<asp:ListItem Value="America/Argentina/San_Juan">America/Argentina/San_Juan</asp:ListItem>
				<asp:ListItem Value="America/Argentina/Tucuman">America/Argentina/Tucuman</asp:ListItem>
				<asp:ListItem Value="America/Argentina/Ushuaia">America/Argentina/Ushuaia</asp:ListItem>
				<asp:ListItem Value="America/Aruba">America/Aruba</asp:ListItem>
				<asp:ListItem Value="America/Asuncion">America/Asuncion</asp:ListItem>
				<asp:ListItem Value="America/Bahia">America/Bahia</asp:ListItem>
				<asp:ListItem Value="America/Barbados">America/Barbados</asp:ListItem>
				<asp:ListItem Value="America/Belem">America/Belem</asp:ListItem>
				<asp:ListItem Value="America/Belize">America/Belize</asp:ListItem>
				<asp:ListItem Value="America/Boa_Vista">America/Boa_Vista</asp:ListItem>
				<asp:ListItem Value="America/Bogota">America/Bogota</asp:ListItem>
				<asp:ListItem Value="America/Boise">America/Boise</asp:ListItem>
				<asp:ListItem Value="America/Cambridge_Bay">America/Cambridge_Bay</asp:ListItem>
				<asp:ListItem Value="America/Campo_Grande">America/Campo_Grande</asp:ListItem>
				<asp:ListItem Value="America/Cancun">America/Cancun</asp:ListItem>
				<asp:ListItem Value="America/Caracas">America/Caracas</asp:ListItem>
				<asp:ListItem Value="America/Cayenne">America/Cayenne</asp:ListItem>
				<asp:ListItem Value="America/Cayman">America/Cayman</asp:ListItem>
				<asp:ListItem Value="America/Chicago">America/Chicago</asp:ListItem>
				<asp:ListItem Value="America/Chihuahua">America/Chihuahua</asp:ListItem>
				<asp:ListItem Value="America/Coral_Harbour">America/Coral_Harbour</asp:ListItem>
				<asp:ListItem Value="America/Costa_Rica">America/Costa_Rica</asp:ListItem>
				<asp:ListItem Value="America/Cuiaba">America/Cuiaba</asp:ListItem>
				<asp:ListItem Value="America/Curacao">America/Curacao</asp:ListItem>
				<asp:ListItem Value="America/Danmarkshavn">America/Danmarkshavn</asp:ListItem>
				<asp:ListItem Value="America/Dawson">America/Dawson</asp:ListItem>
				<asp:ListItem Value="America/Dawson_Creek">America/Dawson_Creek</asp:ListItem>
				<asp:ListItem Value="America/Denver">America/Denver</asp:ListItem>
				<asp:ListItem Value="America/Detroit">America/Detroit</asp:ListItem>
				<asp:ListItem Value="America/Dominica">America/Dominica</asp:ListItem>
				<asp:ListItem Value="America/Edmonton">America/Edmonton</asp:ListItem>
				<asp:ListItem Value="America/Eirunepe">America/Eirunepe</asp:ListItem>
				<asp:ListItem Value="America/El_Salvador">America/El_Salvador</asp:ListItem>
				<asp:ListItem Value="America/Fortaleza">America/Fortaleza</asp:ListItem>
				<asp:ListItem Value="America/Glace_Bay">America/Glace_Bay</asp:ListItem>
				<asp:ListItem Value="America/Godthab">America/Godthab</asp:ListItem>
				<asp:ListItem Value="America/Goose_Bay">America/Goose_Bay</asp:ListItem>
				<asp:ListItem Value="America/Grand_Turk">America/Grand_Turk</asp:ListItem>
				<asp:ListItem Value="America/Grenada">America/Grenada</asp:ListItem>
				<asp:ListItem Value="America/Guadeloupe">America/Guadeloupe</asp:ListItem>
				<asp:ListItem Value="America/Guatemala">America/Guatemala</asp:ListItem>
				<asp:ListItem Value="America/Guayaquil">America/Guayaquil</asp:ListItem>
				<asp:ListItem Value="America/Guyana">America/Guyana</asp:ListItem>
				<asp:ListItem Value="America/Halifax">America/Halifax</asp:ListItem>
				<asp:ListItem Value="America/Havana">America/Havana</asp:ListItem>
				<asp:ListItem Value="America/Hermosillo">America/Hermosillo</asp:ListItem>
				<asp:ListItem Value="America/Indiana/Indianapolis">America/Indiana/Indianapolis</asp:ListItem>
				<asp:ListItem Value="America/Indiana/Knox">America/Indiana/Knox</asp:ListItem>
				<asp:ListItem Value="America/Indiana/Marengo">America/Indiana/Marengo</asp:ListItem>
				<asp:ListItem Value="America/Indiana/Petersburg">America/Indiana/Petersburg</asp:ListItem>
				<asp:ListItem Value="America/Indiana/Vevay">America/Indiana/Vevay</asp:ListItem>
				<asp:ListItem Value="America/Indiana/Vincennes">America/Indiana/Vincennes</asp:ListItem>
				<asp:ListItem Value="America/Inuvik">America/Inuvik</asp:ListItem>
				<asp:ListItem Value="America/Iqaluit">America/Iqaluit</asp:ListItem>
				<asp:ListItem Value="America/Jamaica">America/Jamaica</asp:ListItem>
				<asp:ListItem Value="America/Juneau">America/Juneau</asp:ListItem>
				<asp:ListItem Value="America/Kentucky/Louisville">America/Kentucky/Louisville</asp:ListItem>
				<asp:ListItem Value="America/Kentucky/Monticello">America/Kentucky/Monticello</asp:ListItem>
				<asp:ListItem Value="America/La_Paz">America/La_Paz</asp:ListItem>
				<asp:ListItem Value="America/Lima">America/Lima</asp:ListItem>
				<asp:ListItem Value="America/Los_Angeles">America/Los_Angeles</asp:ListItem>
				<asp:ListItem Value="America/Maceio">America/Maceio</asp:ListItem>
				<asp:ListItem Value="America/Managua">America/Managua</asp:ListItem>
				<asp:ListItem Value="America/Manaus">America/Manaus</asp:ListItem>
				<asp:ListItem Value="America/Martinique">America/Martinique</asp:ListItem>
				<asp:ListItem Value="America/Mazatlan">America/Mazatlan</asp:ListItem>
				<asp:ListItem Value="America/Menominee">America/Menominee</asp:ListItem>
				<asp:ListItem Value="America/Merida">America/Merida</asp:ListItem>
				<asp:ListItem Value="America/Mexico_City">America/Mexico_City</asp:ListItem>
				<asp:ListItem Value="America/Miquelon">America/Miquelon</asp:ListItem>
				<asp:ListItem Value="America/Moncton">America/Moncton</asp:ListItem>
				<asp:ListItem Value="America/Monterrey">America/Monterrey</asp:ListItem>
				<asp:ListItem Value="America/Montevideo">America/Montevideo</asp:ListItem>
				<asp:ListItem Value="America/Montreal">America/Montreal</asp:ListItem>
				<asp:ListItem Value="America/Montserrat">America/Montserrat</asp:ListItem>
				<asp:ListItem Value="America/Nassau">America/Nassau</asp:ListItem>
				<asp:ListItem Value="America/New_York">America/New_York</asp:ListItem>
				<asp:ListItem Value="America/Nipigon">America/Nipigon</asp:ListItem>
				<asp:ListItem Value="America/Nome">America/Nome</asp:ListItem>
				<asp:ListItem Value="America/Noronha">America/Noronha</asp:ListItem>
				<asp:ListItem Value="America/North_Dakota/Center">America/North_Dakota/Center</asp:ListItem>
				<asp:ListItem Value="America/Panama">America/Panama</asp:ListItem>
				<asp:ListItem Value="America/Pangnirtung">America/Pangnirtung</asp:ListItem>
				<asp:ListItem Value="America/Paramaribo">America/Paramaribo</asp:ListItem>
				<asp:ListItem Value="America/Phoenix">America/Phoenix</asp:ListItem>
				<asp:ListItem Value="America/Port">America/Port</asp:ListItem>
				<asp:ListItem Value="America/Port_of_Spain">America/Port_of_Spain</asp:ListItem>
				<asp:ListItem Value="America/Porto_Velho">America/Porto_Velho</asp:ListItem>
				<asp:ListItem Value="America/Puerto_Rico">America/Puerto_Rico</asp:ListItem>
				<asp:ListItem Value="America/Rainy_River">America/Rainy_River</asp:ListItem>
				<asp:ListItem Value="America/Rankin_Inlet">America/Rankin_Inlet</asp:ListItem>
				<asp:ListItem Value="America/Recife">America/Recife</asp:ListItem>
				<asp:ListItem Value="America/Regina">America/Regina</asp:ListItem>
				<asp:ListItem Value="America/Rio_Branco">America/Rio_Branco</asp:ListItem>
				<asp:ListItem Value="America/Santiago">America/Santiago</asp:ListItem>
				<asp:ListItem Value="America/Santo_Domingo">America/Santo_Domingo</asp:ListItem>
				<asp:ListItem Value="America/Sao_Paulo">America/Sao_Paulo</asp:ListItem>
				<asp:ListItem Value="America/Scoresbysund">America/Scoresbysund</asp:ListItem>
				<asp:ListItem Value="America/Shiprock">America/Shiprock</asp:ListItem>
				<asp:ListItem Value="America/St_Johns">America/St_Johns</asp:ListItem>
				<asp:ListItem Value="America/St_Kitts">America/St_Kitts</asp:ListItem>
				<asp:ListItem Value="America/St_Lucia">America/St_Lucia</asp:ListItem>
				<asp:ListItem Value="America/St_Thomas">America/St_Thomas</asp:ListItem>
				<asp:ListItem Value="America/St_Vincent">America/St_Vincent</asp:ListItem>
				<asp:ListItem Value="America/Swift_Current">America/Swift_Current</asp:ListItem>
				<asp:ListItem Value="America/Tegucigalpa">America/Tegucigalpa</asp:ListItem>
				<asp:ListItem Value="America/Thule">America/Thule</asp:ListItem>
				<asp:ListItem Value="America/Thunder_Bay">America/Thunder_Bay</asp:ListItem>
				<asp:ListItem Value="America/Tijuana">America/Tijuana</asp:ListItem>
				<asp:ListItem Value="America/Toronto">America/Toronto</asp:ListItem>
				<asp:ListItem Value="America/Tortola">America/Tortola</asp:ListItem>
				<asp:ListItem Value="America/Vancouver">America/Vancouver</asp:ListItem>
				<asp:ListItem Value="America/Whitehorse">America/Whitehorse</asp:ListItem>
				<asp:ListItem Value="America/Winnipeg">America/Winnipeg</asp:ListItem>
				<asp:ListItem Value="America/Yakutat">America/Yakutat</asp:ListItem>
				<asp:ListItem Value="America/Yellowknife">America/Yellowknife</asp:ListItem>
				<asp:ListItem Value="Antarctica/Casey">Antarctica/Casey</asp:ListItem>
				<asp:ListItem Value="Antarctica/Davis">Antarctica/Davis</asp:ListItem>
				<asp:ListItem Value="Antarctica/DumontDUrville">Antarctica/DumontDUrville</asp:ListItem>
				<asp:ListItem Value="Antarctica/Mawson">Antarctica/Mawson</asp:ListItem>
				<asp:ListItem Value="Antarctica/McMurdo">Antarctica/McMurdo</asp:ListItem>
				<asp:ListItem Value="Antarctica/Palmer">Antarctica/Palmer</asp:ListItem>
				<asp:ListItem Value="Antarctica/Rothera">Antarctica/Rothera</asp:ListItem>
				<asp:ListItem Value="Antarctica/South_Pole">Antarctica/South_Pole</asp:ListItem>
				<asp:ListItem Value="Antarctica/Syowa">Antarctica/Syowa</asp:ListItem>
				<asp:ListItem Value="Antarctica/Vostok">Antarctica/Vostok</asp:ListItem>
				<asp:ListItem Value="Arctic/Longyearbyen">Arctic/Longyearbyen</asp:ListItem>
				<asp:ListItem Value="Asia/Aden">Asia/Aden</asp:ListItem>
				<asp:ListItem Value="Asia/Almaty">Asia/Almaty</asp:ListItem>
				<asp:ListItem Value="Asia/Amman">Asia/Amman</asp:ListItem>
				<asp:ListItem Value="Asia/Anadyr">Asia/Anadyr</asp:ListItem>
				<asp:ListItem Value="Asia/Aqtau">Asia/Aqtau</asp:ListItem>
				<asp:ListItem Value="Asia/Aqtobe">Asia/Aqtobe</asp:ListItem>
				<asp:ListItem Value="Asia/Ashgabat">Asia/Ashgabat</asp:ListItem>
				<asp:ListItem Value="Asia/Baghdad">Asia/Baghdad</asp:ListItem>
				<asp:ListItem Value="Asia/Bahrain">Asia/Bahrain</asp:ListItem>
				<asp:ListItem Value="Asia/Baku">Asia/Baku</asp:ListItem>
				<asp:ListItem Value="Asia/Bangkok">Asia/Bangkok</asp:ListItem>
				<asp:ListItem Value="Asia/Beirut">Asia/Beirut</asp:ListItem>
				<asp:ListItem Value="Asia/Bishkek">Asia/Bishkek</asp:ListItem>
				<asp:ListItem Value="Asia/Brunei">Asia/Brunei</asp:ListItem>
				<asp:ListItem Value="Asia/Calcutta">Asia/Calcutta</asp:ListItem>
				<asp:ListItem Value="Asia/Choibalsan">Asia/Choibalsan</asp:ListItem>
				<asp:ListItem Value="Asia/Chongqing">Asia/Chongqing</asp:ListItem>
				<asp:ListItem Value="Asia/Colombo">Asia/Colombo</asp:ListItem>
				<asp:ListItem Value="Asia/Damascus">Asia/Damascus</asp:ListItem>
				<asp:ListItem Value="Asia/Dhaka">Asia/Dhaka</asp:ListItem>
				<asp:ListItem Value="Asia/Dili">Asia/Dili</asp:ListItem>
				<asp:ListItem Value="Asia/Dubai">Asia/Dubai</asp:ListItem>
				<asp:ListItem Value="Asia/Dushanbe">Asia/Dushanbe</asp:ListItem>
				<asp:ListItem Value="Asia/Gaza">Asia/Gaza</asp:ListItem>
				<asp:ListItem Value="Asia/Harbin">Asia/Harbin</asp:ListItem>
				<asp:ListItem Value="Asia/Hong_Kong">Asia/Hong_Kong</asp:ListItem>
				<asp:ListItem Value="Asia/Hovd">Asia/Hovd</asp:ListItem>
				<asp:ListItem Value="Asia/Irkutsk">Asia/Irkutsk</asp:ListItem>
				<asp:ListItem Value="Asia/Jakarta">Asia/Jakarta</asp:ListItem>
				<asp:ListItem Value="Asia/Jayapura">Asia/Jayapura</asp:ListItem>
				<asp:ListItem Value="Asia/Jerusalem">Asia/Jerusalem</asp:ListItem>
				<asp:ListItem Value="Asia/Kabul">Asia/Kabul</asp:ListItem>
				<asp:ListItem Value="Asia/Kamchatka">Asia/Kamchatka</asp:ListItem>
				<asp:ListItem Value="Asia/Karachi">Asia/Karachi</asp:ListItem>
				<asp:ListItem Value="Asia/Kashgar">Asia/Kashgar</asp:ListItem>
				<asp:ListItem Value="Asia/Katmandu">Asia/Katmandu</asp:ListItem>
				<asp:ListItem Value="Asia/Krasnoyarsk">Asia/Krasnoyarsk</asp:ListItem>
				<asp:ListItem Value="Asia/Kuala_Lumpur">Asia/Kuala_Lumpur</asp:ListItem>
				<asp:ListItem Value="Asia/Kuching">Asia/Kuching</asp:ListItem>
				<asp:ListItem Value="Asia/Kuwait">Asia/Kuwait</asp:ListItem>
				<asp:ListItem Value="Asia/Macau">Asia/Macau</asp:ListItem>
				<asp:ListItem Value="Asia/Magadan">Asia/Magadan</asp:ListItem>
				<asp:ListItem Value="Asia/Makassar">Asia/Makassar</asp:ListItem>
				<asp:ListItem Value="Asia/Manila">Asia/Manila</asp:ListItem>
				<asp:ListItem Value="Asia/Muscat">Asia/Muscat</asp:ListItem>
				<asp:ListItem Value="Asia/Nicosia">Asia/Nicosia</asp:ListItem>
				<asp:ListItem Value="Asia/Novosibirsk">Asia/Novosibirsk</asp:ListItem>
				<asp:ListItem Value="Asia/Omsk">Asia/Omsk</asp:ListItem>
				<asp:ListItem Value="Asia/Oral">Asia/Oral</asp:ListItem>
				<asp:ListItem Value="Asia/Phnom_Penh">Asia/Phnom_Penh</asp:ListItem>
				<asp:ListItem Value="Asia/Pontianak">Asia/Pontianak</asp:ListItem>
				<asp:ListItem Value="Asia/Pyongyang">Asia/Pyongyang</asp:ListItem>
				<asp:ListItem Value="Asia/Qatar">Asia/Qatar</asp:ListItem>
				<asp:ListItem Value="Asia/Qyzylorda">Asia/Qyzylorda</asp:ListItem>
				<asp:ListItem Value="Asia/Rangoon">Asia/Rangoon</asp:ListItem>
				<asp:ListItem Value="Asia/Riyadh">Asia/Riyadh</asp:ListItem>
				<asp:ListItem Value="Asia/Saigon">Asia/Saigon</asp:ListItem>
				<asp:ListItem Value="Asia/Sakhalin">Asia/Sakhalin</asp:ListItem>
				<asp:ListItem Value="Asia/Samarkand">Asia/Samarkand</asp:ListItem>
				<asp:ListItem Value="Asia/Seoul">Asia/Seoul</asp:ListItem>
				<asp:ListItem Value="Asia/Shanghai">Asia/Shanghai</asp:ListItem>
				<asp:ListItem Value="Asia/Singapore">Asia/Singapore</asp:ListItem>
				<asp:ListItem Value="Asia/Taipei">Asia/Taipei</asp:ListItem>
				<asp:ListItem Value="Asia/Tashkent">Asia/Tashkent</asp:ListItem>
				<asp:ListItem Value="Asia/Tbilisi">Asia/Tbilisi</asp:ListItem>
				<asp:ListItem Value="Asia/Tehran">Asia/Tehran</asp:ListItem>
				<asp:ListItem Value="Asia/Thimphu">Asia/Thimphu</asp:ListItem>
				<asp:ListItem Value="Asia/Tokyo">Asia/Tokyo</asp:ListItem>
				<asp:ListItem Value="Asia/Ulaanbaatar">Asia/Ulaanbaatar</asp:ListItem>
				<asp:ListItem Value="Asia/Urumqi">Asia/Urumqi</asp:ListItem>
				<asp:ListItem Value="Asia/Vientiane">Asia/Vientiane</asp:ListItem>
				<asp:ListItem Value="Asia/Vladivostok">Asia/Vladivostok</asp:ListItem>
				<asp:ListItem Value="Asia/Yakutsk">Asia/Yakutsk</asp:ListItem>
				<asp:ListItem Value="Asia/Yekaterinburg">Asia/Yekaterinburg</asp:ListItem>
				<asp:ListItem Value="Asia/Yerevan">Asia/Yerevan</asp:ListItem>
				<asp:ListItem Value="Atlantic/Azores">Atlantic/Azores</asp:ListItem>
				<asp:ListItem Value="Atlantic/Bermuda">Atlantic/Bermuda</asp:ListItem>
				<asp:ListItem Value="Atlantic/Canary">Atlantic/Canary</asp:ListItem>
				<asp:ListItem Value="Atlantic/Cape_Verde">Atlantic/Cape_Verde</asp:ListItem>
				<asp:ListItem Value="Atlantic/Faeroe">Atlantic/Faeroe</asp:ListItem>
				<asp:ListItem Value="Atlantic/Jan_Mayen">Atlantic/Jan_Mayen</asp:ListItem>
				<asp:ListItem Value="Atlantic/Madeira">Atlantic/Madeira</asp:ListItem>
				<asp:ListItem Value="Atlantic/Reykjavik">Atlantic/Reykjavik</asp:ListItem>
				<asp:ListItem Value="Atlantic/South_Georgia">Atlantic/South_Georgia</asp:ListItem>
				<asp:ListItem Value="Atlantic/St_Helena">Atlantic/St_Helena</asp:ListItem>
				<asp:ListItem Value="Atlantic/Stanley">Atlantic/Stanley</asp:ListItem>
				<asp:ListItem Value="Australia/Adelaide">Australia/Adelaide</asp:ListItem>
				<asp:ListItem Value="Australia/Brisbane">Australia/Brisbane</asp:ListItem>
				<asp:ListItem Value="Australia/Broken_Hill">Australia/Broken_Hill</asp:ListItem>
				<asp:ListItem Value="Australia/Currie">Australia/Currie</asp:ListItem>
				<asp:ListItem Value="Australia/Darwin">Australia/Darwin</asp:ListItem>
				<asp:ListItem Value="Australia/Hobart">Australia/Hobart</asp:ListItem>
				<asp:ListItem Value="Australia/Lindeman">Australia/Lindeman</asp:ListItem>
				<asp:ListItem Value="Australia/Lord_Howe">Australia/Lord_Howe</asp:ListItem>
				<asp:ListItem Value="Australia/Melbourne">Australia/Melbourne</asp:ListItem>
				<asp:ListItem Value="Australia/Perth">Australia/Perth</asp:ListItem>
				<asp:ListItem Value="Australia/Sydney">Australia/Sydney</asp:ListItem>
				<asp:ListItem Value="Europe/Amsterdam">Europe/Amsterdam</asp:ListItem>
				<asp:ListItem Value="Europe/Andorra">Europe/Andorra</asp:ListItem>
				<asp:ListItem Value="Europe/Athens">Europe/Athens</asp:ListItem>
				<asp:ListItem Value="Europe/Belgrade">Europe/Belgrade</asp:ListItem>
				<asp:ListItem Value="Europe/Berlin">Europe/Berlin</asp:ListItem>
				<asp:ListItem Value="Europe/Bratislava">Europe/Bratislava</asp:ListItem>
				<asp:ListItem Value="Europe/Brussels">Europe/Brussels</asp:ListItem>
				<asp:ListItem Value="Europe/Bucharest">Europe/Bucharest</asp:ListItem>
				<asp:ListItem Value="Europe/Budapest">Europe/Budapest</asp:ListItem>
				<asp:ListItem Value="Europe/Chisinau">Europe/Chisinau</asp:ListItem>
				<asp:ListItem Value="Europe/Copenhagen">Europe/Copenhagen</asp:ListItem>
				<asp:ListItem Value="Europe/Dublin">Europe/Dublin</asp:ListItem>
				<asp:ListItem Value="Europe/Gibraltar">Europe/Gibraltar</asp:ListItem>
				<asp:ListItem Value="Europe/Helsinki">Europe/Helsinki</asp:ListItem>
				<asp:ListItem Value="Europe/Istanbul">Europe/Istanbul</asp:ListItem>
				<asp:ListItem Value="Europe/Kaliningrad">Europe/Kaliningrad</asp:ListItem>
				<asp:ListItem Value="Europe/Kiev">Europe/Kiev</asp:ListItem>
				<asp:ListItem Value="Europe/Lisbon">Europe/Lisbon</asp:ListItem>
				<asp:ListItem Value="Europe/Ljubljana">Europe/Ljubljana</asp:ListItem>
				<asp:ListItem Value="Europe/Luxembourg">Europe/Luxembourg</asp:ListItem>
				<asp:ListItem Value="Europe/Madrid">Europe/Madrid</asp:ListItem>
				<asp:ListItem Value="Europe/Malta">Europe/Malta</asp:ListItem>
				<asp:ListItem Value="Europe/Mariehamn">Europe/Mariehamn</asp:ListItem>
				<asp:ListItem Value="Europe/Minsk">Europe/Minsk</asp:ListItem>
				<asp:ListItem Value="Europe/Monaco">Europe/Monaco</asp:ListItem>
				<asp:ListItem Value="Europe/Moscow">Europe/Moscow</asp:ListItem>
				<asp:ListItem Value="Europe/Oslo">Europe/Oslo</asp:ListItem>
				<asp:ListItem Value="Europe/Paris">Europe/Paris</asp:ListItem>
				<asp:ListItem Value="Europe/Prague">Europe/Prague</asp:ListItem>
				<asp:ListItem Value="Europe/Riga">Europe/Riga</asp:ListItem>
				<asp:ListItem Value="Europe/Rome">Europe/Rome</asp:ListItem>
				<asp:ListItem Value="Europe/Samara">Europe/Samara</asp:ListItem>
				<asp:ListItem Value="Europe/San_Marino">Europe/San_Marino</asp:ListItem>
				<asp:ListItem Value="Europe/Sarajevo">Europe/Sarajevo</asp:ListItem>
				<asp:ListItem Value="Europe/Simferopol">Europe/Simferopol</asp:ListItem>
				<asp:ListItem Value="Europe/Skopje">Europe/Skopje</asp:ListItem>
				<asp:ListItem Value="Europe/Sofia">Europe/Sofia</asp:ListItem>
				<asp:ListItem Value="Europe/Stockholm">Europe/Stockholm</asp:ListItem>
				<asp:ListItem Value="Europe/Tallinn">Europe/Tallinn</asp:ListItem>
				<asp:ListItem Value="Europe/Tirane">Europe/Tirane</asp:ListItem>
				<asp:ListItem Value="Europe/Uzhgorod">Europe/Uzhgorod</asp:ListItem>
				<asp:ListItem Value="Europe/Vaduz">Europe/Vaduz</asp:ListItem>
				<asp:ListItem Value="Europe/Vatican">Europe/Vatican</asp:ListItem>
				<asp:ListItem Value="Europe/Vienna">Europe/Vienna</asp:ListItem>
				<asp:ListItem Value="Europe/Vilnius">Europe/Vilnius</asp:ListItem>
				<asp:ListItem Value="Europe/Warsaw">Europe/Warsaw</asp:ListItem>
				<asp:ListItem Value="Europe/Zagreb">Europe/Zagreb</asp:ListItem>
				<asp:ListItem Value="Europe/Zaporozhye">Europe/Zaporozhye</asp:ListItem>
				<asp:ListItem Value="Europe/Zurich">Europe/Zurich</asp:ListItem>
				<asp:ListItem Value="Indian/Antananarivo">Indian/Antananarivo</asp:ListItem>
				<asp:ListItem Value="Indian/Chagos">Indian/Chagos</asp:ListItem>
				<asp:ListItem Value="Indian/Christmas">Indian/Christmas</asp:ListItem>
				<asp:ListItem Value="Indian/Cocos">Indian/Cocos</asp:ListItem>
				<asp:ListItem Value="Indian/Comoro">Indian/Comoro</asp:ListItem>
				<asp:ListItem Value="Indian/Kerguelen">Indian/Kerguelen</asp:ListItem>
				<asp:ListItem Value="Indian/Mahe">Indian/Mahe</asp:ListItem>
				<asp:ListItem Value="Indian/Maldives">Indian/Maldives</asp:ListItem>
				<asp:ListItem Value="Indian/Mauritius">Indian/Mauritius</asp:ListItem>
				<asp:ListItem Value="Indian/Mayotte">Indian/Mayotte</asp:ListItem>
				<asp:ListItem Value="Indian/Reunion">Indian/Reunion</asp:ListItem>
				<asp:ListItem Value="Pacific/Apia">Pacific/Apia</asp:ListItem>
				<asp:ListItem Value="Pacific/Auckland">Pacific/Auckland</asp:ListItem>
				<asp:ListItem Value="Pacific/Chatham">Pacific/Chatham</asp:ListItem>
				<asp:ListItem Value="Pacific/Easter">Pacific/Easter</asp:ListItem>
				<asp:ListItem Value="Pacific/Efate">Pacific/Efate</asp:ListItem>
				<asp:ListItem Value="Pacific/Enderbury">Pacific/Enderbury</asp:ListItem>
				<asp:ListItem Value="Pacific/Fakaofo">Pacific/Fakaofo</asp:ListItem>
				<asp:ListItem Value="Pacific/Fiji">Pacific/Fiji</asp:ListItem>
				<asp:ListItem Value="Pacific/Funafuti">Pacific/Funafuti</asp:ListItem>
				<asp:ListItem Value="Pacific/Galapagos">Pacific/Galapagos</asp:ListItem>
				<asp:ListItem Value="Pacific/Gambier">Pacific/Gambier</asp:ListItem>
				<asp:ListItem Value="Pacific/Guadalcanal">Pacific/Guadalcanal</asp:ListItem>
				<asp:ListItem Value="Pacific/Guam">Pacific/Guam</asp:ListItem>
				<asp:ListItem Value="Pacific/Honolulu">Pacific/Honolulu</asp:ListItem>
				<asp:ListItem Value="Pacific/Johnston">Pacific/Johnston</asp:ListItem>
				<asp:ListItem Value="Pacific/Kiritimati">Pacific/Kiritimati</asp:ListItem>
				<asp:ListItem Value="Pacific/Kosrae">Pacific/Kosrae</asp:ListItem>
				<asp:ListItem Value="Pacific/Kwajalein">Pacific/Kwajalein</asp:ListItem>
				<asp:ListItem Value="Pacific/Majuro">Pacific/Majuro</asp:ListItem>
				<asp:ListItem Value="Pacific/Marquesas">Pacific/Marquesas</asp:ListItem>
				<asp:ListItem Value="Pacific/Midway">Pacific/Midway</asp:ListItem>
				<asp:ListItem Value="Pacific/Nauru">Pacific/Nauru</asp:ListItem>
				<asp:ListItem Value="Pacific/Niue">Pacific/Niue</asp:ListItem>
				<asp:ListItem Value="Pacific/Norfolk">Pacific/Norfolk</asp:ListItem>
				<asp:ListItem Value="Pacific/Noumea">Pacific/Noumea</asp:ListItem>
				<asp:ListItem Value="Pacific/Pago_Pago">Pacific/Pago_Pago</asp:ListItem>
				<asp:ListItem Value="Pacific/Palau">Pacific/Palau</asp:ListItem>
				<asp:ListItem Value="Pacific/Pitcairn">Pacific/Pitcairn</asp:ListItem>
				<asp:ListItem Value="Pacific/Ponape">Pacific/Ponape</asp:ListItem>
				<asp:ListItem Value="Pacific/Port_Moresby">Pacific/Port_Moresby</asp:ListItem>
				<asp:ListItem Value="Pacific/Rarotonga">Pacific/Rarotonga</asp:ListItem>
				<asp:ListItem Value="Pacific/Saipan">Pacific/Saipan</asp:ListItem>
				<asp:ListItem Value="Pacific/Tahiti">Pacific/Tahiti</asp:ListItem>
				<asp:ListItem Value="Pacific/Tarawa">Pacific/Tarawa</asp:ListItem>
				<asp:ListItem Value="Pacific/Tongatapu">Pacific/Tongatapu</asp:ListItem>
				<asp:ListItem Value="Pacific/Truk">Pacific/Truk</asp:ListItem>
				<asp:ListItem Value="Pacific/Wake">Pacific/Wake</asp:ListItem>
				<asp:ListItem Value="Pacific/Wallis">Pacific/Wallis</asp:ListItem>
			</asp:DropDownList>
			&nbsp;
		</td>
	</tr>
	<tr>
		<td>
		</td>
		<td>
		</td>
	</tr>
	<tr>
		<td colspan="2">
			Note: Fields marked with a * are required by the consumer
		</td>
	</tr>
</table>
