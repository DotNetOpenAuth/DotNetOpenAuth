<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.1"
				xmlns:MSHelp="http://msdn.microsoft.com/mshelp"
        xmlns:mshelp="http://msdn.microsoft.com/mshelp"
				xmlns:ddue="http://ddue.schemas.microsoft.com/authoring/2003/5"
				xmlns:xlink="http://www.w3.org/1999/xlink"
        xmlns:msxsl="urn:schemas-microsoft-com:xslt"
>

	<xsl:output method="xml" indent="no" encoding="utf-8" />

  <xsl:param name="changeHistoryOptions" />
  <xsl:include href="htmlBody.xsl" />
	<xsl:include href="utilities_dduexml.xsl" />
  <xsl:include href="seeAlsoSection.xsl" />
  <xsl:include href="conceptualMetadataHelp30.xsl"/>
  <xsl:include href="conceptualMetadataHelp20.xsl"/>

  <xsl:variable name="hasSeeAlsoSection" select="boolean(count(/document/topic/*/ddue:relatedTopics/*[local-name()!='sampleRef']) > 0)"/>
  <xsl:variable name="examplesSection" select="boolean(string-length(/document/topic/*/ddue:codeExample[normalize-space(.)]) > 0)"/>
  <xsl:variable name="languageFilterSection" select="normalize-space(/document/topic/*/ddue:codeExample) 
                or normalize-space(/document/topic/*//ddue:snippets/ddue:snippet)
                or /document/topic/ddue:developerSampleDocument/ddue:relatedTopics/ddue:sampleRef[@srcID]" />
  <xsl:variable name="group" select="/document/reference/apidata/@group" />
  <xsl:variable name="subgroup" select="/document/reference/apidata/@subgroup" />
  <xsl:variable name="subsubgroup" select="/document/reference/apidata/@subsubgroup" />
  <xsl:variable name="pseudo" select="boolean(/document/reference/apidata[@pseudo='true'])"/>
  <!-- key parameter is the api identifier string -->
	<xsl:param name="key" />
  <xsl:param name="metadata" value="false" />
  <xsl:param name="languages">false</xsl:param>

	<xsl:template match="/document">
    <html xmlns:xlink="http://www.w3.org/1999/xlink">
      <head>
        <META HTTP-EQUIV="Content-Type" CONTENT="text/html; charset=UTF-8"/>
        <META NAME="save" CONTENT="history"/>
        <xsl:call-template name="insertNoIndexNoFollow" />
        <title>
          <xsl:call-template name="topicTitlePlain"/>
        </title>
        <xsl:call-template name="insert30Metadata" />
        <xsl:call-template name="insertStylesheets" />
        <xsl:call-template name="insertScripts" />
        <xsl:call-template name="insertMetadata" />
      </head>
      <body>
        <xsl:call-template name="upperBodyStuff"/>
        <xsl:call-template name="main"/>
      </body>
    </html>
  </xsl:template>
	
	<!-- document head -->

  <xsl:template name="insertNoIndexNoFollow">
    <xsl:if test="/document/metadata/attribute[@name='NoSearch']">
      <META NAME="ROBOTS" CONTENT="NOINDEX, NOFOLLOW" />
    </xsl:if>
  </xsl:template>
  
  <xsl:template name="insertStylesheets">
    <link rel="stylesheet" type="text/css">
      <includeAttribute name="href" item="stylePath">
        <parameter>presentation.css</parameter>
      </includeAttribute>
    </link>
    <!-- make mshelp links work -->
    <link rel="stylesheet" type="text/css" href="ms-help://Hx/HxRuntime/HxLink.css" />
    <!--<link rel="stylesheet" type="text/css" href="ms-help://Dx/DxRuntime/DxLink.css" />-->
  </xsl:template>

  <xsl:template name="insertScripts">
    <script type="text/javascript">
      <includeAttribute name="src" item="scriptPath">
        <parameter>EventUtilities.js</parameter>
      </includeAttribute>
      <xsl:text> </xsl:text>
    </script>
    <script type="text/javascript">
      <includeAttribute name="src" item="scriptPath">
        <parameter>SplitScreen.js</parameter>
      </includeAttribute>
      <xsl:text> </xsl:text>
    </script>
    <script type="text/javascript">
      <includeAttribute name="src" item="scriptPath">
        <parameter>Dropdown.js</parameter>
      </includeAttribute>
      <xsl:text> </xsl:text>
    </script>
    <script type="text/javascript">
      <includeAttribute name="src" item="scriptPath">
        <parameter>script_manifold.js</parameter>
      </includeAttribute>
      <xsl:text> </xsl:text>
    </script>
    <script type="text/javascript">
      <includeAttribute name="src" item="scriptPath">
        <parameter>script_feedBack.js</parameter>
      </includeAttribute>
      <xsl:text> </xsl:text>
    </script>
    <script type="text/javascript">
      <includeAttribute name="src" item="scriptPath">
        <parameter>CheckboxMenu.js</parameter>
      </includeAttribute>
      <xsl:text> </xsl:text>
    </script>
    <script type="text/javascript">
      <includeAttribute name="src" item="scriptPath">
        <parameter>CommonUtilities.js</parameter>
      </includeAttribute>
      <xsl:text> </xsl:text>
    </script>

  </xsl:template>


  <xsl:template name="nestedKeywordText">
    <xsl:for-each select="keyword[@index='K']">
      <xsl:text>, </xsl:text>
      <xsl:value-of select="text()"/>
    </xsl:for-each>
  </xsl:template>
  
	<!-- document body -->

	<!-- Title in topic -->

	<xsl:template name="topicTitleDecorated">
    <xsl:call-template name="topicTitle" />
	</xsl:template>

  <xsl:template name="topicTitlePlain">
    <xsl:call-template name="topicTitle" />
  </xsl:template>

  <xsl:template name="topicTitle">
    <xsl:choose>
      <xsl:when test="normalize-space(/document/metadata/title)">
        <xsl:value-of select="normalize-space(/document/metadata/title)"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="normalize-space(/document/topic/*/ddue:title)"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- Title in TOC -->

	<!-- Index entry -->

	<!-- main window -->

  <xsl:template name="main">
    <div id="mainSection">

      <div id="mainBody">
        <div id="allHistory" class="saveHistory" onsave="saveAll()" onload="loadAll()"/>

        <!-- 'header' shared content item is used to show optional boilerplate at the top of the topic's scrolling region, e.g. pre-release boilerplate -->
        <include item="header" />

        <xsl:call-template name="body" />
      </div>
      <xsl:call-template name="foot" />
    </div>

  </xsl:template>

	<xsl:template name="body">
    <!-- freshness date -->
    <xsl:call-template name="writeFreshnessDate">
      <xsl:with-param name="ChangedHistoryDate" select="/document/topic/*//ddue:section[ddue:title = 'Change History']/ddue:content/ddue:table/ddue:row[1]/ddue:entry[1] |
                      /document/topic/*/ddue:changeHistory/ddue:content/ddue:table/ddue:row[1]/ddue:entry[1]" />
    </xsl:call-template>

		<xsl:apply-templates select="topic" />
    
    <!-- changed table section -->
    <xsl:call-template name="writeChangeHistorySection" />
	</xsl:template>

	<!-- sections that behave differently in conceptual and reference -->

	<xsl:template match="ddue:title">
		<!-- don't print title -->
	</xsl:template>

	<xsl:template match="ddue:introduction">
    <xsl:apply-templates select="@address" />
		<div class="introduction">
			<xsl:apply-templates />
		</div>
	</xsl:template>

	<xsl:template match="ddue:parameters">
    <xsl:if test="normalize-space(.)">
		<xsl:call-template name="section">
      <xsl:with-param name="toggleSwitch" select="'parameters'"/>
			<xsl:with-param name="title"><include item="parametersTitle" /></xsl:with-param>
			<xsl:with-param name="content">
				<xsl:apply-templates />
			</xsl:with-param>
		</xsl:call-template>
    </xsl:if>
	</xsl:template>

	<xsl:template match="ddue:returnValue">
    <xsl:if test="normalize-space(.)">
      <xsl:choose>
        <xsl:when test="(normalize-space(ddue:content)='') and ddue:sections/ddue:section[ddue:title='Property Value']">
          <xsl:call-template name="section">
            <xsl:with-param name="toggleSwitch" select="'returnValue'"/>
            <xsl:with-param name="title">
              <include item="propertyValueTitle" />
            </xsl:with-param>
            <xsl:with-param name="content">
              <xsl:apply-templates select="ddue:sections/ddue:section[ddue:title='Property Value']/*" />
            </xsl:with-param>
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:call-template name="section">
            <xsl:with-param name="toggleSwitch" select="'returnValue'"/>
            <xsl:with-param name="title">
              <include item="returnValueTitle" />
            </xsl:with-param>
            <xsl:with-param name="content">
              <xsl:apply-templates />
            </xsl:with-param>
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>
	</xsl:template>

	<xsl:template match="ddue:exceptions">
    <xsl:if test="normalize-space(.)">
		<xsl:call-template name="section">
      <xsl:with-param name="toggleSwitch" select="'ddueExceptions'"/>
			<xsl:with-param name="title"><include item="exceptionsTitle" /></xsl:with-param>
			<xsl:with-param name="content">
				<xsl:apply-templates />
			</xsl:with-param>
		</xsl:call-template>
    </xsl:if>
	</xsl:template>

	<xsl:template match="ddue:relatedSections">
    <xsl:if test="normalize-space(.)">
    <xsl:call-template name="section">
      <xsl:with-param name="toggleSwitch" select="'relatedSections'"/>
      <xsl:with-param name="title"><include item="relatedSectionsTitle" /></xsl:with-param>
      <xsl:with-param name="content">
        <xsl:apply-templates />
      </xsl:with-param>
    </xsl:call-template>
    </xsl:if>
	</xsl:template>

  <xsl:template match="ddue:relatedTopics">
    <xsl:if test="$hasSeeAlsoSection">
      <xsl:call-template name="section">
        <xsl:with-param name="toggleSwitch" select="'seeAlso'"/>
        <xsl:with-param name="title">
          <include item="relatedTopicsTitle" />
        </xsl:with-param>
        <xsl:with-param name="content">
          <xsl:apply-templates select="/document/topic/*/ddue:relatedTopics" mode="seeAlso" />
        </xsl:with-param>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>
    
  <xsl:template match="ddue:codeExample">
    <!-- create Example section for the first codeExample node -->
    <xsl:if test="not(preceding-sibling::ddue:codeExample) and ../ddue:codeExample[normalize-space(.)!='']">
      <xsl:call-template name="section">
        <xsl:with-param name="toggleSwitch" select="'example'"/>
        <xsl:with-param name="title">
          <include item="Example" />
        </xsl:with-param>
        <xsl:with-param name="content">
          <xsl:apply-templates />
          <!-- if there are additional codeExample nodes, put them inside this section -->
          <xsl:for-each select="following-sibling::ddue:codeExample">
            <xsl:apply-templates />
          </xsl:for-each>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template match="ddue:codeReference">
    <xsl:apply-templates/>
  </xsl:template>

  <xsl:template name="runningHeader">
    <xsl:variable name="runningHeaderText">
      <xsl:value-of select="/document/metadata/runningHeaderText/@uscid"/>
    </xsl:variable>
    <include item="{$runningHeaderText}" />
  </xsl:template>

	<!-- Footer stuff -->

  <xsl:template name="foot">
    <div id="footer">
      <div class="footerLine">
        <img width="100%" height="3px">
          <includeAttribute name="src" item="iconPath">
            <parameter>footer.gif</parameter>
          </includeAttribute>
          <includeAttribute name="alt" item="footerImage" />
          <includeAttribute name="title" item="footerImage" />
        </img>
      </div>

      <include item="footer">
        <parameter>
          <xsl:value-of select="$key"/>
        </parameter>
        <parameter>
          <xsl:call-template name="topicTitlePlain"/>
        </parameter>
        <parameter>
          <xsl:value-of select="/document/metadata/item[@id='PBM_FileVersion']" />
        </parameter>
        <parameter>
          <xsl:value-of select="/document/metadata/attribute[@name='TopicVersion']" />
        </parameter>
      </include>
    </div>
  </xsl:template>

  <!-- autoOutline
  Inserts a bullet list of links to the topic's top-level sections or a section's subsections.
  Authors can insert <token>autoOutline</token> in a topic's introduction to get a bullet list of the top-level sections;
  or in a ddue:section/ddue:content to get a bullet list of the section's subsections.
  The shared content component replaces <token>autoOutline</token> with an <autoOutline/> node.
  -->
  <xsl:template match="autoOutline">
    <xsl:choose>
      <!--if <autoOutline/> is in introduction, it outlines the topic's toplevel sections-->
      <xsl:when test="ancestor::ddue:introduction">
        <xsl:for-each select="ancestor::ddue:introduction/parent::*">
          <xsl:call-template name="insertAutoOutline">
            <xsl:with-param name="outlineType">toplevel</xsl:with-param>
          </xsl:call-template>
        </xsl:for-each>
      </xsl:when>
      <!--if <autoOutline/> is in section/content, it outlines the section's subsections-->
      <xsl:when test="ancestor::ddue:content[parent::ddue:section]">
        <xsl:for-each select="ancestor::ddue:content/parent::ddue:section/ddue:sections">
          <xsl:call-template name="insertAutoOutline">
            <xsl:with-param name="outlineType">subsection</xsl:with-param>
          </xsl:call-template>
        </xsl:for-each>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="insertAutoOutline">
    <xsl:param name="outlineType"/>
    <!--insert an outline if there are sections with title and address-->
    <xsl:if test="ddue:section[ddue:title[normalize-space(.)!='']]">
      <!--insert a boilerplate intro-->
      <xsl:choose>
        <xsl:when test="$outlineType='toplevel'">
          <include item="autoOutlineTopLevelIntro"/>
        </xsl:when>
        <xsl:when test="$outlineType='subsection'">
          <include item="autoOutlineSubsectionIntro"/>
        </xsl:when>
      </xsl:choose>
      <ul>
        <xsl:for-each select="ddue:section[ddue:title[normalize-space(.)!='']]">
          <xsl:call-template name="outlineSectionEntry" />
        </xsl:for-each>
        <!--for toplevel outlines include a link to See Also-->
        <xsl:if test="starts-with($outlineType,'toplevel') and //ddue:relatedTopics[normalize-space(.)!='']">
          <li>
            <A>
              <xsl:attribute name="HREF">#seeAlsoToggle</xsl:attribute>
              <include item="RelatedTopicsLinkText"/>
            </A>
          </li>
        </xsl:if>
      </ul>
    </xsl:if>
  </xsl:template>

  <!--a list item in the outline's bullet list-->
  <xsl:template name="outlineSectionEntry">
    <li>
      <A>
        <xsl:if test="@address">
          <xsl:attribute name="HREF">
            #<xsl:value-of select="@address"/>
          </xsl:attribute>
        </xsl:if>
        <xsl:value-of select="ddue:title" />
      </A>
    </li>
  </xsl:template>

</xsl:stylesheet>
