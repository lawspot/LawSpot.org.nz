<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns="http://www.w3.org/1999/xhtml" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:include href="Layout.xslt"/>

  <xsl:template match="/*">
    <p>
      Welcome to LawSpot!
    </p>

    <p>
      Thanks for registering with Lawspot.
    </p>

    <p>
      Please follow the link below to confirm that this is your email address and to confirm that
      you read and understood the website’s <a href="{BaseUrl}/terms">Terms of Use</a>:
    </p>

    <p style="font-size: larger; text-align: center">
      <a href="{ValidateEmailUri}">Confirm your email address</a>
    </p>

    <p>
      For your own records, here are your login details. Please keep this email in a safe place.
    </p>

    <p>
      Email: <xsl:value-of select="EmailAddress"/><br />
      Password: <xsl:value-of select="Password"/>
    </p>

    <p>
      We hope you find the LawSpot website useful. If you have any issues with the website or just
      want to say hi, email us at <a href="&#109;ailto&#58;support&#64;lawspot.org.nz">
      support&#64;lawspot.org.nz</a>.
    </p>

    <p>
      Cheers,
    </p>

    <p>
      <strong>The LawSpot Team</strong>
      <a href="{BaseUrl}">www.lawspot.org.nz</a>
      Legal Questions. Free Answers.
    </p>
  </xsl:template>
</xsl:stylesheet>