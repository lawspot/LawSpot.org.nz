<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns="http://www.w3.org/1999/xhtml" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:include href="Layout.xslt"/>

  <xsl:template match="/*">
    <p>
      Welcome to LawSpot!
    </p>

    <p>
      Thanks for registering.
    </p>

    <p>
      Here are your login details. Please keep this email in a safe place.
    </p>

    <p>
      Email: <xsl:value-of select="EmailAddress"/><br />
      Password: <xsl:value-of select="Password"/>
    </p>

    <p>
      Before you can use this service, you must confirm that you understand and accept the
      website’s <a href="{BaseUrl}/terms">Terms of Use</a> and in particular you accept that:
    </p>

    <ul>
      <li>
        LawSpot’s lawyer volunteers may also respond to questions submitted by someone who may be
        the other party to your legal issue;
      </li>
      <li>
        the LawSpot lawyer who answers your question may already act for the other party to your
        legal issue in the course of his or her regular employment, or in the course of volunteering
        at a community law centre; and
      </li>
      <li>
        each LawSpot lawyer is only obliged to tell you information about the relevant area of
        law as at the time they answer your question, and to tell you if they are aware of other
        questions received or answers published by LawSpot that are relevant to your question.
      </li>
      <li>
        You are aware that by agreeing to these terms, a LawSpot lawyer may, in the course of
        volunteering for LawSpot, end up acting in a manner that could have negative implications
        for your own legal position. However, you are aware that there are alternatives to
        consenting to these terms. For example, you could seek face-to-face advice at a local
        Community Law Centre, or retain a lawyer to advise you. Further, none of these terms affect
        the obligation that applies to all lawyers, to exercise independent judgment when answering
        questions and to give objective advice based on the lawyer’s understanding of the law.
      </li>
    </ul>

    <xsl:if test="AskedQuestion = 'True'">
      <p>
        Your question will now will be screened and categorised by one of our LawSpot administrators.
        LawSpot reserves the right to refuse to answer a question if the LawSpot administrator
        considers that the question:
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