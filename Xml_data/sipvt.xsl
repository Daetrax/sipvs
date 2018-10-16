<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:egonp="https://github.com/Daetrax/sipvs/blob/master/Xml_data/">
<xsl:output method="html" indent="yes"/>

<xsl:template match="Request">
  <html>
  <body>
    <h2>Book loan request form</h2>
	<div style="float:left;">
    <div>Name:  <xsl:value-of select="Name"/></div>
    <div>Surname:  <xsl:value-of select="Surname"/></div>
	<div>Country:  <xsl:value-of select="Country"/></div>
	<div>City:  <xsl:value-of select="City"/> </div>
    <div>Street:  <xsl:value-of select="Street"/>  <xsl:value-of select="StreetNumber"/></div> 
	<div>Zip: <xsl:value-of select="Zip"/></div>
    <div>Request Date:  <xsl:value-of select="RequestDate"/></div>
    <div>Loan Period:  <xsl:value-of select="LoanPeriod"/></div>
	</div>	
    <table style="float:left;" border="1">
      <tr bgcolor="#F0F8FF">
        <th>List of books to loan</th>
      </tr>
     <xsl:for-each select="BookList/Book">
        <tr>
          <td><xsl:value-of select="BookName"/></td>
		  <td><xsl:value-of select="attribute::lang"/></td>
        </tr>
      </xsl:for-each>
    </table>
  </body>
  </html>
</xsl:template>

</xsl:stylesheet> 
