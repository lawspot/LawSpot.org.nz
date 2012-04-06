<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns="http://www.w3.org/1999/xhtml" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output method="xml" indent="yes"/>

	<xsl:template match="/">
		<html>
			<head>
				<style type="text/css">
					/* Hotmail hack */
					.ExternalClass { width: 100%; }
				</style>
			</head>
			<body>
				<table width="100%" cellpadding="0" cellspacing="0">
					<tr>
						<td align="center">
							<table width="597" cellpadding="0" cellspacing="0">
								<!-- Header -->
								<tr>
									<td height="102" background="{*/BaseUrl}/email/assets/header.jpg" bgcolor="#1982A2" valign="top">
										<!--[if gte mso 9]>
										<v:image xmlns:v="urn:schemas-microsoft-com:vml" id="theImage" style='behavior: url(#default#VML); display:inline-block; position:absolute; height:102px; width:597px; top:0; left:0; border:0; z-index:1;' src="{*/BaseUrl}/email/assets/header.jpg"/>
										<v:shape xmlns:v="urn:schemas-microsoft-com:vml" id="theText" style='behavior: url(#default#VML); display:inline-block; position:absolute; height:102px; width:597px; top:-5; left:-10; border:0; z-index:2;'>
										<div>
										<![endif]-->
											
										<!-- Content that floats over the image -->
										<table width="597" height="88" cellpadding="0" cellspacing="0" border="0">
											<tr>
                        <td align="left" style="padding: 0 20px">
                          <a href="{*/BaseUrl}"><img src="{*/BaseUrl}/email/assets/logo.png" style="border: none"/></a>
                        </td>
												<td align="right" valign="middle" style="color: white; font-size: 10pt; font-weight: bold;
														font-family: Segoe UI, Segoe, Helvetica Neue, Helvetica, Arial, sans-serif; line-height: 26pt;
														padding: 0 20px 9px">
													<xsl:value-of select="*/CurrentDate"/>
													<br />
													<a href="http://www.twitter.com/lawspot_NZ"><img src="{*/BaseUrl}/email/assets/twitter.png" style="border: none" /></a>
													&#160;
													<a href="http://www.facebook.com/LawSpot"><img src="{*/BaseUrl}/email/assets/facebook.png" style="border: none" /></a>
												</td>
											</tr>
										</table>

										<!--[if gte mso 9]>
										</div>
										</v:shape>
										<![endif]-->
									</td>
								</tr>
									
								<!-- Body -->
								<tr>
									<td align="left" style="border-left: 1px solid #A9661E; border-bottom: 1px solid #A9661E; border-right: 1px solid #A9661E;
											padding: 11px 10px; font-family: Segoe UI, Segoe, Helvetica Neue, Helvetica, Arial, sans-serif; font-size: 11pt">
										<xsl:apply-templates select="*"/>
									</td>
								</tr>
									
								<!-- Footer -->
								<tr>
									<td align="center" style="padding: 16px 0; font-family: Segoe UI, Segoe, Helvetica Neue, Helvetica, Arial, sans-serif; font-size: 10pt; color: #666">
										© Lawspot.org.nz<br />
										In association with The Wellington Law Centre
									</td>
								</tr>
							</table>
						</td>
					</tr>
				</table>
			</body>
		</html>
	</xsl:template>
</xsl:stylesheet>