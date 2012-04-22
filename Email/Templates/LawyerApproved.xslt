<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns="http://www.w3.org/1999/xhtml" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:include href="Layout.xslt"/>

  <xsl:template match="/*">
    <p>
      Hi <xsl:value-of select="Name"/>,
    </p>

    <p>
      Hooray, we’ve just confirmed your status as a lawyer and you are now registered as a lawyer
      volunteer. Welcome to the LawSpot community!
    </p>

    <p>
      Here are a few helpful links for you:
    </p>

    <ul>
      <li>
        <a href="{BaseUrl}/admin/answer-questions">Answer a question page</a>: you can post answers
        to questions posted by the New Zealand general public on this page.
      </li>
      <li>
        <a href="{BaseUrl}/admin/account-settings">Account settings</a>: on this page, you can
        change your password, and change other details about yourself (such as your work email
        address).
      </li>
      <li>
        <a href="{BaseUrl}/browse">Browse answers</a> you can browse through past answers already
        published on the website.
      </li>
    </ul>

    <p>
      We’re stoked to have you onboard as a member of the volunteering team and we look forward to
      your contributions to increase Kiwis’ accessibility to the law.
    </p>

    <p>
      If you have any questions or just want to say hi, feel free to email us at
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