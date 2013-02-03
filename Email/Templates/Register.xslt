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

    <xsl:if test="AskedQuestion = 'True'">
      <h3> What's next? </h3>
      
      <p>
        Your question will now will be screened and categorised by one of our LawSpot administrators.
        LawSpot reserves the right to refuse to answer a question if the LawSpot administrator
        considers that the question:
      </p>

      <ul>
        <li> is about an area of law that LawSpot doesn't answer (<a href="{BaseUrl}/terms">find out more</a>); </li>
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
      For further legal assistance, try the
      <a href="http://www.communitylaw.org.nz/community-law-manual/">Community Law Manual</a>, visit
      <a href="http://www.communitylaw.org.nz/your-local-centre/find-a-community-law-centre/">your local community law centre</a>
      or <a href="http://www.nzls.org.nz/RegistrationDB/faces/presentation/general/FindLawyerOrOrganisation.jsp">engage a lawyer</a>.
    </p>

    <h3> Terms of use </h3>

    <p>
      By using this service, you understand and agree to the
      website’s <a href="{BaseUrl}/terms">Terms of Use</a> and in particular you accept that:
    </p>

    <ul>
      <li>
        LawSpot’s lawyer volunteers may also respond to questions submitted by someone who may be
        the other party to your legal issue;
      </li>
      <li>
        The LawSpot lawyer who answers your question may already act for the other party to your
        legal issue in the course of his or her regular employment, or in the course of volunteering
        at a community law centre; and
      </li>
      <li>
        Each LawSpot lawyer is only obliged to tell you information about the relevant area of
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