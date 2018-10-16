<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:t="https://github.com/Daetrax/sipvs/blob/master/Xml_data/sipvt">
<xsl:output method="html" indent="yes" omit-xml-declaration="yes"/>

<xsl:template match="/t:Request">
  <html>
  <body>
	
    <h2>Book loan request form</h2>
	<div style="float:left;">
	
    <div>Name:  <xsl:value-of select="./t:Name"/></div>
    <div>Surname:  <xsl:value-of select="./t:Surname"/></div>
	<div>Country:  <xsl:value-of select="./t:Country"/></div>
	<div>City:  <xsl:value-of select="./t:City"/> </div>
    <div>Street:  <xsl:value-of select="./t:Street"/>  <xsl:value-of select="./t:StreetNumber"/></div> 
	<div>Zip: <xsl:value-of select="./t:Zip"/></div>
    <div>Request Date:  <xsl:value-of select="./t:RequestDate"/></div>
    <div>Loan Period:  <xsl:value-of select="./t:LoanPeriod"/></div>
	</div>	
    <table style="float:left;" border="1">
      <tr bgcolor="#F0F8FF">
        <th>List of books to loan</th>
      </tr>
	 
     <xsl:for-each select="./t:BookList/t:Book">
        <tr>
          <td><xsl:value-of select="./t:BookName"/></td>
		  <td><xsl:value-of select="./attribute::lang"/></td>
        </tr>
      </xsl:for-each>
	  
    </table>
  </body>
  </html>
</xsl:template>

<xsl:template match="BookList">

<xsl:for-each select="Book">
        <tr>
          <td><xsl:value-of select="BookName"/></td>
		  <td><xsl:value-of select="attribute::lang"/></td>
        </tr>
      </xsl:for-each>

</xsl:template>


</xsl:stylesheet> 