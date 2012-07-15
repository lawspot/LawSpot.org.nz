<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns="http://www.w3.org/1999/xhtml" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:include href="Layout.xslt"/>

  <xsl:template match="/*">
    <p>
      Hi <xsl:value-of select="Name"/>,
    </p>

    <p>
      There are currently [number of questions] unanswered questions on LawSpot.
      Below are the latest 20 (or however many) questions [show these questions by
      the lawyer’s specialization first then show by date posted] submitted by users.
    </p>

    <p>
      <xsl:value-of select="Reason"/>
    </p>

    <p>
      If you have any issues with this, please email us at
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