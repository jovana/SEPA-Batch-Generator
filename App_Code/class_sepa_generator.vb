
'*******************************************************************************
' MODULE:		Classes
' FILENAME:		class_sepa_generator.vb
' AUTHOR:		Jovana
' CREATED:		06-08-2013
'
' DESCRIPTION:
' 	Generate SEPA XML batch file for direct debit
'
'************************
Imports System.Xml

Public Class cSepaXML

    '' Public values
    Public pss_contractname As String
    Public pss_bic As String
    Public pss_iban As String
    Public pss_fixeddesc As String
    Public pss_id As Integer
    Public pss_mandaatid As String
    Public pss_mandaatdate As String
    Public pss_incassantid As String
    Public customer_data As DataTable

    '' private values
    Private SumTransActions As Integer
    Private SumAmount As Decimal

    ''' <summary>
    ''' This function creatte the XML file
    ''' </summary>
    ''' <returns></returns>
    Public Function GenerateSepaXML() As String

        Dim iCountTransactions As Integer
        Dim msgID As String
        Dim PmtInf, child1, rootEl, painEl As XmlElement
        Dim PmtTpInf, child1_1, child1_2 As XmlElement
        Dim iTotaalTransaction As Decimal
        Dim xmlDoc As New XmlDocument

        xmlDoc.PreserveWhitespace = True

        '' Create batch ID's, this needs a unique ID each time a batch has generated
        msgID = cCrypto.sha1(System.DateTime.UtcNow)

        '' Create the root elements
        '--------------------------
        rootEl = xmlDoc.CreateElement("Document")
        rootEl.SetAttribute("xmlns", "urn:iso:std:iso:20022:tech:xsd:pain.008.001.02")
        rootEl.SetAttribute("xmlns:xsi", "www.w3.org/2001/XMLSchema-instance")
        xmlDoc.AppendChild(rootEl)

        '' Create the pain element
        '-------------------------
        painEl = xmlDoc.CreateElement("CstmrDrctDbtInitn")
        rootEl.AppendChild(painEl)

        '' First loop cotains the FRST customers (payers)
        '------------------------------------------------

        '' Control / check items needed for later use
        iTotaalTransaction = 0
        iCountTransactions = 0

        '' set the PmtInf xml element 
        '----------------------------
        PmtInf = xmlDoc.CreateElement("PmtInf")
        painEl.AppendChild(PmtInf)

        child1 = xmlDoc.CreateElement("PmtInfId")       '' <-- Unique id (Using the same MsgId)
        child1.InnerText = msgID & "FRST"
        PmtInf.AppendChild(child1)

        child1 = xmlDoc.CreateElement("PmtMtd")         '' <-- PaymentMethod (always DD = DirectDebet)
        child1.InnerText = "DD"
        PmtInf.AppendChild(child1)

        '' Adding thePmtTpInf xml element
        '--------------------------------
        PmtTpInf = xmlDoc.CreateElement("PmtTpInf")
        PmtInf.AppendChild(PmtTpInf)

        child1_1 = xmlDoc.CreateElement("SvcLvl")       '' <-- ServiceLevel (Not implemented jet) (verplicht bij rabobank)
        PmtTpInf.AppendChild(child1_1)

        child1_2 = xmlDoc.CreateElement("Cd")
        child1_2.InnerText = "SEPA"
        child1_1.AppendChild(child1_2)

        child1_1 = xmlDoc.CreateElement("LclInstrm")    '' <-- LocalInstrument (Not implemented jet)
        PmtTpInf.AppendChild(child1_1)

        child1_2 = xmlDoc.CreateElement("Cd")
        child1_2.InnerText = "CORE"                         '' <-- Using CORE or B2B
        child1_1.AppendChild(child1_2)

        child1_1 = xmlDoc.CreateElement("SeqTp")            '' <-- SequenceType Allowed codes: FRST, RCUR, OOFF, FNAL
        child1_1.InnerText = "FRST"
        PmtTpInf.AppendChild(child1_1)

        child1 = xmlDoc.CreateElement("ReqdColltnDt")       '' <-- date and time for the moment the collection need to be executed
        child1.InnerText = FormatSEPADate(cFunctions.GetLocalDateTimeFromUTC(System.DateTime.UtcNow))
        PmtInf.AppendChild(child1)

        child1 = xmlDoc.CreateElement("Cdtr")               '' <-- verplicht bij ABN en Rabo (niet volgens de standaard)
        PmtInf.AppendChild(child1)

        child1_1 = xmlDoc.CreateElement("Nm")               '' <-- Name payer
        child1_1.InnerText = pss_contractname
        child1.AppendChild(child1_1)

        child1 = xmlDoc.CreateElement("CdtrAcct")
        PmtInf.AppendChild(child1)

        child1_1 = xmlDoc.CreateElement("Id")
        child1.AppendChild(child1_1)

        child1_2 = xmlDoc.CreateElement("IBAN")             '' <-- IBAN number payer
        child1_2.InnerText = UCase(pss_iban)                '' <-- IBAN number always upercase
        child1_1.AppendChild(child1_2)

        child1 = xmlDoc.CreateElement("CdtrAgt")
        PmtInf.AppendChild(child1)

        child1_1 = xmlDoc.CreateElement("FinInstnId")
        child1.AppendChild(child1_1)

        child1_2 = xmlDoc.CreateElement("BIC")              '' <-- BIC code payer
        child1_2.InnerText = pss_bic
        child1_1.AppendChild(child1_2)

        child1 = xmlDoc.CreateElement("ChrgBr")
        child1.InnerText = "SLEV"
        PmtInf.AppendChild(child1)

        '' Start the loop
        '-----------------
        Dim tempTotalTransactions As Decimal = 0     '' getting values from function, needs to reset for second loop
        Dim tempCountTransaction As Integer = 0      '' getting values from function, needs to reset for second loop 
        Dim tempErrorTransaction As Integer = 0      '' getting values from function, needs to reset for second loop 

        For Each oRS As DataRow In customer_data.Rows
            '' set the DrctDbtTxInf xml element
            '----------------------------------
            If oRS("cps_seqtp") = "FRST" Then
                CreateDrctDbtTxInf(xmlDoc, PmtInf, oRS, tempErrorTransaction, tempTotalTransactions, tempCountTransaction)
            End If
            ' end DrctDbtTxInf
        Next

        iTotaalTransaction = iTotaalTransaction + tempTotalTransactions
        iCountTransactions = iCountTransactions + tempCountTransaction - tempErrorTransaction

        '' select node to add the checks elements
        Dim node = PmtInf.SelectSingleNode("./PmtMtd")

        child1 = xmlDoc.CreateElement("CtrlSum")            '' <-- Total payment sum
        child1.InnerText = Replace(tempTotalTransactions, ",", ".")
        PmtInf.InsertAfter(child1, node)

        child1 = xmlDoc.CreateElement("NbOfTxs")            '' <-- Check the amount of items in batch
        child1.InnerText = tempCountTransaction - tempErrorTransaction
        PmtInf.InsertAfter(child1, node)
        '' End first loop


        '' set het PmtInf xml element
        '----------------------------
        PmtInf = xmlDoc.CreateElement("PmtInf")
        painEl.AppendChild(PmtInf)

        child1 = xmlDoc.CreateElement("PmtInfId")           '' <-- Unique id (Using the same like MsgId)
        child1.InnerText = msgID & "RCUR"
        PmtInf.AppendChild(child1)

        child1 = xmlDoc.CreateElement("PmtMtd")             '' <-- PaymentMethod (always DD = DirectDebet)
        child1.InnerText = "DD"
        PmtInf.AppendChild(child1)

        '' Adding the PmtTpInf xml element
        '---------------------------------
        PmtTpInf = xmlDoc.CreateElement("PmtTpInf")         '' PaymentTypeInformation
        PmtInf.AppendChild(PmtTpInf)

        child1_1 = xmlDoc.CreateElement("SvcLvl")           '' <-- ServiceLevel (nodig voor latere implementatie) (verplicht bij rabobank)
        PmtTpInf.AppendChild(child1_1)

        child1_2 = xmlDoc.CreateElement("Cd")
        child1_2.InnerText = "SEPA"
        child1_1.AppendChild(child1_2)

        child1_1 = xmlDoc.CreateElement("LclInstrm")        '' <-- LocalInstrument (nodig voor latere implementatie)
        PmtTpInf.AppendChild(child1_1)

        child1_2 = xmlDoc.CreateElement("Cd")
        child1_2.InnerText = "CORE"                         '' <-- Use only CORE of B2B (B2B needs a signed contract for customer and know by the bank)
        child1_1.AppendChild(child1_2)

        child1_1 = xmlDoc.CreateElement("SeqTp")            '' <-- SequenceType Allowed codes: FRST, RCUR, OOFF, FNAL
        child1_1.InnerText = "RCUR"
        PmtTpInf.AppendChild(child1_1)

        child1 = xmlDoc.CreateElement("ReqdColltnDt")       '' <-- date and time when the batch should executed (could be in the future, so the bank will keep the payments on pending)
        child1.InnerText = FormatSEPADate(cFunctions.GetLocalDateTimeFromUTC(System.DateTime.UtcNow))
        PmtInf.AppendChild(child1)

        child1 = xmlDoc.CreateElement("Cdtr")               '' <-- Required for ABN en Rabo (not according the standard SEPA)
        PmtInf.AppendChild(child1)

        child1_1 = xmlDoc.CreateElement("Nm")               '' <-- Name payer
        child1_1.InnerText = pss_contractname
        child1.AppendChild(child1_1)

        child1 = xmlDoc.CreateElement("CdtrAcct")           '' <-- CreditorAccount
        PmtInf.AppendChild(child1)

        child1_1 = xmlDoc.CreateElement("Id")
        child1.AppendChild(child1_1)

        child1_2 = xmlDoc.CreateElement("IBAN")             '' <-- IBAN number payer
        child1_2.InnerText = pss_iban.ToUpper
        child1_1.AppendChild(child1_2)

        child1 = xmlDoc.CreateElement("CdtrAgt")            '' <-- CreditorAgent
        PmtInf.AppendChild(child1)

        child1_1 = xmlDoc.CreateElement("FinInstnId")       '' <-- FinancialInstitutionIdentification
        child1.AppendChild(child1_1)

        child1_2 = xmlDoc.CreateElement("BIC")              '' <-- BIC code payer
        child1_2.InnerText = pss_bic.ToUpper
        child1_1.AppendChild(child1_2)

        child1 = xmlDoc.CreateElement("ChrgBr")             '' <-- ChargeBearer
        child1.InnerText = "SLEV"
        PmtInf.AppendChild(child1)

        '' Start the loop
        '----------------
        tempTotalTransactions = 0     '' getting values from function, needs to reset for second loop
        tempCountTransaction = 0      '' getting values from function, needs to reset for second loop 
        tempErrorTransaction = 0      '' getting values from function, needs to reset for second loop 

        For Each oRS As DataRow In customer_data.Rows

            '' Create and adding the DrctDbtTxInf xml element
            '------------------------------------------------
            If oRS("cps_seqtp") = "RCUR" Then
                CreateDrctDbtTxInf(xmlDoc, PmtInf, oRS, tempErrorTransaction, tempTotalTransactions, tempCountTransaction)
            End If
            ' eind DrctDbtTxInf
        Next

        iTotaalTransaction = iTotaalTransaction + tempTotalTransactions
        iCountTransactions = iCountTransactions + tempCountTransaction - tempErrorTransaction

        '' adding the amount count and sum of the transactions in batch.
        ' Use insertBefore to make sure it has put on the right position in XML path

        '' select node to add the checks elements
        node = PmtInf.SelectSingleNode("./PmtMtd")
        child1 = xmlDoc.CreateElement("CtrlSum")            '' <-- Total payment sum
        child1.InnerText = Replace(tempTotalTransactions, ",", ".")
        PmtInf.InsertAfter(child1, node)

        child1 = xmlDoc.CreateElement("NbOfTxs")            '' <-- Check the amount of items in batch
        child1.InnerText = tempCountTransaction - tempErrorTransaction
        PmtInf.InsertAfter(child1, node)
        '' end second loop

        '' Saving the total sum of transactions
        SumAmount = iTotaalTransaction
        SumTransActions = iCountTransactions

        '' Create Groep Header element
        '-----------------------------
        Dim GrpHdr As XmlElement
        GrpHdr = xmlDoc.CreateElement("GrpHdr")                     '' <-- GroupHeader 

        '' Adding childs in GrpHdr
        '-------------------------
        child1 = xmlDoc.CreateElement("MsgId")                      '' <-- uniek batch id payment_sepa_batches id
        child1.InnerText = msgID
        GrpHdr.AppendChild(child1)

        child1 = xmlDoc.CreateElement("CreDtTm")                    '' <-- generatie datum
        child1.InnerText = FormatSEPADate(System.DateTime.UtcNow) & "T" & FormatSEPATime(System.DateTime.UtcNow)
        GrpHdr.AppendChild(child1)

        child1 = xmlDoc.CreateElement("NbOfTxs")                    '' <-- Total count of items in batch (total of REQ and FRST batch)
        child1.InnerText = SumTransActions
        GrpHdr.AppendChild(child1)

        child1 = xmlDoc.CreateElement("CtrlSum")                    '' <-- Total payment sum in batch (total of REQ and FRST batch)
        child1.InnerText = Replace(SumAmount, ",", ".")
        GrpHdr.AppendChild(child1)

        child1 = xmlDoc.CreateElement("InitgPty")                   '' <-- Details payer
        GrpHdr.AppendChild(child1)

        child1_1 = xmlDoc.CreateElement("Nm")
        child1_1.InnerText = pss_contractname
        child1.AppendChild(child1_1)

        '' Adding group header to the PmtInf elements
        '--------------------------------------------
        painEl.InsertBefore(GrpHdr, painEl.ChildNodes(0))

        '' Merge all the XML files data
        '-----------------------------
        Dim newPI As XmlProcessingInstruction
        newPI = xmlDoc.CreateProcessingInstruction("xml", "version=""1.0"" encoding=""utf-8""")
        xmlDoc.InsertBefore(newPI, xmlDoc.ChildNodes(0))

        Return xmlDoc.OuterXml

    End Function

    ''' <summary>
    ''' This sub generate the batches containing the payment details
    ''' </summary>
    ''' <param name="p_XmlObject"></param>
    ''' <param name="p_PmtInfObject"></param>
    ''' <param name="p_RecordSetObject"></param>
    ''' <param name="p_ErrorCount">If there is an error on one of the item, return in here so the amount can be updated</param>
    ''' <param name="p_TotaalTransaction"></param>
    ''' <param name="p_CountTransactions"></param>
    Sub CreateDrctDbtTxInf(ByVal p_XmlObject As XmlDocument,
                           ByVal p_PmtInfObject As XmlElement,
                           ByVal p_RecordSetObject As DataRow,
                           ByRef p_ErrorCount As Integer,
                           ByRef p_TotaalTransaction As Decimal,
                           ByRef p_CountTransactions As Integer)

        Dim oFactuurSum As New cInvoice

        '' Sometimes the customer does not leave the complete details.
        ' If so remove from batch and send out an e-mail to vendor.

        '' TIP: add addition check for validation IBAN number Or cIban.VALIDATEIBAN(p_RecordSetObject("cps_iban").ToString) > 0 
        If p_RecordSetObject("cps_iban").ToString.Length = 0 Or
           p_RecordSetObject("cps_bic").ToString.Length = 0 Or
           p_RecordSetObject("cps_name").ToString.Length = 0 Or
           p_RecordSetObject("cps_status") = 0 Then

            '' something is wrong, give error or do something

            p_ErrorCount = p_ErrorCount + 1
        Else
            p_TotaalTransaction = p_TotaalTransaction + p_RecordSetObject("invoice_total")
            p_CountTransactions = p_CountTransactions + 1

            '' Add the DrctDbtTxInf xml element
            '----------------------------------
            Dim DrctDbtTxInf, child1, child1_1, DrctDbtTx As XmlElement
            Dim child1_3, child1_2, child1_4, child1_5 As XmlElement

            DrctDbtTxInf = p_XmlObject.CreateElement("DrctDbtTxInf")        '' <-- DirectDebitTransactionInformation
            p_PmtInfObject.AppendChild(DrctDbtTxInf)

            child1 = p_XmlObject.CreateElement("PmtId")                     '' <-- PaymentIdentification
            DrctDbtTxInf.AppendChild(child1)

            child1_1 = p_XmlObject.CreateElement("EndToEndId")              '' <-- EndToEndIdentification <-- Invoice number (Unique ID)
            child1_1.InnerText = RemoveIlegalChars(p_RecordSetObject("customer_end_to_end_id"))
            child1.AppendChild(child1_1)

            child1 = p_XmlObject.CreateElement("InstdAmt")                  '' <-- InstructedAmount (the payment balance)
            child1.InnerText = Replace(p_RecordSetObject("invoice_total"), ",", ".")
            child1.SetAttribute("Ccy", "EUR")                                '' <-- valuta symbol
            DrctDbtTxInf.AppendChild(child1)

            'Set child1 = xmlDoc.createElement("ChrgBr")					'' <-- ChargeBearer (komt ook voor in PmtInf (rababank accepteerd dit veld niet wanneer binnnen PmtInf gedefineerd))
            'child1.Text = "SLEV"
            'DrctDbtTxInf.appendChild child1

            DrctDbtTx = p_XmlObject.CreateElement("DrctDbtTx")              '' <-- DirectDebitTransaction
            DrctDbtTxInf.AppendChild(DrctDbtTx)

            child1 = p_XmlObject.CreateElement("MndtRltdInf")               '' <-- MandateRelatedInformation
            DrctDbtTx.AppendChild(child1)

            child1_1 = p_XmlObject.CreateElement("MndtId")                  '' <-- MandateIdentification
            'child1_1.Text = pss_incassantid 								' p_RecordSetObject("cu_id") op last van de BANK ook hier het incassantID opgeven
            child1_1.InnerText = RemoveIlegalChars(p_RecordSetObject("customer_id"))          ' en weer terug naar het klantid
            child1.AppendChild(child1_1)

            child1_1 = p_XmlObject.CreateElement("DtOfSgntr")               '' <-- DateOfSignature
            child1_1.InnerText = FormatSEPADate(pss_mandaatdate)
            child1.AppendChild(child1_1)

            child1 = p_XmlObject.CreateElement("CdtrSchmeId")               '' <-- CreditorSchemeIdentification
            DrctDbtTx.AppendChild(child1)

            child1_1 = p_XmlObject.CreateElement("Id")
            child1.AppendChild(child1_1)

            child1_2 = p_XmlObject.CreateElement("PrvtId")                  '' <-- PrivateIdentification
            child1_1.AppendChild(child1_2)

            child1_3 = p_XmlObject.CreateElement("Othr")                    '' <-- Other
            child1_2.AppendChild(child1_3)

            child1_4 = p_XmlObject.CreateElement("Id")
            child1_4.InnerText = pss_incassantid
            child1_3.AppendChild(child1_4)

            child1_4 = p_XmlObject.CreateElement("SchmeNm")                 '' <-- SchemeName
            child1_3.AppendChild(child1_4)

            child1_5 = p_XmlObject.CreateElement("Prtry")                   '' <-- Proprietary
            child1_5.InnerText = "SEPA"
            child1_4.AppendChild(child1_5)

            child1 = p_XmlObject.CreateElement("DbtrAgt")
            DrctDbtTxInf.AppendChild(child1)

            child1_1 = p_XmlObject.CreateElement("FinInstnId")
            child1.AppendChild(child1_1)

            child1_2 = p_XmlObject.CreateElement("BIC")                                 '' <-- BIC code from payer
            child1_2.InnerText = trimtrim(FixNull(p_RecordSetObject("cps_bic"))).ToUpper
            child1_1.AppendChild(child1_2)

            child1 = p_XmlObject.CreateElement("Dbtr")
            DrctDbtTxInf.AppendChild(child1)

            child1_1 = p_XmlObject.CreateElement("Nm")                                  '' <-- name from payer, max 70 char.
            child1_1.InnerText = RemoveIlegalChars(Left(p_RecordSetObject("cps_name"), 70))
            child1.AppendChild(child1_1)

            child1_1 = p_XmlObject.CreateElement("PstlAdr")                             '' <-- Full name from customer
            child1.AppendChild(child1_1)

            child1_2 = p_XmlObject.CreateElement("Ctry")                                '' <- Country code customer
            child1_2.InnerText = "NL"
            child1_1.AppendChild(child1_2)

            child1_2 = p_XmlObject.CreateElement("AdrLine")                             '' <- address line customer
            child1_2.InnerText = RemoveIlegalChars(p_RecordSetObject("customer_street").ToString)
            child1_1.AppendChild(child1_2)

            child1_2 = p_XmlObject.CreateElement("AdrLine")                             '' <- address line customer
            child1_2.InnerText = RemoveIlegalChars(p_RecordSetObject("customer_zip") & " " & p_RecordSetObject("customer_city"))
            child1_1.AppendChild(child1_2)

            child1 = p_XmlObject.CreateElement("DbtrAcct")                                  '' <-- DebtorAgent
            DrctDbtTxInf.AppendChild(child1)

            child1_1 = p_XmlObject.CreateElement("Id")
            child1.AppendChild(child1_1)

            child1_2 = p_XmlObject.CreateElement("IBAN")                            '' <- iban number customer (payer)
            child1_2.InnerText = RemoveIlegalChars(p_RecordSetObject("cps_iban")).ToUpper
            child1_1.AppendChild(child1_2)

            child1 = p_XmlObject.CreateElement("RmtInf")                                    '' <-- RemittanceInformation
            DrctDbtTxInf.AppendChild(child1)

            child1_1 = p_XmlObject.CreateElement("Ustrd")                               '' <-- description, max 140 char
            child1_1.InnerText = RemoveIlegalChars(Left(p_RecordSetObject("invoice_number") & " KL-" & p_RecordSetObject("customer_id") & " " & pss_fixeddesc, 140))
            child1.AppendChild(child1_1)
            ' eind DrctDbtTxInf
        End If

    End Sub

    ''' <summary>
    ''' Needed to format the date into SEPA standard
    ''' </summary>
    ''' <param name="p_Date"></param>
    ''' <returns></returns>
    Private Function FormatSEPADate(ByVal p_Date As Date) As String
        FormatSEPADate = Year(p_Date) & "-" & AddLeadingChar(Month(p_Date), "0", 2) & "-" & AddLeadingChar(Day(p_Date), "0", 2)
    End Function

    ''' <summary>
    ''' Needed to format the time into SEPA standard
    ''' </summary>
    ''' <param name="p_Time"></param>
    ''' <returns></returns>
    Private Function FormatSEPATime(ByVal p_Time As Date) As String
        FormatSEPATime = AddLeadingChar(Hour(p_Time), "0", 2) & ":" & AddLeadingChar(Month(p_Time), "0", 2) & ":" & AddLeadingChar(Second(p_Time), "0", 2)
    End Function

    ''' <summary>
    ''' SEPA XML can only contains this chars: a-zA-Z 0-9 and -?:().,'+
    ''' Remove al other chars from string
    ''' </summary>
    ''' <param name="p_String"></param>
    ''' <returns></returns>
    Private Function RemoveIlegalChars(ByVal p_String As String) As String
        Return RegularExpressions.Regex.Replace(p_String, "[^\a-z\d\s\?\-\:\(\)\.\,]", "")
    End Function

    ''' <summary>
    ''' Encrypt a string into sha1 hash (WARNING: Do not use to store sha1 hashes, it's not save)
    ''' </summary>
    ''' <param name="p_Text"></param>
    ''' <returns></returns>
    Private Shared Function sha1(ByVal p_Text As String) As String
        Dim sha As New System.Security.Cryptography.SHA1Managed()
        Dim ae As New ASCIIEncoding()
        Dim Hash() As Byte = sha.ComputeHash(ae.GetBytes(p_Text))
        Dim sb As New StringBuilder(Hash.Length * 2)
        Dim ndx As Integer
        For ndx = 0 To Hash.Length - 1
            sb.Append(Right("0" & Hex(Hash(ndx)), 2))
        Next
        Return sb.ToString.ToLower
    End Function
End Class

