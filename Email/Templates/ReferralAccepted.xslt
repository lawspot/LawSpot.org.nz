<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns="http://www.w3.org/1999/xhtml" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:include href="Layout.xslt"/>

  <xsl:template match="/*">
    <p>
      Hi <xsl:value-of select="Name"/>,
    </p>

    <p>
      Thanks for accepting a referral from LawSpot! Below are the details you need to contact the client.
    </p>

    <table>
      <tr>
        <td><b>Name:</b></td>
        <td><xsl:value-of select="ClientName"/></td>
      </tr>
      <tr>
        <td><b>Email address:</b></td>
        <td><xsl:value-of select="ClientEmail"/></td>
      </tr>
      <tr>
        <td><b>Phone number:</b></td>
        <td><xsl:value-of select="ClientPhone"/></td>
      </tr>
      <tr>
        <td><b>Location:</b></td>
        <td><xsl:value-of select="ClientLocation"/></td>
      </tr>
      <tr>
        <td><b>Question:</b></td>
        <td><xsl:value-of select="Question"/></td>
      </tr>
      <tr>
        <td colspan="2">
          <b>Question details:</b><br/>
          <div style="padding-left: 20px">
            <xsl:value-of select="DetailsHtml" disable-output-escaping="yes"/>
          </div>
        </td>
      </tr>
    </table>

    <p>
      Now that you have accepted this referral, please handle the client as you would any other
      client that had approached you for legal advice. The only conditions we impose on this
      referral are that you:
      <ul>
        <li>contact the client within three business days of receiving this email, and</li>
        <li>provide the client with 20 minutes of pro bono time.</li>
      </ul>
      If you have any questions, or if you have received this email or taken this referral in
      error, please contact us at <a href="mailto:support@lawspot.org.nz">support@lawspot.org.nz</a>.
      We hope you have found this referral service useful! You can also provide us with feedback to
      <a href="mailto:ceo@lawspot.org.nz">ceo@lawspot.org.nz</a>. Your feedback will be treated
      confidentially.
    </p>

    <p>
      Cheers,
    </p>

    <p>
      <strong>The LawSpot Team</strong><br />
      <a href="{BaseUrl}">www.lawspot.org.nz</a><br />
      Legal Questions. Free Answers.
    </p>
  </xsl:template>
</xsl:stylesheet>