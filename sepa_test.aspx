<%@ Page Language="VB"  %>

<%
    '' This file is an example on how to use the SEPA generator class
    '================================================================
    Dim oSEPA As New cSepaXML
    Dim oDataTable As New DataTable
    Dim newRow As DataRow


    '' To have a working example the below data is example data.
    '' This information can also retrived from a database
    '' or other source. The class works with a DataTable.
    ''==========================================================

    '' Headers for the example data
    oDataTable.Columns.Add("cps_iban", System.Type.GetType("System.String"))
    oDataTable.Columns.Add("cps_bic", System.Type.GetType("System.String"))
    oDataTable.Columns.Add("cps_name", System.Type.GetType("System.String"))
    oDataTable.Columns.Add("cps_status", System.Type.GetType("System.Boolean"))
    oDataTable.Columns.Add("cps_seqtp", System.Type.GetType("System.String"))
    oDataTable.Columns.Add("invoice_number", System.Type.GetType("System.String"))
    oDataTable.Columns.Add("invoice_total", System.Type.GetType("System.Double"))
    oDataTable.Columns.Add("customer_end_to_end_id", System.Type.GetType("System.String"))
    oDataTable.Columns.Add("customer_id", System.Type.GetType("System.String"))
    oDataTable.Columns.Add("customer_street", System.Type.GetType("System.String"))
    oDataTable.Columns.Add("customer_zip", System.Type.GetType("System.String"))
    oDataTable.Columns.Add("customer_city", System.Type.GetType("System.String"))

    '' Example data row 1
    newRow = Nothing
    newRow = oDataTable.NewRow
    newRow("cps_iban") = "iban-number"
    newRow("cps_bic") = "bic-code"
    newRow("cps_name") = "Customer Name A"
    newRow("cps_status") = True
    newRow("cps_seqtp") = "FRST"  '' or RCUR
    newRow("invoice_number") = "Invoice.1234"
    newRow("invoice_total") = 10.12
    newRow("customer_end_to_end_id") = "cust.123.456"
    newRow("customer_id") = "4001"
    newRow("customer_street") = "Street 1"
    newRow("customer_zip") = "1234AB"
    newRow("customer_city") = "Amsterdam"
    oDataTable.Rows.Add(newRow)

    '' Example data row 2
    newRow = Nothing
    newRow = oDataTable.NewRow
    newRow("cps_iban") = "iban-number"
    newRow("cps_bic") = "bic-code"
    newRow("cps_name") = "Customer Name B"
    newRow("cps_status") = True
    newRow("cps_seqtp") = "FRST"  '' or RCUR
    newRow("invoice_number") = "Invoice.2345"
    newRow("invoice_total") = 23.12
    newRow("customer_end_to_end_id") = "cust.456.789"
    newRow("customer_id") = "4002"
    newRow("customer_street") = "Street 3"
    newRow("customer_zip") = "1234AB"
    newRow("customer_city") = "Amsterdam"
    oDataTable.Rows.Add(newRow)
    ''=================================================
    '' end example data


    '' Below setup YOUR details. 
    '' This details you have recieved from your bank and can be found on your SEPA contract
    oSEPA.pss_bic = "YOUR_BIC_CODE"
    oSEPA.pss_iban = "YOUR_IBAN"
    oSEPA.pss_fixeddesc = "Your fixed description"
    oSEPA.pss_mandaatid = "Your_mandateID"
    oSEPA.pss_mandaatdate = "2018-01-01"
    oSEPA.pss_incassantid = "YOURID_123345456"

    '' add the datatable object (use the data example above, or add your own data from database)
    oSEPA.customer_data = oDataTable
    Dim sXml As String = oSEPA.GenerateSepaXML()

    Response.ContentType = "application/xml"    '' setup the client header
    Response.Write(sXml)
    %>