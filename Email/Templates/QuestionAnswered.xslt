﻿<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns="http://www.w3.org/1999/xhtml" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:include href="Layout.xslt"/>

  <xsl:template match="/*">
    <p>
      Hi there,
    </p>

    <p>
      Hooray! An answer to your question "<xsl:value-of select="Question"/>" has been posted on
      LawSpot. You’ll see the answer below or view it on our website.
    </p>

    <p>
      <xsl:value-of select="Answer"/>
    </p>

    <p>
      IMPORTANT NOTICE:<br/>
      The answer provided above is intended for general informational purposes only and cannot be
      considered a substitute for face-to-face. It should not be relied upon as the sole basis for
      taking action in relation to a legal issue. Laws change frequently, and small variations in
      the facts, or a fact not provided in the question, can often change a legal outcome or a
      lawyer’s conclusion. No liability whatsoever is accepted by the authors or publishers of the
      answer, for loss, damage or inconvenience arising in any way from the use of this site.
      Although LawSpot has verified that each lawyer volunteer holds a current practising
      certificate, he or she may not necessarily have experience in the particular area of law
      involved.
    </p>

    <p>
      For more information about this website, please review our <a href="{BaseUrl}/terms">Terms of Use</a>.
    </p>

    <p>
      We hope you find this answer to be useful. If you have more questions about New Zealand law,
      we encourage you to post additional questions using the <a href="{BaseUrl}/ask">Ask a Lawyer</a>
      section of LawSpot.
    </p>

    <p>
      If you have any issues with the website or just want to say hi, email us at
      <a href="mailto:support@lawspot.org.nz">support@lawspot.org.nz</a>.
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