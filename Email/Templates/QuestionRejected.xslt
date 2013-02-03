<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns="http://www.w3.org/1999/xhtml" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:include href="Layout.xslt"/>

  <xsl:template match="/*">
    <p>
      Hi there,
    </p>

    <p>
      Thanks for submitting your question to LawSpot. Unfortunately, your question
      "<xsl:value-of select="Question"/>" submitted on <xsl:value-of select="QuestionDate"/> cannot
      be answered by the LawSpot service and has not been passed onto a lawyer. Your question was
      rejected for the following reason:
    </p>

    <p style="padding-left: 20px">
      <xsl:value-of select="ReasonHtml" disable-output-escaping="yes"/>
    </p>

    <p>
      We’re sorry that we’re unable to help you in this case, but we hope you continue to use the
      LawSpot service to answer other questions you may have about New Zealand law.
    </p>

    <p>
      Here are some other legal resources you may want to try:
    </p>
    <ul>
      <li> <a href="http://www.communitylaw.org.nz/community-law-manual/">the Community Law Manual</a> </li>
      <li> <a href="http://www.communitylaw.org.nz/your-local-centre/find-a-community-law-centre/">your local community law centre</a>, or </li>
      <li> <a href="http://www.nzls.org.nz/RegistrationDB/faces/presentation/general/FindLawyerOrOrganisation.jsp">the Law Society register</a> (for finding a lawyer). </li>
    </ul>

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