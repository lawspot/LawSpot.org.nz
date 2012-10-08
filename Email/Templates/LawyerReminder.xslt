<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns="http://www.w3.org/1999/xhtml" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:include href="Layout.xslt"/>

  <xsl:template match="/*">
    <p>
      Hi <xsl:value-of select="Name"/>,
    </p>

    <p>
      <xsl:if test="UnansweredQuestionCount = 1">
        There is currently one unanswered question on LawSpot.
      </xsl:if>
      <xsl:if test="UnansweredQuestionCount != 1">
        There are currently <xsl:value-of select="UnansweredQuestionCount"/> unanswered questions on LawSpot.
      </xsl:if>
    </p>

    <ul>
      <xsl:for-each select="UnansweredQuestions/UnansweredQuestion">
        <li><a href="{Uri}"><xsl:value-of select="Title"/></a></li>
      </xsl:for-each>
    </ul>

    <p>
      If you have any issues or just want to say hi, please email us at
      <a href="mailto:volunteer@lawspot.org.nz">volunteer@lawspot.org.nz</a>. Thanks for sharing
      your skills with the New Zealand community!
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