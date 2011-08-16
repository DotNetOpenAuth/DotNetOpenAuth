<?xml version="1.0" encoding="utf-8"?>
<!-- Report generation template for PartCover by Gaspar Nagy, TechTalk -->
<!-- version 1.0 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxml="urn:schemas-microsoft-com:xslt"
                xmlns:exsl="http://exslt.org/common"
                extension-element-prefixes="exsl">
  <xsl:output method="html" indent="yes" doctype-system="http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd" doctype-public="-//W3C//DTD XHTML 1.0 Transitional//EN"/>
  
  <xsl:param name="simpleMethodMaxSize" select="number('20')" />

  <xsl:template name="calculate-coverage">
    <xsl:param name="methods" />

    <xsl:variable name="codeSize" select="sum($methods/@bodysize)"/>
    <xsl:variable name="nonCoveredCodeSize" select="sum($methods/pt[@visit=0]/@len)+sum($methods[count(pt)=0]/@bodysize)"/>

    <xsl:choose>
      <xsl:when test="$codeSize=0">0</xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="100 - ceiling(100 * $nonCoveredCodeSize div $codeSize)"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="calculate-count-coverage">
    <xsl:param name="elements" />

    <xsl:variable name="elementCount" select="count($elements)"/>
    <xsl:variable name="visitedElementCount" select="count($elements[.//pt[@visit!=0]])"/>

    <xsl:choose>
      <xsl:when test="$elementCount=0">0</xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="ceiling(100 * $visitedElementCount div $elementCount)"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="show-hide-button">
    <xsl:param name="controlId" />
    
    <xsl:text> </xsl:text>
    <a href="#" onclick="toggle('{$controlId}', event); return false;" class="button">[show]</a>
  </xsl:template>

  <xsl:template name="get-coverage-class">
    <xsl:param name="coverage" />
    <xsl:choose>
      <xsl:when test="$coverage = 'n/a'">coverage100</xsl:when>
      <xsl:when test="$coverage &gt;=  0 and $coverage &lt; 20">coverage20</xsl:when>
      <xsl:when test="$coverage &gt;= 20 and $coverage &lt; 40">coverage40</xsl:when>
      <xsl:when test="$coverage &gt;= 40 and $coverage &lt; 60">coverage60</xsl:when>
      <xsl:when test="$coverage &gt;= 60 and $coverage &lt; 80">coverage80</xsl:when>
      <xsl:when test="$coverage &gt;= 80">coverage100</xsl:when>
      <xsl:otherwise>coverage0</xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  
  <xsl:template name="coverage-line">
    <xsl:param name="name" select="Name" />
    <xsl:param name="coverage" select="Coverage" />
    <xsl:param name="nonSimpleCoverage" select="NonSimpleCoverage" />
    <xsl:param name="id" select="Id" />
    <xsl:param name="toggleButton" select="false()" />
    <xsl:param name="details" select="''" />
    <xsl:param name="link" select="''" />

    <tr>
      <xsl:if test="Size and Size &lt;= $simpleMethodMaxSize">
        <xsl:attribute name="class">simplemethod</xsl:attribute>
      </xsl:if>
      <td class="left">
        <xsl:choose>
          <xsl:when test="$link != ''">
            <a href="{$link}">
              <xsl:value-of select="$name"/>
            </a>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$name"/>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:if test="$toggleButton and $coverage != 100">
          <xsl:call-template name="show-hide-button">
            <xsl:with-param name="controlId" select="concat('details', $id)" />
          </xsl:call-template>
        </xsl:if>
        <xsl:if test="$details != ''">
          <br />
          <span class="coverageDetails"><xsl:value-of select="$details"/></span>
        </xsl:if>
      </td>
      <td>
        <xsl:attribute name="class">
          <xsl:text>coverage numeric </xsl:text>
          <xsl:call-template name="get-coverage-class">
            <xsl:with-param name="coverage" select="$coverage" />
          </xsl:call-template>
        </xsl:attribute>
        <xsl:value-of select="$coverage"/>
        <xsl:text>%</xsl:text>
      </td>
      <xsl:if test="$nonSimpleCoverage">
        <td>
          <xsl:attribute name="class">
            <xsl:text>coverage numeric </xsl:text>
            <xsl:call-template name="get-coverage-class">
              <xsl:with-param name="coverage" select="$nonSimpleCoverage" />
            </xsl:call-template>
          </xsl:attribute>
          <xsl:choose>
            <xsl:when test="$nonSimpleCoverage = 'n/a'">n/a</xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$nonSimpleCoverage"/>%
            </xsl:otherwise>
          </xsl:choose>
        </td>
      </xsl:if>
    </tr>
  </xsl:template>

  <xsl:template match="Assembly" mode="calculate-coverage">
    <xsl:variable name="assemblyId" select="@id" />
    <CoverageEntry>
      <Id>
        <xsl:value-of select="$assemblyId"/>
      </Id>
      <Name>
        <xsl:value-of select="@name"/>
      </Name>
      <Coverage>
        <xsl:call-template name="calculate-coverage">
          <xsl:with-param name="methods" select="/PartCoverReport/Type[@asmref=$assemblyId]/Method" />
        </xsl:call-template>
      </Coverage>
      <NonSimpleCoverage>
        <xsl:choose>
          <xsl:when test="/PartCoverReport/Type[@asmref=$assemblyId]/Method[@bodysize &gt; $simpleMethodMaxSize]">
            <xsl:call-template name="calculate-coverage">
              <xsl:with-param name="methods" select="/PartCoverReport/Type[@asmref=$assemblyId]/Method[@bodysize &gt; $simpleMethodMaxSize]" />
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>n/a</xsl:otherwise>
        </xsl:choose>
      </NonSimpleCoverage>
    </CoverageEntry>
  </xsl:template>
  
  <xsl:template match="Type" mode="calculate-coverage">
    <CoverageEntry>
      <Id>
        <xsl:value-of select="generate-id(.)"/>
      </Id>
      <AssemblyId>
        <xsl:value-of select="@asmref"/>
      </AssemblyId>
      <Name>
        <xsl:value-of select="@name"/>
      </Name>
      <Coverage>
        <xsl:call-template name="calculate-coverage">
          <xsl:with-param name="methods" select="Method" />
        </xsl:call-template>
      </Coverage>
      <NonSimpleCoverage>
        <xsl:choose>
          <xsl:when test="Method[@bodysize &gt; $simpleMethodMaxSize]">
            <xsl:call-template name="calculate-coverage">
              <xsl:with-param name="methods" select="Method[@bodysize &gt; $simpleMethodMaxSize]" />
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>n/a</xsl:otherwise>
        </xsl:choose>
      </NonSimpleCoverage>
    </CoverageEntry>
  </xsl:template>

  <xsl:template match="Method" mode="calculate-coverage">
    <xsl:variable name="nameWithSig" select="concat(@name, '(', substring-after(@sig, '('))" />
    <CoverageEntry>
      <Id>
        <xsl:value-of select="generate-id(.)"/>
      </Id>
      <TypeId>
        <xsl:value-of select="generate-id(..)"/>
      </TypeId>
      <Name>
        <xsl:value-of select="$nameWithSig"/>
      </Name>
      <FullName>
        <xsl:value-of select="concat(../@name, '.', $nameWithSig)"/>
      </FullName>
      <Coverage>
        <xsl:call-template name="calculate-coverage">
          <xsl:with-param name="methods" select="." />
        </xsl:call-template>
      </Coverage>
      <Size>
        <xsl:value-of select="@bodysize"/>
      </Size>
    </CoverageEntry>
  </xsl:template>

  <xsl:template match="/">
    <xsl:variable name="title">
      Code Coverage Report
    </xsl:variable>
    <html>
      <xsl:variable name="methods" select="/PartCoverReport/Type/Method" />
      
      <xsl:variable name="assemblyCoverage">
        <xsl:apply-templates select="/PartCoverReport/Assembly" mode="calculate-coverage">
          <xsl:sort select="@name" order="ascending"/>
        </xsl:apply-templates>
      </xsl:variable>
      
      <xsl:variable name="typeCoverage">
        <xsl:apply-templates select="/PartCoverReport/Type" mode="calculate-coverage">
          <xsl:sort select="@name" order="ascending"/>
        </xsl:apply-templates>
      </xsl:variable>
      
      <xsl:variable name="methodCoverage">
        <xsl:apply-templates select="$methods" mode="calculate-coverage">
          <xsl:sort select="@name" order="ascending"/>
          <xsl:sort select="@sig" order="ascending"/>
        </xsl:apply-templates>
      </xsl:variable>
      
      <xsl:call-template name="html-header">
        <xsl:with-param name="title" select="$title" />
      </xsl:call-template>

      <body>
        <xsl:call-template name="html-body-header">
          <xsl:with-param name="title" select="$title" />
        </xsl:call-template>
        <h2>Summary</h2>
        <table class="reportTable" cellpadding="0" cellspacing="0">
          <tr>
            <th class="top left">Assembly Name</th>
            <th class="top coverage numeric">Coverage</th>
            <th class="top coverage numeric">Coverage (w/o simple members)</th>
          </tr>
          <tr>
            <td class="left">Class</td>
            <td class="coverage numeric">
              <xsl:call-template name="calculate-count-coverage">
                <xsl:with-param name="elements" select="/PartCoverReport/Type" />
              </xsl:call-template>%
            </td>
            <td class="coverage numeric">
              <xsl:call-template name="calculate-count-coverage">
                <xsl:with-param name="elements" select="/PartCoverReport/Type[Method[@bodysize &gt; $simpleMethodMaxSize]]" />
              </xsl:call-template>%
            </td>
          </tr>
          <tr>
            <td class="left">Method</td>
            <td class="coverage numeric">
              <xsl:call-template name="calculate-count-coverage">
                <xsl:with-param name="elements" select="/PartCoverReport/Type/Method" />
              </xsl:call-template>%
            </td>
            <td class="coverage numeric">
              <xsl:call-template name="calculate-count-coverage">
                <xsl:with-param name="elements" select="/PartCoverReport/Type/Method[@bodysize &gt; $simpleMethodMaxSize]" />
              </xsl:call-template>%
            </td>
          </tr>
          <tr>
            <td class="left">Line</td>
            <td class="coverage numeric">
              <xsl:call-template name="calculate-coverage">
                <xsl:with-param name="methods" select="/PartCoverReport/Type/Method" />
              </xsl:call-template>%
            </td>
            <td class="coverage numeric">
              <xsl:call-template name="calculate-coverage">
                <xsl:with-param name="methods" select="/PartCoverReport/Type/Method[@bodysize &gt; $simpleMethodMaxSize]" />
              </xsl:call-template>%
            </td>
          </tr>
        </table>

        <hr />
        <h2>Top 10 Uncovered Method</h2>
        <div class="subtitle">w/o simple members, higher impact first</div>
        
        <table class="reportTable" cellpadding="0" cellspacing="0">
          <tr>
            <th class="top left">Method</th>
            <th class="top coverage numeric">Line Coverage</th>
          </tr>
          <xsl:for-each select="exsl:node-set($methodCoverage)/CoverageEntry[Size &gt; $simpleMethodMaxSize and Coverage &lt; 100]">
            <xsl:sort select="Size * (100 - Coverage)" order="descending" data-type="number"/>
            <xsl:sort select="Coverage" order="ascending" data-type="number"/>
            <xsl:if test="position() &lt;= 10">
              <xsl:call-template name="method-coverage-line">
                <xsl:with-param name="methods" select="$methods" />
                <xsl:with-param name="fullName" select="true()" />
              </xsl:call-template>
            </xsl:if>
          </xsl:for-each>
        </table>

        <hr />
        <h2>Assembly Coverage Summary</h2>
          <table class="reportTable" cellpadding="0" cellspacing="0">
            <tr>
              <th class="top left">Assembly Name</th>
              <th class="top coverage numeric">Line Coverage</th>
              <th class="top coverage numeric">Line Coverage (w/o simple members)</th>
            </tr>
            <xsl:for-each select="exsl:node-set($assemblyCoverage)/CoverageEntry">
              <xsl:call-template name="coverage-line">
                <xsl:with-param name="link" select="concat('#al', Id)" />
              </xsl:call-template>
            </xsl:for-each>
          </table>
          <hr />
        <h2>Detailed Report</h2>
        <div class="subtitle">simple members are gray</div>
        <xsl:for-each select="exsl:node-set($assemblyCoverage)/CoverageEntry">
          <xsl:variable name="assemblyId" select="Id" />
          <a name="#al{$assemblyId}"/>
          <h3>
            Assembly: <xsl:value-of select="Name"/>
          </h3>
          <table class="reportTable" cellpadding="0" cellspacing="0">
            <tr>
              <th class="top left">Class Name</th>
              <th class="top coverage numeric">Line Coverage</th>
              <th class="top coverage numeric">Line Coverage (w/o simple members)</th>
            </tr>
            <xsl:for-each select="exsl:node-set($typeCoverage)/CoverageEntry[AssemblyId = $assemblyId]">
              <xsl:variable name="typeId" select="Id" />
              <xsl:call-template name="coverage-line" >
                <xsl:with-param name="toggleButton" select="true()" />
              </xsl:call-template>
              <tr id="details{$typeId}" style="display:none;">
                <td colspan="3" class="left subRow">
                  <table class="subReportTable" cellpadding="0" cellspacing="0">
                    <tr>
                      <th class="top left">Method</th>
                      <th class="top coverage numeric">Line Coverage</th>
                    </tr>
                    <xsl:for-each select="exsl:node-set($methodCoverage)/CoverageEntry[TypeId = $typeId and Size &gt; $simpleMethodMaxSize]">
                      <xsl:call-template name="method-coverage-line">
                        <xsl:with-param name="methods" select="$methods" />
                      </xsl:call-template>
                    </xsl:for-each>
                    <xsl:for-each select="exsl:node-set($methodCoverage)/CoverageEntry[TypeId = $typeId and Size &lt;= $simpleMethodMaxSize]">
                      <xsl:call-template name="method-coverage-line">
                        <xsl:with-param name="methods" select="$methods" />
                      </xsl:call-template>
                    </xsl:for-each>
                  </table>
                </td>
              </tr>
            </xsl:for-each>
          </table>
        </xsl:for-each>
        <xsl:call-template name="html-body-footer" />
      </body>
    </html>
  </xsl:template>

  <xsl:template name="method-coverage-line">
    <xsl:param name="methods" />
    <xsl:param name="fullName" select="false()" />
    <xsl:variable name="methodId" select="Id" />
    <xsl:call-template name="coverage-line">
      <xsl:with-param name="name">
        <xsl:choose>
          <xsl:when test="$fullName">
            <xsl:value-of select="FullName"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="Name"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:with-param>
      <xsl:with-param name="details">
        <xsl:if test="Coverage != 100">
          <xsl:call-template name="uncovered-lines">
            <xsl:with-param name="pts" select="$methods[generate-id(.) = string($methodId)]/pt[@visit=0]" />
          </xsl:call-template>
        </xsl:if>
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>
  
  <xsl:template name="uncovered-lines">
    <xsl:param name="pts" />
    
    <xsl:text>uncovered: </xsl:text>
    <xsl:choose>
      <xsl:when test="count($pts) = 0">
        <xsl:text>entire method</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>line(s) </xsl:text>
        <xsl:call-template name="uncovered-lines-impl">
          <xsl:with-param name="pts" select="$pts" />
          <xsl:with-param name="lastStartLine" select="$pts[1]/@sl" />
          <xsl:with-param name="lastLine" select="$pts[1]/@el" />
        </xsl:call-template>
        <xsl:text> in </xsl:text>
        <xsl:variable name="filePath" select="$pts[1]/../../../File[@id = $pts[1]/@fid]/@url" />
        <xsl:choose>
          <xsl:when test="string-length($filePath) &gt; 30">
            <xsl:text>...</xsl:text>
            <xsl:value-of select="substring($filePath, string-length($filePath) - 30)"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$filePath"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="uncovered-lines-impl">
    <xsl:param name="pts" />
    <xsl:param name="lastStartLine" />
    <xsl:param name="lastLine" />

    <xsl:choose>
      <xsl:when test="count($pts) = 0">
        <xsl:call-template name="line-range">
          <xsl:with-param name="from" select="$lastStartLine" />
          <xsl:with-param name="to" select="$lastLine" />
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$pts[1]/@sl &gt; $lastLine + 1">
        <xsl:call-template name="line-range">
          <xsl:with-param name="from" select="$lastStartLine" />
          <xsl:with-param name="to" select="$lastLine" />
        </xsl:call-template>
        <xsl:text>, </xsl:text>

        <xsl:call-template name="uncovered-lines-impl">
          <xsl:with-param name="pts" select="$pts[position() &gt; 1]" />
          <xsl:with-param name="lastStartLine" select="$pts[1]/@sl" />
          <xsl:with-param name="lastLine" select="$pts[1]/@el" />
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="uncovered-lines-impl">
          <xsl:with-param name="pts" select="$pts[position() &gt; 1]" />
          <xsl:with-param name="lastStartLine" select="$lastStartLine" />
          <xsl:with-param name="lastLine" select="$pts[1]/@el" />
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="line-range">
    <xsl:param name="from" />
    <xsl:param name="to" />

    <xsl:choose>
      <xsl:when test="$from = $to">
        <xsl:value-of select="$from"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$from"/>
        <xsl:text>-</xsl:text>
        <xsl:value-of select="$to"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="html-header">
    <xsl:param name="title" />

    <head>
    <xsl:comment>
      Generated by PartCover (XSLT by Gaspar Nagy / TechTalk, http://gasparnagy.blogspot.com/2010/09/detailed-report-for-partcover-in.html)
    </xsl:comment>
    <title>
      <xsl:value-of select="$title"/>
    </title>
    <style>
      <![CDATA[
      body
      {
        font: small verdana, arial, helvetica; color:#000000;
      }
      h1
      {
        font-size: 16px;
      }
      h2
      {
        font-size: 14px;
      }
      h3
      {
        font-size: 12px;
      }
      .subtitle
      {
        font-style:italic;
        margin-top: -1.1em;
        margin-bottom: 1.0em;
      }
      div.marker
      {
        height: 1.1em;
        width: 1.1em;
        float:left;
        margin-right: 0.3em;
      }
      table.reportTable
      {
        width: 65em;
        font-size: 12px;
      }
      table.subReportTable
      {
        font-size: 12px;
        width: 63em;
        float:right;
      }
      td.subRow
      {
        padding-left: 0px;
        padding-right: 0px;
        border-right: solid 0px;
        width: 100%;
      }
      td, th
      {
        text-align: left;
        border-bottom: solid 1px #dcdcdc;
        border-right: solid 1px #dcdcdc;
        padding-left: 0.5em;
        padding-right: 0.5em;
        padding-top: 0.25em;
        padding-bottom: 0.25em;
      }
      th
      {
        background-color: #FFF2E5;
        padding-top: 0.4em;
        padding-bottom: 0.4em;
      }
      .top
      {
        border-top: solid 1px #dcdcdc;
      }
      .left
      {
        border-left: solid 1px #dcdcdc;
      }
      td.accessPath, th.accessPath
      {
        padding-left: 1.0em;
      }
      td.accessPath
      {
        font-style:italic;
      }
      .coverageDetails
      {
        padding-left: 1.0em;
        font-style:italic;
      }
      td.empty
      {
        border: none;
        height: 2.0em;
      }
      td.numeric, th.numeric
      {
        text-align: right;
      }
      td.marker
      {
        white-space: nowrap;
      }
      
      div.legend
      {
        margin-top: 2em;
        padding-left: 2em;
        font-style:italic;
        font-size: 10px;
      }
      a.button
      {
      }
      .hidden
      {
        display: none;
      }

      .coverage0
      {
        background:#FF9999;
      }
      .coverage20
      {
        background:#FFAAAA;
      }
      .coverage40
      {
        background:#FFBBBB;
      }
      .coverage60
      {
        background:#FFCCCC;
      }
      .coverage80
      {
        background:#FFEEEE;
      }
      .coverage100
      {
        background:#FFFFFF;
      }
      
      td.coverage, th.coverage
      {
        width: 9em;
      }
      tr.simplemethod td
      {
        color: gray;
      }
      
      .credits 
      {
        width: 100%;
        text-align: right;
        font-size: 8px;
        margin-top: 2em;
      }
      ]]>
    </style>
    <script>
      var showButtonText = "[show]";
      var hideButtonText = "[hide]";
      <![CDATA[
          function toggle(sdid, event){
            var link;
            if(window.event) {
              link = window.event.srcElement;
            } else {
              link = event.target;
            }

            toToggle=document.getElementById(sdid);
            if (link.innerHTML==showButtonText)
            {
              link.innerHTML=hideButtonText;
              toToggle.style.display="";
            }
            else
            {
              link.innerHTML=showButtonText;
              toToggle.style.display="none";
            }
          }

          function copyToClipboard(s)
          {
            if (window.clipboardData)
            {
              window.clipboardData.setData('Text',s);
            }
            else
            {
              try
              {
                netscape.security.PrivilegeManager.enablePrivilege('UniversalXPConnect');
              }
              catch(e)
              {
                alert("The clipboard copy didn't work.\nYour browser doesn't allow Javascript clipboard copy.\nIf you want to change its behaviour: \n1.Open a new browser window\n2.Enter the URL: about:config\n3.Change the signed.applets.codebase_principal_support property to true");
                return;
              }
              var clip = Components.classes['@mozilla.org/widget/clipboard;1'].createInstance(Components.interfaces.nsIClipboard);
              var trans = Components.classes['@mozilla.org/widget/transferable;1'].createInstance(Components.interfaces.nsITransferable);
              trans.addDataFlavor('text/unicode');
              var len = new Object();
              var str = Components.classes["@mozilla.org/supports-string;1"].createInstance(Components.interfaces.nsISupportsString);
              str.data=s;
              trans.setTransferData("text/unicode",str,s.length*2);
              var clipid=Components.interfaces.nsIClipboard;
              clip.setData(trans,null,clipid.kGlobalClipboard);
            }
          } 
          ]]>
    </script>
    </head>
  </xsl:template>
  
  <xsl:template name="html-body-header">
    <xsl:param name="title" />
    <xsl:param name="generatedAt" select="/*/@date" />
    <h1>
      <xsl:value-of select="$title"/>
    </h1>

    Generated by PartCover at <xsl:value-of select="substring($generatedAt, 1, 10)"/>
    <xsl:text> </xsl:text>
    <xsl:value-of select="substring($generatedAt, 12, 8)"/>.
  </xsl:template>
  
  <xsl:template name="html-body-footer">
  </xsl:template>
</xsl:stylesheet>
