<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns="http://www.w3.org/1999/xhtml" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:include href="Layout.xslt"/>

  <xsl:template match="/*">
    <p>
      Hi <xsl:value-of select="Name"/>,
    </p>

    <p>
      Thanks for submitting your answer to LawSpot. Unfortunately, your answer to the question
      "<xsl:value-of select="Question"/>" that you submitted on <xsl:value-of select="AnswerDate"/>
      has been rejected by LawSpot's supervisors, for the following reason:
    </p>

    <p style="padding-left: 20px">
      <xsl:value-of select="ReasonHtml" disable-output-escaping="yes"/>
    </p>

    <p>
      We hope that you will see this as a learning opportunity and that you will continue to
      contribute to making the law more accessible to New Zealanders by answering questions on
      LawSpot.
    </p>

    <p>
      If you have any issues with the website or just want to say hi, email us at
      <a href="mailto:volunteer@lawspot.org.nz">volunteer@lawspot.org.nz</a>.
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