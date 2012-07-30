<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns="http://www.w3.org/1999/xhtml" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:include href="Layout.xslt"/>

  <xsl:template match="/*">
    <p>
      Welcome to LawSpot!
    </p>

    <p>
      Thanks for registering your interest to volunteer as a lawyer with LawSpot – we’re stoked
      that you’ve decided to help out the New Zealand community!
    </p>

    <p>
      Please confirm your email address (make sure this is a work email address) by clicking on
      the link below:
    </p>

    <p style="font-size: larger; text-align: center">
      <a href="{ValidateEmailUri}">Confirm your email address</a>
    </p>

    <p>
      Once you’ve confirmed your email address, please give us a little time to verify that your
      status as a lawyer – we’ll endeavour to get back to you within the next 48 hours, but it may
      take longer on some weeks when we might be particularly busy.
    </p>

    <p>
      Please note that LawSpot is currently being piloted in the Wellington region. This means that
      for the moment only lawyers based in Wellington will be able to submit answers to questions,
      and only under the supervision of Community Law Wellington & Hutt Valley. We'll be sure to let
      you know over the next few weeks when lawyers from other regions can start submitting
      answers.  You may still be able to help with the pilot even if you're not based in Wellington
      - to find out more, please email us at <a href="&#109;ailto&#58;volunteer&#64;lawspot.org.nz">
      volunteer&#64;lawspot.org.nz</a>.
    </p>

    <p>
      For your own records, here are your login details. Please keep this email in a safe place.
    </p>

    <p>
      Email: <xsl:value-of select="EmailAddress"/><br />
      Password: <xsl:value-of select="Password"/>
    </p>
    
    <p>
      In the meantime, have a browse around the site. If you have any questions, please do not
      hesitate to contact us at <a href="&#109;ailto&#58;support&#64;lawspot.org.nz">
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