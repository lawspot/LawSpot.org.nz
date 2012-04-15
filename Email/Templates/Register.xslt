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

    <xsl:if test="AskedQuestion = 'True'">
      <p>
        Once you have followed the link, your question will be screened and categorised by one of our
        LawSpot administrators. LawSpot reserves the right to refuse to answer a question if the
        LawSpot administrator considers that the question:
      </p>

      <ul>
        <li> has already been asked and answered on the Website; </li>
        <li> contains objectionable or offensive material; </li>
        <li> is too complex or specific in nature; or </li>
        <li> contains information that would place the user at risk of being personally identified; </li>
        <li> or for any other reason. </li>
      </ul>

      <p>
        LawSpot also reserves the right to modify (through its administrators) any question that
        might otherwise be rejected for any of the above reasons so that it can be made available
        for lawyers to answer.
      </p>
    </xsl:if>

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
      <strong>The LawSpot Team</strong><br />
      <a href="{BaseUrl}">www.lawspot.org.nz</a><br />
      Legal Questions. Free Answers.
    </p>
  </xsl:template>
</xsl:stylesheet>