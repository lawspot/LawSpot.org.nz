<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns="http://www.w3.org/1999/xhtml" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:include href="Layout.xslt"/>

  <xsl:template match="/*">
    <p>
      Hi there,
    </p>

    <p>
      Unfortunately LawSpot can’t answer your question.  This is either because we have more
      questions than volunteer lawyers, or because your question is outside
      LawSpot’s <a href="{BaseUrl}/terms">Terms of Use</a>.
    </p>

    <h3> We can refer you to one of our partner lawyers </h3>

    <p>
      The good news is that we have a bunch of trusted lawyers that we partner with who can answer
      your question. Here is a list of lawyers that specialise in the area of law related to
      the question you asked below.  They are friendly and more than willing to help you, so
      take your pick!
    </p>

    <table style="margin: 16px 0">
      <xsl:for-each select="ReferralPartners/ReferralPartner">
        <tr>
          <td valign="top">
            <a href="{LinkUri}">
              <img src="{LogoUri}" alt="Logo" style="float: left; margin-right: 12px"/>
            </a>
          </td>
          <td>
            <h2>
              <a href="{LinkUri}">
                <xsl:value-of select="Name"/>
              </a>
            </h2>
            <div>
              <xsl:value-of select="Description"/>
            </div>
          </td>
        </tr>
      </xsl:for-each>
    </table>

    <h3 style="clear: both"> How it works </h3>

    <p>
      If you click on one of the law firms above, you will be taken to their profile page on our
      website.  From there, you can use the lawyers’ contact details to get in touch with them.
      Our partner lawyers have can give you 20 minutes of their time for you for free.  This
      means that you can explain your legal problem to them and the lawyer can:
      <ul>
        <li>give you advice,  or</li>
        <li>help you access any further legal advice or assistance, if you need it.</li>
      </ul>
      If you need further legal advice or assistance, and it’s going to cost, you can ask the lawyer to give you an estimate of their fees.
    </p>

    <h3> Further information </h3>

    <p>
      If you have any further questions, please visit <a href="{BaseUrl}/how-it-works">this page</a>
      or email <a href="mailto:support@lawspot.org.nz">support@lawspot.org.nz</a>.
    </p>

    <p>
      Kind regards,
    </p>

    <p>
      <strong>The LawSpot Team</strong><br />
      <a href="{BaseUrl}">www.lawspot.org.nz</a><br />
      Legal Questions. Free Answers.
    </p>
  </xsl:template>
</xsl:stylesheet>