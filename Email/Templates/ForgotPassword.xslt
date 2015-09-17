<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns="http://www.w3.org/1999/xhtml" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:include href="Layout.xslt"/>

  <xsl:template match="/*">
    <p>
      Hi there,
    </p>
    
    <p>
      We are sending you this email because you have indicated that you have forgotten your LawSpot
      password.  Click the link below to reset your password now:
    </p>

    <p style="font-size: larger; text-align: center">
      <a href="{ResetPasswordUri}">Reset Your Password</a>
    </p>

    <p>
      Please note that this link is only valid for one hour.
    </p>
    
    <p>
      If you did not request a password reset, then you can ignore this email.  If you get multiple
      unsolicited copies of this email, then someone may be abusing our password reset function -
      please contact us at
      <a href="&#109;ailto&#58;support&#64;lawspot.org.nz">support&#64;lawspot.org.nz</a>.
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