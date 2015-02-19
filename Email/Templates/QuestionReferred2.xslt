<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns="http://www.w3.org/1999/xhtml" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:include href="Layout.xslt"/>

  <xsl:template match="/*">
    <p>
      Hi there,
    </p>

    <p>
      One of our partner lawyers has picked up your question! You can expect to hear from
      <xsl:value-of select="LawFirm"/> within the next few business days. If you do not hear from
      <xsl:value-of select="LawFirm"/> within this timeframe, please email us at
      <a href="mailto:support@lawspot.org.nz">support@lawspot.org.nz</a> to let us know.
    </p>

    <p>
      <xsl:value-of select="LawFirm"/> will be providing you with 20 minutes of free legal assistance.
      This should give you enough time to explain your situation to <xsl:value-of select="LawFirm"/> and
      receive some basic legal assistance from <xsl:value-of select="LawFirm"/>.
    </p>

    <p>
      If you still need help after speaking with <xsl:value-of select="LawFirm"/>, then
      <xsl:value-of select="LawFirm"/> can either continue to assist you under their standard client
      terms of engagement (which they will describe to you) or direct you to other relevant legal
      services.
    </p>

    <p>
      Thanks for seeking an answer to your legal question through LawSpot. We hope our service has been
      useful for you. You can provide us with feedback by emailing
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