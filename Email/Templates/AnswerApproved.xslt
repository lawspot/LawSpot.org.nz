<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns="http://www.w3.org/1999/xhtml" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:include href="Layout.xslt"/>

  <xsl:template match="/*">
    <p>
      Hi <xsl:value-of select="Name"/>,
    </p>

    <p>
      We’ve just reviewed the answer you posted for the question "<xsl:value-of select="Question"/>"
      and the answer below has been published to the LawSpot website (you can view it online
      <a href="{QuestionUri}">here</a>).
    </p>

    <p style="padding-left: 20px">
      <xsl:value-of select="AnswerHtml" disable-output-escaping="yes"/>
    </p>

    <p>
      There are <xsl:value-of select="UnansweredQuestionCount"/> unanswered questions on LawSpot,
      click <a href="{BaseUrl}/admin/answer-questions">here</a> to answer more questions.
    </p>

    <p>
      If you have any issues or just want to say hi, please email us at
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