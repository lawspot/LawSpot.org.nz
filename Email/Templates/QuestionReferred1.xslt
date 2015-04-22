<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns="http://www.w3.org/1999/xhtml" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:include href="Layout.xslt"/>

  <xsl:template match="/*">
    <p>
      Hi there,
    </p>

    <p>
      Unfortunately LawSpot can’t answer your question directly this time. This is either because
      we have more questions than volunteer lawyers, or because your question is outside LawSpot’s
      <a href="{BaseUrl}/terms">terms of use</a>.
    </p>

    <p>
      <strong>We can refer your question to a partner lawyer</strong>
    </p>

    <p>
      The good news is that we have a bunch of lawyers and law firms that we partner with who can
      answer your question. If you’d like us to refer your question to our partners, please click
      the big link below.
    </p>

    <p>
      <strong>How it works</strong>
    </p>

    <p>
      You will need to provide us with your name, your phone number, and the name of the other
      party you are asking a question about (if relevant). We need this information so we can
      check that our partners don’t have a conflict of interest before we give them the details of
      your legal question.
    </p>

    <p>
      From here, your question will go into a queue for our partners to pick up.
    </p>

    <p>
      If a partner picks up your question within three days, you will get another email from us to
      let you know that partner law firm’s name and contact details. If a partner does not pick up
      your question within three days, you will receive another email from us apologising and
      referring you to other legal resources.
    </p>

    <p>
      <a href="{BaseUrl}/collect-referral-details?questionId={QuestionId}" style="font-size: xx-large">Request a referral</a><br/>
      (You may need to log in).
    </p>

    <p>
      <strong>Further information</strong>
    </p>

    <p>
      If you have any further questions, please visit the <a href="{BaseUrl}/how-it-works">how it
      works page</a> or email <a href="mailto:support@lawspot.org.nz">support@lawspot.org.nz</a>.
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