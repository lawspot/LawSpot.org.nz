<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <!-- Email content -->
  <xsl:template match="*">

    <html>
      <body bgcolor="white" style="font-family: Helvetica, Arial, Sans-serif; font-size: 10pt">

        <h1 style="font-size: 16pt">
          <xsl:value-of select="ErrorMessageHtml" disable-output-escaping="yes" />
        </h1>

        <div style="white-space: pre; font-family: Helvetica, Arial, Sans-serif; font-size: 10pt; margin: 2em 0; line-height: 1em">
          <xsl:value-of select="StackTrace"/>
        </div>

        <xsl:if test="count(ExceptionData/NameValuePair) &gt; 0">
          <div style="font-weight: bold; font-size: larger; text-align: left; border-top: 1px solid darkgray; margin: 8px 0; padding: 8px 0">
            Exception Data
          </div>

          <table cellpadding="2" cellspacing="0" style="width: 100%; font-family: Helvetica, Arial, Sans-serif; font-size: 10pt">
            <xsl:for-each select="ExceptionData/NameValuePair">
              <tr>
                <th align="right" valign="top" width="120" style="padding-right: 8px">
                  <xsl:value-of select="Name"/>
                </th>
                <td>
                  <xsl:value-of select="Value"/>
                </td>
              </tr>
            </xsl:for-each>
          </table>
        </xsl:if>

        <table cellpadding="4" cellspacing="0" style="width: 100%; font-family: Helvetica, Arial, Sans-serif; font-size: 10pt">
          <tr>
            <th colspan="4" style="font-size: larger; text-align: left; border-top: 1px solid darkgray; padding: 16px 0">
              Request Details
            </th>
          </tr>
          <tr>
            <th align="right" width="120"> Request Time: </th>
            <td>
              <xsl:value-of select="RequestTime"/>
            </td>
            <th align="right" width="120"> Current Time: </th>
            <td>
              <xsl:value-of select="CurrentTime"/>
            </td>
          </tr>
          <tr>
            <th align="right" width="120"> URL: </th>
            <td colspan="3">
              <xsl:value-of select="RequestURL"/>
            </td>
          </tr>
          <tr>
            <xsl:if test="string-length(RequestType) = 0">
              <xsl:attribute name="style">
                padding-bottom: 16px
              </xsl:attribute>
            </xsl:if>
            <th align="right" width="120"> Referrer: </th>
            <td colspan="3">
              <xsl:value-of select="RequestReferrer"/>
            </td>
          </tr>
          <xsl:if test="string-length(RequestType) &gt; 0">
            <tr>
              <th align="right" width="120"> Method: </th>
              <td>
                <xsl:value-of select="RequestType"/>
              </td>
              <th align="right" width="120"> Content Length: </th>
              <td>
                <xsl:value-of select="RequestLength"/>
              </td>
            </tr>
            <tr style="padding-bottom: 16px">
              <th align="right" width="120"> Content Type: </th>
              <td>
                <xsl:value-of select="ContentType"/>
              </td>
              <th align="right" width="120"> Content Encoding: </th>
              <td>
                <xsl:value-of select="ContentEncoding"/>
              </td>
            </tr>
          </xsl:if>
          
          <tr>
            <th colspan="4" style="font-size: larger; text-align: left; border-top: 1px solid darkgray; padding: 16px 0">
              User Details
            </th>
          </tr>
          <tr>
            <th align="right" width="120"> IP Address: </th>
            <td>
              <xsl:value-of select="UserIP"/>
            </td>
            <th align="right" width="120"> Username: </th>
            <td>
              <xsl:value-of select="UserName"/>
            </td>
          </tr>
          <tr>
            <th align="right" width="120"> Languages: </th>
            <td>
              <xsl:value-of select="UserLanguages"/>
            </td>
            <th align="right" width="120"> True Identity: </th>
            <td>
              <xsl:value-of select="TrueIdentity"/>
            </td>
          </tr>
          <tr>
            <th align="right" width="120"> Browser: </th>
            <td colspan="3">
              <xsl:value-of select="UserAgent"/>
            </td>
          </tr>
          <tr style="padding-bottom: 16px">
            <th align="right" width="120"> Client App: </th>
            <td>
              <xsl:value-of select="ClientApplication"/>
            </td>
            <th align="right" width="120"> </th>
            <td>
            </td>
          </tr>

          <tr>
            <th colspan="4" style="font-size: larger; text-align: left; border-top: 1px solid darkgray; padding: 16px 0">
              Server Details
            </th>
          </tr>
          <tr>
            <th align="right" width="120"> Application: </th>
            <td>
              <xsl:value-of select="Application"/>
            </td>
            <th align="right" width="120"> Server Name: </th>
            <td>
              <xsl:value-of select="ServerHost"/>
            </td>
          </tr>
          <tr>
            <th align="right" width="120"> Command Line: </th>
            <td colspan="3">
              <xsl:value-of select="ServerCommandLine"/>
            </td>
          </tr>
        </table>

        <xsl:if test="count(RequestCookies/NameValuePair) &gt; 0">
          <div style="font-weight: bold; font-size: larger; text-align: left; border-top: 1px solid darkgray; margin: 8px 0; padding: 8px 0">
            Cookies
          </div>

          <table cellpadding="2" cellspacing="0" style="width: 100%; font-family: Helvetica, Arial, Sans-serif; font-size: 10pt">
            <xsl:for-each select="RequestCookies/NameValuePair">
              <tr>
                <th align="right" valign="top" width="120" style="padding-right: 8px">
                  <xsl:value-of select="Name"/>
                </th>
                <td>
                  <xsl:value-of select="Value"/>
                </td>
              </tr>
            </xsl:for-each>
          </table>
        </xsl:if>

        <xsl:if test="count(RequestFormData/NameValuePair) &gt; 0">
          <div style="font-weight: bold; font-size: larger; text-align: left; border-top: 1px solid darkgray; margin: 8px 0; padding: 8px 0">
            Form Data
          </div>

          <table cellpadding="2" cellspacing="0" style="width: 100%; font-family: Helvetica, Arial, Sans-serif; font-size: 10pt">
            <xsl:for-each select="RequestFormData/NameValuePair">
              <tr>
                <th align="right" valign="top" width="120" style="padding-right: 8px">
                  <xsl:value-of select="Name"/>
                </th>
                <td>
                  <xsl:value-of select="Value"/>
                </td>
              </tr>
            </xsl:for-each>
          </table>
        </xsl:if>

        <xsl:if test="count(RequestFormData) = 0 and string-length(RequestData) &gt; 0">
          <div style="font-weight: bold; font-size: larger; text-align: left; border-top: 1px solid darkgray; margin: 8px 0; padding: 8px 0">
            Request Data
          </div>

          <p>
            <xsl:value-of select="RequestData"/>
          </p>
        </xsl:if>

      </body>
    </html>

  </xsl:template>

</xsl:stylesheet>
