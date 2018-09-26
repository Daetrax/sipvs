<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<xsl:output method="html" indent="yes"/>

<xsl:template match="/">
  <html>
  <body>
    <h2>Book loan request form</h2>

    <div>Name <xsl:value-of select="Name"/></div>
    <div>Surname <xsl:value-of select="Surname"/></div>
    <div>Street <xsl:value-of select="Street"/> No. <xsl:value-of select="StreetNumber"/></div>
    <div>City <xsl:value-of select="City"/> Zip <xsl:value-of select="Cip"/></div>
    <div>Country <xsl:value-of select="Country"/></div>
    <div>Request Date <xsl:value-of select="RequestDate"/></div>
    <div>Loan Period <xsl:value-of select="LoanPeriod"/></div>

    <table border="1">
      <tr bgcolor="#9acd32">
        <th>List of books to loan</th>
      </tr>
      <xsl:for-each select="Request/BookList">
        <tr>
          <td><xsl:value-of select="BookName"/></td>
        </tr>
      </xsl:for-each>
    </table>
  </body>
  </html>
</xsl:template>

</xsl:stylesheet> 