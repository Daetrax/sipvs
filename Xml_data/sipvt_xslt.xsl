 <?xml version="1.0"?>

<xsl:stylesheet version="1.0"
xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

<xsl:template match="/">
  <html>
  <body>
    <h2>Book loan request form</h2>

    <div>Name <xsl:value-of select="name"/><div>
    <div>Surname <xsl:value-of select="surname"/><div>
    <div>Street <xsl:value-of select="street"/> No. <xsl:value-of select="streetNumber"/><div>
    <div>City <xsl:value-of select="city"/> Zip <xsl:value-of select="zip"/><div>
    <div>Country <xsl:value-of select="country"/><div>
    <div>Request Date <xsl:value-of select="requestDate"/><div>
    <div>Loan Period <xsl:value-of select="loanPeriod"/><div>

    <table border="1">
      <tr bgcolor="#9acd32">
        <th>List of books to loan</th>
      </tr>
      <xsl:for-each select="request/bookList">
        <tr>
          <td><xsl:value-of select="bookName"/></td>
        </tr>
      </xsl:for-each>
    </table>
  </body>
  </html>
</xsl:template>

</xsl:stylesheet> 